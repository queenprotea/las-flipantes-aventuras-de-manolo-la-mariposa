using System.Collections.Generic;

namespace Game.Persistence
{
    public interface IFriendshipRepository
    {
        List<Friendship> ListByPlayer(int playerId);

        Friendship? FindBetween(int firstPlayerId, int secondPlayerId);

        FriendRequestCreationOutcome CreateRequest(int requesterId, int addresseeId);

        bool AcceptRequest(int requesterId, int addresseeId);

        bool DeleteRequest(int requesterId, int addresseeId);

        bool DeleteFriendship(int firstPlayerId, int secondPlayerId);
    }
}
