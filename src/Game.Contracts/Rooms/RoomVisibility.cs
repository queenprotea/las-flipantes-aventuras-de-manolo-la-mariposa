using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a room appears in the public list or is entered only with its code.
    /// </summary>
    [DataContract]
    public enum RoomVisibility
    {
        [EnumMember]
        Public,

        [EnumMember]
        Private,
    }
}
