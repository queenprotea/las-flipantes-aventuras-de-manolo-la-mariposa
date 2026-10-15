using System.ServiceModel;
using System.Threading.Tasks;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IFriendsService
    {
        [OperationContract(Name = "GetFriendsOverview")]
        Task<FriendsOverviewContract> GetFriendsOverviewAsync();

        [OperationContract(Name = "FindPlayerByUsername")]
        Task<PlayerSearchContract> FindPlayerByUsernameAsync(string username);

        [OperationContract(Name = "SendFriendRequest")]
        Task<FriendRequestResult> SendFriendRequestAsync(int targetPlayerId);

        [OperationContract(Name = "RespondFriendRequest")]
        Task<FriendRequestResponseResult> RespondFriendRequestAsync(int requesterPlayerId, bool accept);

        [OperationContract(Name = "RemoveFriend")]
        Task<FriendRemovalResult> RemoveFriendAsync(int friendPlayerId);
    }
}
