using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether a placement or a turn action was applied, or why not.
    /// </summary>
    [DataContract]
    public enum MatchActionResult
    {
        [EnumMember]
        Applied,

        [EnumMember]
        NotIdentified,

        [EnumMember]
        NotInMatch,

        [EnumMember]
        OutsideBoard,

        [EnumMember]
        TimeExpired,

        [EnumMember]
        WrongPhase,

        [EnumMember]
        NotYourTurn,

        [EnumMember]
        NotAFlower,

        [EnumMember]
        CellOccupied,

        [EnumMember]
        NotEnoughActionPoints,

        [EnumMember]
        NoCaterpillarsLeft,

        [EnumMember]
        NotYourCaterpillar,

        [EnumMember]
        NotNeighborCell,

        [EnumMember]
        ClimbTooHigh,

        [EnumMember]
        FlowerAtMaximumLevel,

        [EnumMember]
        NoBuildsLeft,

        [EnumMember]
        NotNextToPlanter,

        [EnumMember]
        JoinsPlanters,
    }
}
