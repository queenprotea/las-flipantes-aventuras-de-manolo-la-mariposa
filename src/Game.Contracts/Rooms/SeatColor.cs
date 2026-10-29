using System.Runtime.Serialization;

namespace Game.Contracts
{
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
