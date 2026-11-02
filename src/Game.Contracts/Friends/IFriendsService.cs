using System.ServiceModel;

namespace Game.Contracts
{
    /// <summary>
    /// Manages the friendships between players with an account; guests cannot use it.
    /// </summary>
    [ServiceContract]
    public interface IFriendsService
    {
        /// <summary>
        /// Lists the caller's friends and the friend requests the caller received and sent.
        /// </summary>
        /// <returns>The caller's friends, received requests and sent requests.</returns>
        /// <exception cref="FaultException">The friendships could not be read.</exception>
        [OperationContract(Name = "GetFriendsOverview", IsInitiating = false)]
        FriendsOverviewResponse GetFriendsOverview();

        /// <summary>
        /// Looks for the account with the given username and tells how it relates to the caller.
        /// </summary>
        /// <param name="username">Username to look for; surrounding spaces are ignored.</param>
        /// <returns>The player found and their relationship with the caller, or that nobody has that username.</returns>
        /// <exception cref="FaultException">The search could not be completed.</exception>
        [OperationContract(Name = "FindPlayerByUsername", IsInitiating = false)]
        PlayerSearchResponse FindPlayerByUsername(string username);

        /// <summary>
        /// Asks another player to become the caller's friend.
        /// </summary>
        /// <param name="targetPlayerId">Identifier of the player who receives the request.</param>
        /// <returns>Whether the request was sent, or why it was not.</returns>
        [OperationContract(Name = "SendFriendRequest", IsInitiating = false)]
        FriendRequestResult SendFriendRequest(int targetPlayerId);

        /// <summary>
        /// Accepts or rejects a friend request the caller received.
        /// </summary>
        /// <param name="requesterPlayerId">Identifier of the player who sent the request.</param>
        /// <param name="accept">True to accept the request; false to reject it.</param>
        /// <returns>Whether the request was accepted or rejected, or why it could not be answered.</returns>
        [OperationContract(Name = "RespondFriendRequest", IsInitiating = false)]
        FriendRequestResponseResult RespondFriendRequest(int requesterPlayerId, bool accept);

        /// <summary>
        /// Ends the friendship between the caller and another player.
        /// </summary>
        /// <param name="friendPlayerId">Identifier of the friend to remove.</param>
        /// <returns>Whether the friendship was ended, or why it could not be.</returns>
        /// <exception cref="FaultException">Removing friends is not available yet.</exception>
        [OperationContract(Name = "RemoveFriend", IsInitiating = false)]
        FriendRemovalResult RemoveFriend(int friendPlayerId);
    }
}
