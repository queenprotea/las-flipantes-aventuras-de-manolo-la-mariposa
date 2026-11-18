using System.Runtime.Serialization;

namespace Game.Contracts
{
    /// <summary>
    /// Tells whether the caller left its match by forfeiting, or why not.
    /// </summary>
    [DataContract]
    public enum ForfeitResult
    {
        [EnumMember]
        Forfeited,

        [EnumMember]
        NotInMatch,

        [EnumMember]
        NotIdentified,
    }
}
