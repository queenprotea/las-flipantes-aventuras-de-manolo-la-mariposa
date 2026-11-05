using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether the match started, or why not.
    /// </summary>
    [DataContract]
    public enum StartMatchStatus
    {
        [EnumMember]
        Started,

        [EnumMember]
        NotInRoom,

        [EnumMember]
        NotHost,

        [EnumMember]
        NotEnoughPlayers,

        [EnumMember]
        RoomInMatch,

        [EnumMember]
        PlayerBanned,

        [EnumMember]
        NotIdentified,

        [EnumMember]
        DatabaseUnavailable,
    }
}
