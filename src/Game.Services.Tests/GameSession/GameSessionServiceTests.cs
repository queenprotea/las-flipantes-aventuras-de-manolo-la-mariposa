using System.Threading.Tasks;

using CoreWCF;

using Game.Contracts;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class GameSessionServiceTests
    {
        private const int AnyRoomId = 30;
        private const int AnyMatchId = 5;
        private const int AnyMessageId = 1;
        private const int FirstPage = 1;
        private const int AnyThumbnailBytes = 1024;
        private const int FirstOffset = 0;
        private const string UnknownRoomCode = "ZZZZ";
        private const string Greeting = "Hola";

        private static readonly byte[] _avatarPart = new byte[] { 1, 2, 3 };

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly MatchCell _boardCell = new MatchCell { Row = 0, Column = 0 };
        private readonly GameSessionService _gameSessionService;

        public GameSessionServiceTests()
        {
            _gameSessionService = SampleServices.CreateGameSessionService(_callerSessions, _fakeTimeProvider);
        }

        [Fact]
        public void Test_OpenSession_WithAnEmptyToken_ReturnsInvalidSessionToken()
        {
            OpenSessionResult opening = _gameSessionService.OpenSession(string.Empty);

            Assert.Equal(OpenSessionStatus.InvalidSessionToken, opening.Status);
        }

        [Fact]
        public async Task Test_CloseSessionAsync_WithoutSession_ReturnsSessionClosed()
        {
            CloseSessionResult closing = await _gameSessionService.CloseSessionAsync();

            Assert.Equal(CloseSessionResult.SessionClosed, closing);
        }

        [Fact]
        public async Task Test_CreateRoomAsync_WithoutSession_ReturnsNotIdentified()
        {
            CreateRoomResult creation = await _gameSessionService.CreateRoomAsync(RoomVisibility.Public);

            Assert.Equal(CreateRoomStatus.NotIdentified, creation.Status);
        }

        [Fact]
        public void Test_GetRooms_WithoutRooms_ReturnsNoRooms()
        {
            RoomListResponse roomList = _gameSessionService.GetRooms();

            Assert.Empty(roomList.Rooms);
        }

        [Fact]
        public void Test_FindRoom_WithAnUnknownCode_ReturnsNoRooms()
        {
            RoomListResponse roomList = _gameSessionService.FindRoom(UnknownRoomCode);

            Assert.Empty(roomList.Rooms);
        }

        [Fact]
        public async Task Test_JoinRoomAsync_WithoutSession_ReturnsNotIdentified()
        {
            JoinRoomResult joining = await _gameSessionService.JoinRoomAsync(AnyRoomId);

            Assert.Equal(JoinRoomStatus.NotIdentified, joining.Status);
        }

        [Fact]
        public async Task Test_LeaveRoomAsync_WithoutSession_ReturnsNotInRoom()
        {
            LeaveRoomResult leaveResult = await _gameSessionService.LeaveRoomAsync();

            Assert.Equal(LeaveRoomResult.NotInRoom, leaveResult);
        }

        [Fact]
        public void Test_GetCurrentRoom_WithoutSession_ReturnsNotInRoom()
        {
            RoomLookupResponse lookup = _gameSessionService.GetCurrentRoom();

            Assert.Equal(RoomLookupStatus.NotInRoom, lookup.Status);
        }

        [Fact]
        public void Test_KickMember_WithoutSession_ReturnsNotInRoom()
        {
            KickMemberResult kickResult = _gameSessionService.KickMember(SamplePlayers.BetoId);

            Assert.Equal(KickMemberResult.NotInRoom, kickResult);
        }

        [Fact]
        public void Test_GetFriendshipList_WithoutSession_ThrowsFaultException()
        {
            Assert.Throws<FaultException>(() => _gameSessionService.GetFriendshipList());
        }

        [Fact]
        public void Test_FindPlayerByUsername_WithoutSession_ThrowsFaultException()
        {
            Assert.Throws<FaultException>(() => _gameSessionService.FindPlayerByUsername(SamplePlayers.BetoUsername));
        }

        [Fact]
        public void Test_SendFriendRequest_WithoutSession_ThrowsFaultException()
        {
            Assert.Throws<FaultException>(() => _gameSessionService.SendFriendRequest(SamplePlayers.BetoId));
        }

        [Fact]
        public void Test_RespondFriendRequest_WithoutSession_ThrowsFaultException()
        {
            Assert.Throws<FaultException>(() => _gameSessionService.RespondFriendRequest(SamplePlayers.BetoId, true));
        }

        [Fact]
        public void Test_RemoveFriend_WithoutSession_ThrowsFaultException()
        {
            Assert.Throws<FaultException>(() => _gameSessionService.RemoveFriend(SamplePlayers.BetoId));
        }

        [Fact]
        public async Task Test_GetPendingInvitationsAsync_WithoutSession_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _gameSessionService.GetPendingInvitationsAsync());
        }

        [Fact]
        public async Task Test_GetRoomInviteesAsync_WithoutSession_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _gameSessionService.GetRoomInviteesAsync());
        }

        [Fact]
        public async Task Test_SendInvitationAsync_WithoutSession_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _gameSessionService.SendInvitationAsync(SamplePlayers.BetoId));
        }

        [Fact]
        public async Task Test_SendInvitationByEmailAsync_WithoutSession_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _gameSessionService.SendInvitationByEmailAsync(SamplePlayers.BetoId));
        }

        [Fact]
        public async Task Test_RespondInvitationAsync_WithoutSession_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _gameSessionService.RespondInvitationAsync(AnyRoomId, true));
        }

        [Fact]
        public void Test_SendChatMessage_WithoutSession_ReturnsNotIdentified()
        {
            SendChatMessageResult sendResult = _gameSessionService.SendChatMessage(Greeting);

            Assert.Equal(SendChatMessageResult.NotIdentified, sendResult);
        }

        [Fact]
        public void Test_GetChatHistory_WithoutSession_ReturnsNoMessages()
        {
            ChatHistoryResponse history = _gameSessionService.GetChatHistory();

            Assert.Empty(history.Messages);
        }

        [Fact]
        public async Task Test_ReportChatMessageAsync_WithoutSession_ReturnsNotIdentified()
        {
            ReportChatMessageResult reportResult = await _gameSessionService.ReportChatMessageAsync(AnyRoomId, AnyMessageId);

            Assert.Equal(ReportChatMessageResult.NotIdentified, reportResult);
        }

        [Fact]
        public async Task Test_GetMatchHistoryAsync_WithoutSession_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _gameSessionService.GetMatchHistoryAsync(FirstPage));
        }

        [Fact]
        public async Task Test_GetMatchDetailAsync_WithoutSession_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _gameSessionService.GetMatchDetailAsync(AnyMatchId));
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithoutSession_ReturnsNotIdentified()
        {
            StartMatchResult startResult = await _gameSessionService.StartMatchAsync();

            Assert.Equal(StartMatchStatus.NotIdentified, startResult.Status);
        }

        [Fact]
        public void Test_PlacePiece_WithoutSession_ReturnsNotIdentified()
        {
            MatchActionResult actionResult = _gameSessionService.PlacePiece(MatchPiece.Caterpillar, _boardCell);

            Assert.Equal(MatchActionResult.NotIdentified, actionResult);
        }

        [Fact]
        public void Test_MoveCaterpillar_WithoutSession_ReturnsNotIdentified()
        {
            MatchActionResult actionResult = _gameSessionService.MoveCaterpillar(new MatchMove());

            Assert.Equal(MatchActionResult.NotIdentified, actionResult);
        }

        [Fact]
        public void Test_AddFlowerLevel_WithoutSession_ReturnsNotIdentified()
        {
            MatchActionResult actionResult = _gameSessionService.AddFlowerLevel(_boardCell);

            Assert.Equal(MatchActionResult.NotIdentified, actionResult);
        }

        [Fact]
        public void Test_DrawCard_WithoutSession_ReturnsNotIdentified()
        {
            MatchActionResult actionResult = _gameSessionService.DrawCard();

            Assert.Equal(MatchActionResult.NotIdentified, actionResult);
        }

        [Fact]
        public void Test_PlayCard_WithoutSession_ReturnsNotIdentified()
        {
            MatchActionResult actionResult = _gameSessionService.PlayCard(new MatchCardPlay());

            Assert.Equal(MatchActionResult.NotIdentified, actionResult);
        }

        [Fact]
        public void Test_EndTurn_WithoutSession_ReturnsNotIdentified()
        {
            MatchActionResult actionResult = _gameSessionService.EndTurn();

            Assert.Equal(MatchActionResult.NotIdentified, actionResult);
        }

        [Fact]
        public void Test_Forfeit_WithoutSession_ReturnsNotIdentified()
        {
            ForfeitResult forfeitResult = _gameSessionService.Forfeit();

            Assert.Equal(ForfeitResult.NotIdentified, forfeitResult);
        }

        [Fact]
        public void Test_ReturnToMatch_WithoutSession_ReturnsNotIdentified()
        {
            ReturnToMatchResult returnResult = _gameSessionService.ReturnToMatch();

            Assert.Equal(ReturnToMatchResult.NotIdentified, returnResult);
        }

        [Fact]
        public async Task Test_GetProfileAsync_WithoutSession_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _gameSessionService.GetProfileAsync());
        }

        [Fact]
        public void Test_BeginAvatarUpload_WithoutSession_ThrowsFaultException()
        {
            Assert.Throws<FaultException>(() => _gameSessionService.BeginAvatarUpload(AnyThumbnailBytes));
        }

        [Fact]
        public void Test_SendAvatarPart_WithoutSession_ThrowsFaultException()
        {
            Assert.Throws<FaultException>(() => _gameSessionService.SendAvatarPart(FirstOffset, _avatarPart));
        }

        [Fact]
        public void Test_CancelAvatarUpload_WithoutSession_ThrowsFaultException()
        {
            Assert.Throws<FaultException>(() => _gameSessionService.CancelAvatarUpload());
        }

        [Fact]
        public async Task Test_UpdateProfileAsync_WithoutSession_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _gameSessionService.UpdateProfileAsync(SamplePlayers.AnaUsername, false));
        }

        [Fact]
        public async Task Test_GetAvatarThumbnailAsync_WithoutSession_ThrowsFaultException()
        {
            await Assert.ThrowsAsync<FaultException>(() => _gameSessionService.GetAvatarThumbnailAsync(SamplePlayers.AnaAvatarReference));
        }
    }
}
