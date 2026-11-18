using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells which part of the match is being played, or that the match waits for its players to come back after a server restart.
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

        [EnumMember]
        AwaitingPlayers,
    }
}
