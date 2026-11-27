using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Persistence
{
    public interface IRoomInvitationRepository
    {
        Task<InvitationCreationOutcome> CreateAsync(RoomInvitationRecord newInvitation);

        Task<List<RoomInvitationRecord>> ListForInviteeAsync(int invitedPlayerId);

        Task<List<int>> ListInviteeIdsAsync(int roomId);

        Task<bool> ExistsAsync(int roomId, int invitedPlayerId);

        Task<string?> FindChannelAsync(int roomId, int invitedPlayerId);

        Task ChangeChannelAsync(int roomId, int invitedPlayerId, string channel);

        Task<bool> DeleteAsync(int roomId, int invitedPlayerId);
    }
}
