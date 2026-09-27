using System.Runtime.Serialization;

namespace Game.Contracts
{
    [DataContract]
    public sealed class RankingEntryContract
    {
        [DataMember]
        public string PlayerName { get; set; } = string.Empty;

        [DataMember]
        public int Wins { get; set; }

        [DataMember]
        public int Points { get; set; }

        [DataMember]
        public int Matches { get; set; }
    }
}
