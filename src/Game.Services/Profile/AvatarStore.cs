using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;

namespace Game.Services
{
    public sealed class AvatarStore
    {
        private const string ReferenceFormat = "avatars/{0}";
        private const string AvatarFileNameFormat = "{0}-{1:N}.png";
        private const string ThumbnailFileNameFormat = "{0}-mini.png";

        private readonly string _folderPath;

        public AvatarStore(string folderPath)
        {
            _folderPath = folderPath;
        }

        public void EnsureFolder()
        {
            Directory.CreateDirectory(_folderPath);
        }

        internal string Save(int playerId, CompletedAvatarUpload completedUpload)
        {
            string avatarFileName = string.Format(CultureInfo.InvariantCulture, AvatarFileNameFormat, playerId, Guid.NewGuid());
            File.WriteAllBytes(ToThumbnailPath(avatarFileName), completedUpload.Thumbnail);
            string avatarReference = string.Format(CultureInfo.InvariantCulture, ReferenceFormat, avatarFileName);

            return avatarReference;
        }

        internal async Task<byte[]?> ReadThumbnailAsync(string avatarReference)
        {
            string? avatarFileName = ToFileName(avatarReference);
            string? thumbnailPath = avatarFileName is null ? null : ToThumbnailPath(avatarFileName);
            byte[]? thumbnail = null;
            if ((thumbnailPath is not null) && File.Exists(thumbnailPath))
            {
                thumbnail = await File.ReadAllBytesAsync(thumbnailPath);
            }

            return thumbnail;
        }

        internal void Delete(string avatarReference)
        {
            string? avatarFileName = ToFileName(avatarReference);
            if (avatarFileName is null)
            {
                return;
            }

            File.Delete(Path.Combine(_folderPath, avatarFileName));
            File.Delete(ToThumbnailPath(avatarFileName));
        }

        private static string? ToFileName(string avatarReference)
        {
            string fileName = Path.GetFileName(avatarReference);
            string? safeFileName = fileName.Length == 0 ? null : fileName;

            return safeFileName;
        }

        private string ToThumbnailPath(string avatarFileName)
        {
            string thumbnailFileName = string.Format(CultureInfo.InvariantCulture, ThumbnailFileNameFormat, Path.GetFileNameWithoutExtension(avatarFileName));
            string thumbnailPath = Path.Combine(_folderPath, thumbnailFileName);

            return thumbnailPath;
        }
    }
}
