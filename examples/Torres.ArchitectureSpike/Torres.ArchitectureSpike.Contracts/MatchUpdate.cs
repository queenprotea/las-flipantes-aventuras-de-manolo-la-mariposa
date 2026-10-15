using System.Runtime.Serialization;

namespace Torres.ArchitectureSpike.Contracts
{
    [DataContract]
    public sealed class MatchUpdate
    {
        [DataMember]
        public MatchEventCode EventCode { get; set; }

        [DataMember]
        public int TokenPosition { get; set; }

        [DataMember]
        public int GoalPosition { get; set; }

        [DataMember]
        public int SeatInTurn { get; set; }

        [DataMember]
        public int RecipientSeat { get; set; }

        [DataMember]
        public int WinnerSeat { get; set; }
    }
}
