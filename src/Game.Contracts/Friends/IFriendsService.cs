using System.ServiceModel;

namespace Game.Contracts
{
    [ServiceContract]
    public interface IFriendsService
    {
        [OperationContract(Name = "GetFriendsOverview", IsInitiating = false)]
        FriendsOverviewResponse GetFriendsOverview();

        [OperationContract(Name = "FindPlayerByUsername", IsInitiating = false)]
        PlayerSearchResponse FindPlayerByUsername(string username);

        [OperationContract(Name = "SendFriendRequest", IsInitiating = false)]
        FriendRequestResult SendFriendRequest(int targetPlayerId);

        [OperationContract(Name = "RespondFriendRequest", IsInitiating = false)]
        FriendRequestResponseResult RespondFriendRequest(int requesterPlayerId, bool accept);

        [OperationContract(Name = "RemoveFriend", IsInitiating = false)]
        FriendRemovalResult RemoveFriend(int friendPlayerId);
    }
}
