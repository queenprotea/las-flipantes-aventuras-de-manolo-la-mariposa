using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract(CallbackContract = typeof(IInvitationCallback))]
    public interface IInvitationService
    {
        [OperationContract(Name = "GetPendingInvitations")]
        Task<PendingInvitationsResponse> GetPendingInvitationsAsync();

        [OperationContract(Name = "SendInvitation")]
        Task<SendInvitationResult> SendInvitationAsync(int targetPlayerId);

        [OperationContract(Name = "RespondInvitation")]
        Task<RespondInvitationResult> RespondInvitationAsync(int invitationId, bool accept);
    }
    
    [ServiceContract]
    public interface IInvitationCallback
    {
        [OperationContract(Name = "OnRoomInvitationReceived", IsOneWay = true)]
        void OnRoomInvitationReceived(RoomInvitation invitation);
    }
}
