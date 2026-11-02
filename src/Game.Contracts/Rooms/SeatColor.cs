using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Identifies the color that marks a member of a room and their pieces.
    /// </summary>
    [DataContract]
    public enum SeatColor
    {
        [EnumMember]
        Green,

        [EnumMember]
        Blue,

        [EnumMember]
        Red,

        [EnumMember]
        Yellow,
    }
}
