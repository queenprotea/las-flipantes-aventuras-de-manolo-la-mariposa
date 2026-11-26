using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework.Graphics;

using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;

using StbImageSharp;

using Torres.Client.Connection;
using Torres.Client.Threading;

namespace Torres.Client.Avatars
{
    internal sealed class AvatarCache : IDisposable
    {
        private const string ApplicationFolderName = "Torres";
        private const string AvatarsFolderName = "avatars";
        private const string ThumbnailFileNameFormat = "{0}-mini.png";
        private const string ThumbnailFilePattern = "*-mini.png";
        private const string TemporaryExtension = ".tmp";
        private const string TemporaryFilePattern = "*.tmp";
        private const long LargestCacheBytes = 50L * 1024 * 1024;
        private const int MostDownloads = 4;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly string _folderPath;
        private readonly Texture2D _defaultTexture;
        private readonly Task _folderPreparation;
        private readonly Dictionary<string, TextureRegion> _imagesByReference = new Dictionary<string, TextureRegion>(StringComparer.Ordinal);
        private readonly Dictionary<string, Task<AvatarResponse>> _downloadsByReference = new Dictionary<string, Task<AvatarResponse>>(StringComparer.Ordinal);
        private readonly Dictionary<string, Task<ImageResult>> _decodingsByReference = new Dictionary<string, Task<ImageResult>>(StringComparer.Ordinal);
        private readonly Dictionary<string, byte[]> _downloadedThumbnailsByReference = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        private readonly Queue<ThumbnailFile> _pendingCacheWrites = new Queue<ThumbnailFile>();
        private readonly Queue<string> _queuedReferences = new Queue<string>();
        private readonly HashSet<string> _waitingReferences = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _missingReferences = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _unavailableReferences = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _unreadableCachedReferences = new HashSet<string>(StringComparer.Ordinal);

        private Texture2D? _previewTexture;
        private Task _cacheWriting = Task.CompletedTask;
        private bool _isCacheWritable = true;

        internal AvatarCache(GraphicsDevice graphicsDevice, GameSessionConnection gameSessionConnection)
        {
            _graphicsDevice = graphicsDevice;
            _gameSessionConnection = gameSessionConnection;
            _folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationFolderName, AvatarsFolderName);
            _folderPreparation = Task.Run(PrepareFolder);
            _defaultTexture = DefaultAvatarArt.Create(graphicsDevice);
            DefaultImage = new TextureRegion(_defaultTexture);
        }

        internal IImage DefaultImage { get; }

        internal IImage FindImage(string? avatarReference)
        {
            IImage avatarImage = DefaultImage;
            if (string.IsNullOrWhiteSpace(avatarReference))
            {
                return avatarImage;
            }

            TextureRegion? loadedImage = _imagesByReference.GetValueOrDefault(avatarReference);
            if (loadedImage is null)
            {
                Request(avatarReference);
            }
            else
            {
                avatarImage = loadedImage;
            }

            return avatarImage;
        }

        internal bool IsMissing(string? avatarReference)
        {
            bool isMissing = (avatarReference is not null) && _missingReferences.Contains(avatarReference);

            return isMissing;
        }

        internal void Update()
        {
            if (!_gameSessionConnection.IsSessionOpen)
            {
                _unavailableReferences.Clear();
            }

            FinishDownloads();
            FinishDecodings();
            FollowCacheWriting();
            StartQueued();
        }

        internal void Put(string avatarReference, byte[] thumbnail)
        {
            ShowImage(avatarReference, LoadImage(thumbnail));
            QueueCacheWrite(avatarReference, thumbnail);
        }

        internal IImage CreatePreview(byte[] thumbnail)
        {
            _previewTexture?.Dispose();
            using var thumbnailStream = new MemoryStream(thumbnail);
            _previewTexture = Texture2D.FromStream(_graphicsDevice, thumbnailStream);
            var previewImage = new TextureRegion(_previewTexture);

            return previewImage;
        }

        public void Dispose()
        {
            foreach (TextureRegion loadedImage in _imagesByReference.Values)
            {
                loadedImage.Texture.Dispose();
            }

            _imagesByReference.Clear();
            _previewTexture?.Dispose();
            _defaultTexture.Dispose();
        }

        private void Request(string avatarReference)
        {
            bool isAlreadyRequested = _waitingReferences.Contains(avatarReference)
                || _missingReferences.Contains(avatarReference)
                || _unavailableReferences.Contains(avatarReference);
            if (isAlreadyRequested)
            {
                return;
            }

            _waitingReferences.Add(avatarReference);
            _queuedReferences.Enqueue(avatarReference);
        }

        private void FinishDownloads()
        {
            List<string> finishedReferences = _downloadsByReference
                .Where(downloadEntry => downloadEntry.Value.IsCompleted)
                .Select(downloadEntry => downloadEntry.Key)
                .ToList();
            foreach (string finishedReference in finishedReferences)
            {
                Task<AvatarResponse> finishedDownload = _downloadsByReference[finishedReference];
                _downloadsByReference.Remove(finishedReference);
                _waitingReferences.Remove(finishedReference);
                FollowDownload(finishedReference, finishedDownload);
            }
        }

        private void FollowDownload(string avatarReference, Task<AvatarResponse> finishedDownload)
        {
            if (!finishedDownload.IsCompletedSuccessfully)
            {
                _unavailableReferences.Add(avatarReference);
            }
            else if ((finishedDownload.Result.Status == AvatarLookupStatus.Found) && (finishedDownload.Result.Image is byte[] thumbnail))
            {
                _waitingReferences.Add(avatarReference);
                _downloadedThumbnailsByReference[avatarReference] = thumbnail;
                _decodingsByReference[avatarReference] = new BackgroundCall<byte[], ImageResult>(DecodeThumbnail, thumbnail).StartAsync();
            }
            else
            {
                _missingReferences.Add(avatarReference);
            }
        }

        private void StartQueued()
        {
            while ((_downloadsByReference.Count < MostDownloads) && (_queuedReferences.Count > 0))
            {
                string queuedReference = _queuedReferences.Peek();
                string cachedPath = ToCachedPath(queuedReference);
                if (!IsCached(queuedReference, cachedPath) && !_gameSessionConnection.IsSessionOpen)
                {
                    break;
                }

                _queuedReferences.Dequeue();
                StartLoading(queuedReference, cachedPath);
            }
        }

        private void StartLoading(string avatarReference, string cachedPath)
        {
            if (IsCached(avatarReference, cachedPath))
            {
                _decodingsByReference[avatarReference] = new BackgroundCall<string, ImageResult>(ReadCachedThumbnail, cachedPath).StartAsync();
            }
            else
            {
                _downloadsByReference[avatarReference] = _gameSessionConnection.GetAvatarThumbnailAsync(avatarReference);
            }
        }

        private bool IsCached(string avatarReference, string cachedPath)
        {
            bool isCached = !_unreadableCachedReferences.Contains(avatarReference) && File.Exists(cachedPath);

            return isCached;
        }

        private void FinishDecodings()
        {
            List<string> decodedReferences = _decodingsByReference
                .Where(decodingEntry => decodingEntry.Value.IsCompleted)
                .Select(decodingEntry => decodingEntry.Key)
                .ToList();
            foreach (string decodedReference in decodedReferences)
            {
                Task<ImageResult> finishedDecoding = _decodingsByReference[decodedReference];
                _decodingsByReference.Remove(decodedReference);
                _waitingReferences.Remove(decodedReference);
                FollowDecoding(decodedReference, finishedDecoding);
            }
        }

        private void FollowDecoding(string avatarReference, Task<ImageResult> finishedDecoding)
        {
            bool wasDownloaded = _downloadedThumbnailsByReference.Remove(avatarReference, out byte[]? downloadedThumbnail);
            if (!finishedDecoding.IsCompletedSuccessfully)
            {
                RecordUnreadable(avatarReference, wasDownloaded);
                return;
            }

            ShowImage(avatarReference, CreateImage(finishedDecoding.Result));
            if (downloadedThumbnail is not null)
            {
                QueueCacheWrite(avatarReference, downloadedThumbnail);
            }
        }

        private void RecordUnreadable(string avatarReference, bool wasDownloaded)
        {
            if (wasDownloaded)
            {
                _unavailableReferences.Add(avatarReference);
            }
            else
            {
                _unreadableCachedReferences.Add(avatarReference);
                Request(avatarReference);
            }
        }

        private void ShowImage(string avatarReference, TextureRegion avatarImage)
        {
            TextureRegion? replacedImage = _imagesByReference.GetValueOrDefault(avatarReference);
            replacedImage?.Texture.Dispose();

            _missingReferences.Remove(avatarReference);
            _imagesByReference[avatarReference] = avatarImage;
        }

        private TextureRegion CreateImage(ImageResult decodedThumbnail)
        {
            var thumbnailTexture = new Texture2D(_graphicsDevice, decodedThumbnail.Width, decodedThumbnail.Height);
            thumbnailTexture.SetData(decodedThumbnail.Data);
            var thumbnailImage = new TextureRegion(thumbnailTexture);

            return thumbnailImage;
        }

        private TextureRegion LoadImage(byte[] thumbnail)
        {
            using var thumbnailStream = new MemoryStream(thumbnail);
            Texture2D thumbnailTexture = Texture2D.FromStream(_graphicsDevice, thumbnailStream);
            var thumbnailImage = new TextureRegion(thumbnailTexture);

            return thumbnailImage;
        }

        private void QueueCacheWrite(string avatarReference, byte[] thumbnail)
        {
            if (_isCacheWritable)
            {
                _pendingCacheWrites.Enqueue(new ThumbnailFile(ToCachedPath(avatarReference), thumbnail));
            }
        }

        private void FollowCacheWriting()
        {
            bool hasFailed = (_folderPreparation.IsCompleted && !_folderPreparation.IsCompletedSuccessfully)
                || (_cacheWriting.IsCompleted && !_cacheWriting.IsCompletedSuccessfully);
            if (hasFailed)
            {
                _isCacheWritable = false;
                _pendingCacheWrites.Clear();
            }

            bool canStartWrite = _folderPreparation.IsCompletedSuccessfully && _cacheWriting.IsCompleted && (_pendingCacheWrites.Count > 0);
            if (canStartWrite)
            {
                _cacheWriting = new BackgroundWork<ThumbnailFile>(WriteToCache, _pendingCacheWrites.Dequeue()).StartAsync();
            }
        }

        private static ImageResult ReadCachedThumbnail(string cachedPath)
        {
            File.SetLastAccessTimeUtc(cachedPath, DateTime.UtcNow);
            ImageResult decodedThumbnail = DecodeThumbnail(File.ReadAllBytes(cachedPath));

            return decodedThumbnail;
        }

        private static ImageResult DecodeThumbnail(byte[] thumbnail)
        {
            ImageResult decodedThumbnail = ImageResult.FromMemory(thumbnail, ColorComponents.RedGreenBlueAlpha);
            DefaultColorProcessors.ZeroTransparentPixels(decodedThumbnail.Data);

            return decodedThumbnail;
        }

        private string ToCachedPath(string avatarReference)
        {
            string thumbnailFileName = string.Format(CultureInfo.InvariantCulture, ThumbnailFileNameFormat, Path.GetFileNameWithoutExtension(avatarReference));
            string cachedPath = Path.Combine(_folderPath, thumbnailFileName);

            return cachedPath;
        }

        private void PrepareFolder()
        {
            Directory.CreateDirectory(_folderPath);
            foreach (string temporaryPath in Directory.GetFiles(_folderPath, TemporaryFilePattern))
            {
                File.Delete(temporaryPath);
            }
        }

        private void WriteToCache(ThumbnailFile thumbnailFile)
        {
            string temporaryPath = thumbnailFile.FilePath + TemporaryExtension;
            File.WriteAllBytes(temporaryPath, thumbnailFile.Contents);
            File.Move(temporaryPath, thumbnailFile.FilePath, true);
            TrimToLimit();
        }

        private void TrimToLimit()
        {
            List<FileInfo> cachedFiles = new DirectoryInfo(_folderPath)
                .GetFiles(ThumbnailFilePattern)
                .OrderBy(cachedFile => cachedFile.LastAccessTimeUtc)
                .ToList();
            long cachedTotalBytes = cachedFiles.Sum(cachedFile => cachedFile.Length);
            foreach (FileInfo oldestFile in cachedFiles)
            {
                if (cachedTotalBytes <= LargestCacheBytes)
                {
                    break;
                }

                cachedTotalBytes -= oldestFile.Length;
                oldestFile.Delete();
            }
        }
    }
}
