using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Describes a caterpillar move: the cell where the caterpillar stands and the neighbor cell it goes to.
    /// </summary>
    [DataContract]
    public sealed class MatchMove
    {
        [DataMember]
        public MatchCell Origin { get; set; } = new MatchCell();

        [DataMember]
        public MatchCell Destination { get; set; } = new MatchCell();
    }
}
