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

using Torres.Client.Session;

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
        private readonly Dictionary<string, TextureRegion> _imagesByReference = new Dictionary<string, TextureRegion>(StringComparer.Ordinal);
        private readonly Dictionary<string, Task<AvatarResponse>> _downloadsByReference = new Dictionary<string, Task<AvatarResponse>>(StringComparer.Ordinal);
        private readonly Queue<string> _queuedReferences = new Queue<string>();
        private readonly HashSet<string> _waitingReferences = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _missingReferences = new HashSet<string>(StringComparer.Ordinal);
        private readonly HashSet<string> _unavailableReferences = new HashSet<string>(StringComparer.Ordinal);

        private Texture2D? _previewTexture;

        internal AvatarCache(GraphicsDevice graphicsDevice, GameSessionConnection gameSessionConnection)
        {
            ArgumentNullException.ThrowIfNull(graphicsDevice);
            ArgumentNullException.ThrowIfNull(gameSessionConnection);

            _graphicsDevice = graphicsDevice;
            _gameSessionConnection = gameSessionConnection;
            _folderPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), ApplicationFolderName, AvatarsFolderName);
            Directory.CreateDirectory(_folderPath);
            DeleteTemporaryFiles();
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
            StartQueued();
        }

        internal void Put(string avatarReference, byte[] thumbnail)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(avatarReference);
            ArgumentNullException.ThrowIfNull(thumbnail);

            Store(avatarReference, thumbnail);
        }

        internal IImage CreatePreview(byte[] thumbnail)
        {
            ArgumentNullException.ThrowIfNull(thumbnail);

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
                Store(avatarReference, thumbnail);
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
                if (!File.Exists(cachedPath) && !_gameSessionConnection.IsSessionOpen)
                {
                    break;
                }

                _queuedReferences.Dequeue();
                StartLoading(queuedReference, cachedPath);
            }
        }

        private void StartLoading(string avatarReference, string cachedPath)
        {
            if (File.Exists(cachedPath))
            {
                File.SetLastAccessTimeUtc(cachedPath, DateTime.UtcNow);
                _waitingReferences.Remove(avatarReference);
                _imagesByReference[avatarReference] = LoadImage(File.ReadAllBytes(cachedPath));
            }
            else
            {
                _downloadsByReference[avatarReference] = _gameSessionConnection.GetAvatarThumbnailAsync(avatarReference);
            }
        }

        private void Store(string avatarReference, byte[] thumbnail)
        {
            string cachedPath = ToCachedPath(avatarReference);
            string temporaryPath = cachedPath + TemporaryExtension;
            File.WriteAllBytes(temporaryPath, thumbnail);
            File.Move(temporaryPath, cachedPath, true);
            TextureRegion? replacedImage = _imagesByReference.GetValueOrDefault(avatarReference);
            replacedImage?.Texture.Dispose();

            _missingReferences.Remove(avatarReference);
            _imagesByReference[avatarReference] = LoadImage(thumbnail);
            TrimToLimit();
        }

        private TextureRegion LoadImage(byte[] thumbnail)
        {
            using var thumbnailStream = new MemoryStream(thumbnail);
            Texture2D thumbnailTexture = Texture2D.FromStream(_graphicsDevice, thumbnailStream);
            var thumbnailImage = new TextureRegion(thumbnailTexture);

            return thumbnailImage;
        }

        private string ToCachedPath(string avatarReference)
        {
            string thumbnailFileName = string.Format(CultureInfo.InvariantCulture, ThumbnailFileNameFormat, Path.GetFileNameWithoutExtension(avatarReference));
            string cachedPath = Path.Combine(_folderPath, thumbnailFileName);

            return cachedPath;
        }

        private void DeleteTemporaryFiles()
        {
            foreach (string temporaryPath in Directory.GetFiles(_folderPath, TemporaryFilePattern))
            {
                File.Delete(temporaryPath);
            }
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
