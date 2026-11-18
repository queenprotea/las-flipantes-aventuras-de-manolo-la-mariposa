using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells that the player holds a seat in a match: the code of the room the match started from, and whether the match was interrupted by a server restart and waits for its players to come back.
    /// </summary>
    [DataContract]
    public sealed class MatchSeating
    {
        [DataMember]
        public string RoomCode { get; set; } = string.Empty;

        [DataMember]
        public bool IsAwaitingPlayers { get; set; }
    }
}
