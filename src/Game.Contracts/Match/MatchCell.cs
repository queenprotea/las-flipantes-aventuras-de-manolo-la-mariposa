using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Names a cell of the board by its row and column, both from 0 to 7.
    /// </summary>
    [DataContract]
    public sealed class MatchCell
    {
        [DataMember]
        public int Row { get; set; }

        [DataMember]
        public int Column { get; set; }
    }
}
