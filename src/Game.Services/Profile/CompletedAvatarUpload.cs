namespace Game.Services
{
    internal sealed record CompletedAvatarUpload(byte[] Original, string Extension, byte[] Thumbnail);
}
