using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Names the eight action cards, in the order of their numbers from 1 to 8.
    /// </summary>
    [DataContract]
    public enum MatchCard
    {
        [EnumMember]
        SixActionPoints,

        [EnumMember]
        SevenActionPoints,

        [EnumMember]
        DoubleClimb,

        [EnumMember]
        GroundLeap,

        [EnumMember]
        FlowerLevelMove,

        [EnumMember]
        Relocation,

        [EnumMember]
        PlanterCrossing,

        [EnumMember]
        FlowerLevelRemoval,
    }
}
