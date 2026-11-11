using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells which part of the match is being played.
    /// </summary>
    [DataContract]
    public enum MatchPhase
    {
        [EnumMember]
        CaterpillarPlacement,

        [EnumMember]
        ButterflyPlacement,

        [EnumMember]
        PlayerTurn,

        [EnumMember]
        Finished,
    }
}
