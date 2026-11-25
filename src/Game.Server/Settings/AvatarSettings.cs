using System;
using System.IO;

using Microsoft.Extensions.Configuration;

namespace Game.Server
{
    public sealed class AvatarSettings
    {
        private const string SectionName = "Avatars";
        private const string InvalidAvatarSettingsMessage = "The Avatars section of appsettings.json needs a Folder and a positive UploadIdleSeconds.";

        public string Folder { get; set; } = string.Empty;

        public int UploadIdleSeconds { get; set; }

        public static AvatarSettings Read(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            AvatarSettings? settings = configuration.GetSection(SectionName).Get<AvatarSettings>();
            if ((settings is null) || string.IsNullOrWhiteSpace(settings.Folder) || (settings.UploadIdleSeconds <= 0))
            {
                throw new InvalidOperationException(InvalidAvatarSettingsMessage);
            }

            return settings;
        }

        public string ResolveFolderPath()
        {
            string folderPath = Path.Combine(AppContext.BaseDirectory, Folder);

            return folderPath;
        }
    }
}
