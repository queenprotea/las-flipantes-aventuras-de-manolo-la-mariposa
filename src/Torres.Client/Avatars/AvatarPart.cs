using Game.Contracts;

namespace Torres.Client.Avatars
{
    internal sealed record AvatarPart(AvatarFileKind FileKind, int Offset, byte[] Bytes);
}
