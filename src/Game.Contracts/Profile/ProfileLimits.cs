namespace Game.Contracts
{
    /// <summary>
    /// Groups the sizes that the client and the server must agree on to move avatars.
    /// </summary>
    public static class ProfileLimits
    {
        public const int LargestAvatarBytes = 5 * 1024 * 1024;

        public const int AvatarPartBytes = 48 * 1024;

        public const int LargestThumbnailBytes = 60 * 1024;

        public const int ThumbnailSide = 128;

        public const int LargestArrayBytes = 64 * 1024;
    }
}
