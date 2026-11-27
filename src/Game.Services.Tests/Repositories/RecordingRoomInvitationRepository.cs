using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal sealed class RecordingRoomInvitationRepository : IRoomInvitationRepository
    {
        internal List<RoomInvitationRecord> Invitations { get; } = new List<RoomInvitationRecord>();

        internal List<int> MissingPlayerIds { get; } = new List<int>();

        public Task<InvitationCreationOutcome> CreateAsync(RoomInvitationRecord newInvitation)
        {
            InvitationCreationOutcome creationOutcome = InvitationCreationOutcome.Created;
            if (MissingPlayerIds.Contains(newInvitation.InvitedPlayerId))
            {
                creationOutcome = InvitationCreationOutcome.PlayerMissing;
            }
            else if (Find(newInvitation.RoomId, newInvitation.InvitedPlayerId) is not null)
            {
                creationOutcome = InvitationCreationOutcome.AlreadyPending;
            }
            else
            {
                Invitations.Add(newInvitation);
            }

            Task<InvitationCreationOutcome> creation = Task.FromResult(creationOutcome);

            return creation;
        }

        public Task<List<RoomInvitationRecord>> ListForInviteeAsync(int invitedPlayerId)
        {
            Task<List<RoomInvitationRecord>> listing = Task.FromResult(Invitations.FindAll(invitation => invitation.InvitedPlayerId == invitedPlayerId));

            return listing;
        }

        public Task<List<int>> ListInviteeIdsAsync(int roomId)
        {
            Task<List<int>> listing = Task.FromResult(Invitations.Where(invitation => invitation.RoomId == roomId).Select(invitation => invitation.InvitedPlayerId).ToList());

            return listing;
        }

        public Task<bool> ExistsAsync(int roomId, int invitedPlayerId)
        {
            Task<bool> checking = Task.FromResult(Find(roomId, invitedPlayerId) is not null);

            return checking;
        }

        public Task<string?> FindChannelAsync(int roomId, int invitedPlayerId)
        {
            Task<string?> finding = Task.FromResult(Find(roomId, invitedPlayerId)?.Channel);

            return finding;
        }

        public Task ChangeChannelAsync(int roomId, int invitedPlayerId, string channel)
        {
            RoomInvitationRecord? storedInvitation = Find(roomId, invitedPlayerId);
            if (storedInvitation is not null)
            {
                storedInvitation.Channel = channel;
            }

            Task changing = Task.CompletedTask;

            return changing;
        }

        public Task<bool> DeleteAsync(int roomId, int invitedPlayerId)
        {
            RoomInvitationRecord? storedInvitation = Find(roomId, invitedPlayerId);
            bool wasDeleted = (storedInvitation is not null) && Invitations.Remove(storedInvitation);
            Task<bool> deleting = Task.FromResult(wasDeleted);

            return deleting;
        }

        private RoomInvitationRecord? Find(int roomId, int invitedPlayerId)
        {
            RoomInvitationRecord? storedInvitation = Invitations.Find(invitation => (invitation.RoomId == roomId) && (invitation.InvitedPlayerId == invitedPlayerId));

            return storedInvitation;
        }
    }
}
