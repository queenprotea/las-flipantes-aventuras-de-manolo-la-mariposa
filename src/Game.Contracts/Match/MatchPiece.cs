using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Names the piece a player places on the board.
    /// </summary>
    [DataContract]
    public enum MatchPiece
    {
        [EnumMember]
        Caterpillar,

        [EnumMember]
        Butterfly,
    }
}
