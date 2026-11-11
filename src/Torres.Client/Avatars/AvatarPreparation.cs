namespace Torres.Client.Avatars
{
    internal sealed record AvatarPreparation(AvatarPreparationStatus Status, PreparedAvatar? Avatar, long FileBytes);
}
