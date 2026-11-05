using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a starting piece was placed, or why not.
    /// </summary>
    [DataContract]
    public enum PlacementResult
    {
        [EnumMember]
        Placed,

        [EnumMember]
        NotInMatch,

        [EnumMember]
        WrongPhase,

        [EnumMember]
        NotYourPlacement,

        [EnumMember]
        OutsideBoard,

        [EnumMember]
        NotAFlower,

        [EnumMember]
        FlowerOccupied,

        [EnumMember]
        TimeExpired,

        [EnumMember]
        NotIdentified,
    }
}
