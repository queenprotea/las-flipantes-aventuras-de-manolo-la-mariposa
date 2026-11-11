using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells which file of an avatar upload a part belongs to.
    /// </summary>
    [DataContract]
    public enum AvatarFileKind
    {
        [EnumMember]
        Original,

        [EnumMember]
        Thumbnail,
    }
}
