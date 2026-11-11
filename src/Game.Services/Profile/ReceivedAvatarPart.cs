using Game.Contracts;

namespace Game.Services
{
    internal sealed record ReceivedAvatarPart(AvatarFileKind FileKind, int Offset, byte[] Bytes);
}
