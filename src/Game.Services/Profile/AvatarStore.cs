using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Game.Services
{
    public sealed class AvatarStore
    {
        private const string ReferenceFormat = "avatars/{0}";
        private const string OriginalFileNameFormat = "{0}-{1:N}.{2}";
        private const string ThumbnailFileNameFormat = "{0}-mini.png";

        private readonly string _folderPath;

        public AvatarStore(string folderPath)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(folderPath);

            _folderPath = folderPath;
        }

        public void EnsureFolder()
        {
            Directory.CreateDirectory(_folderPath);
        }

        internal string Save(int playerId, CompletedAvatarUpload completedUpload)
        {
            string originalFileName = string.Format(CultureInfo.InvariantCulture, OriginalFileNameFormat, playerId, Guid.NewGuid(), completedUpload.Extension);
            string originalPath = Path.Combine(_folderPath, originalFileName);
            File.WriteAllBytes(originalPath, completedUpload.Original);
            try
            {
                File.WriteAllBytes(ToThumbnailPath(originalFileName), completedUpload.Thumbnail);
            }
            catch (IOException)
            {
                File.Delete(originalPath);
                throw;
            }
            catch (UnauthorizedAccessException)
            {
                File.Delete(originalPath);
                throw;
            }

            string avatarReference = string.Format(CultureInfo.InvariantCulture, ReferenceFormat, originalFileName);

            return avatarReference;
        }

        internal async Task<byte[]?> ReadThumbnailAsync(string avatarReference)
        {
            string? originalFileName = ToFileName(avatarReference);
            string? thumbnailPath = originalFileName is null ? null : ToThumbnailPath(originalFileName);
            byte[]? thumbnail = null;
            if ((thumbnailPath is not null) && File.Exists(thumbnailPath))
            {
                thumbnail = await File.ReadAllBytesAsync(thumbnailPath);
            }

            return thumbnail;
        }

        internal void Delete(string avatarReference)
        {
            string? originalFileName = ToFileName(avatarReference);
            if (originalFileName is null)
            {
                return;
            }

            File.Delete(Path.Combine(_folderPath, originalFileName));
            File.Delete(ToThumbnailPath(originalFileName));
        }

        private static string? ToFileName(string avatarReference)
        {
            string fileName = Path.GetFileName(avatarReference);
            string? safeFileName = fileName.Length == 0 ? null : fileName;

            return safeFileName;
        }

        private string ToThumbnailPath(string originalFileName)
        {
            string thumbnailFileName = string.Format(CultureInfo.InvariantCulture, ThumbnailFileNameFormat, Path.GetFileNameWithoutExtension(originalFileName));
            string thumbnailPath = Path.Combine(_folderPath, thumbnailFileName);

            return thumbnailPath;
        }
    }
}
