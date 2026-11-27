using System.Collections.Generic;

using Game.Contracts;

using Torres.Client.Session;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class RoomInvitationInboxTests
    {
        private const int OtherRoomId = 31;

        private readonly RoomInvitationInbox _inbox = new RoomInvitationInbox();
        private readonly RoomInvitation _firstInvitation = new RoomInvitation { RoomId = SampleClients.RoomId };
        private readonly RoomInvitation _otherRoomInvitation = new RoomInvitation { RoomId = OtherRoomId };

        [Fact]
        public void Test_Add_OfASecondInvitationToTheSameRoom_KeepsOnlyOne()
        {
            _inbox.Add(_firstInvitation);

            _inbox.Add(new RoomInvitation { RoomId = SampleClients.RoomId });

            Assert.Single(_inbox.Invitations);
        }

        [Fact]
        public void Test_Add_OfANewInvitation_PutsItFirst()
        {
            _inbox.Add(_firstInvitation);

            _inbox.Add(_otherRoomInvitation);

            RoomInvitation firstKept = _inbox.Invitations[0];
            Assert.Same(_otherRoomInvitation, firstKept);
        }

        [Fact]
        public void Test_Remove_OfAnUnknownRoom_KeepsTheRevision()
        {
            _inbox.Add(_firstInvitation);
            int revisionBefore = _inbox.Revision;

            _inbox.Remove(OtherRoomId);

            int revisionAfter = _inbox.Revision;
            Assert.Equal(revisionBefore, revisionAfter);
        }

        [Fact]
        public void Test_Remove_OfAKnownRoom_RemovesItsInvitation()
        {
            _inbox.Add(_firstInvitation);

            _inbox.Remove(SampleClients.RoomId);

            Assert.Empty(_inbox.Invitations);
        }

        [Fact]
        public void Test_Load_WithPendingInvitations_ReplacesTheKeptOnes()
        {
            _inbox.Add(_firstInvitation);

            _inbox.Load(new PendingInvitationsResponse { Invitations = new List<RoomInvitation> { _otherRoomInvitation } });

            RoomInvitation firstKept = _inbox.Invitations[0];
            Assert.Same(_otherRoomInvitation, firstKept);
        }

        [Fact]
        public void Test_Clear_WithInvitations_EmptiesTheInbox()
        {
            _inbox.Add(_firstInvitation);

            _inbox.Clear();

            Assert.Empty(_inbox.Invitations);
        }
    }
}
