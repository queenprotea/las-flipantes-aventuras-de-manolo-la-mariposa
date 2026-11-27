using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Avatars;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class AvatarUploadTests
    {
        private const string BeginAvatarUploadCall = nameof(RecordingGameSessionConnection.BeginAvatarUploadAsync);
        private const string SendAvatarPartCall = nameof(RecordingGameSessionConnection.SendAvatarPartAsync);
        private const string CancellationCall = nameof(RecordingGameSessionConnection.RequestAvatarUploadCancellation);
        private const int ThumbnailBytes = 1000;
        private const float FullProgress = 1f;
        private const int Precision = 4;

        private readonly RecordingGameSessionConnection _gameSessionConnection = new RecordingGameSessionConnection();
        private readonly PreparedAvatar _preparedAvatar = new PreparedAvatar(new byte[ThumbnailBytes]);

        public AvatarUploadTests()
        {
            _gameSessionConnection.BeginAvatarUploadReply = Task.FromResult(BeginAvatarUploadResult.Started);
            _gameSessionConnection.SendAvatarPartReply = Task.FromResult(SendAvatarPartResult.Accepted);
        }

        [Fact]
        public void Test_Constructor_WithAPreparedAvatar_AsksTheServerToBegin()
        {
            _ = new AvatarUpload(_gameSessionConnection, _preparedAvatar);

            Assert.Contains(BeginAvatarUploadCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_Update_AfterTheServerStarts_GoesToSending()
        {
            var upload = new AvatarUpload(_gameSessionConnection, _preparedAvatar);

            upload.Update();

            Assert.Equal(AvatarUploadState.Sending, upload.State);
        }

        [Fact]
        public void Test_Update_WhileSending_SendsThePart()
        {
            var upload = new AvatarUpload(_gameSessionConnection, _preparedAvatar);
            upload.Update();

            upload.Update();

            Assert.Contains(SendAvatarPartCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_Update_AfterEveryPartWasAccepted_CompletesTheUpload()
        {
            AvatarUpload upload = CreateUploadWithThePartSent();

            upload.Update();

            Assert.Equal(AvatarUploadState.Completed, upload.State);
        }

        [Fact]
        public void Test_Update_AfterEveryPartWasAccepted_ReachesFullProgress()
        {
            AvatarUpload upload = CreateUploadWithThePartSent();

            upload.Update();

            Assert.Equal(FullProgress, upload.Progress, Precision);
        }

        [Fact]
        public void Test_Update_WhenTheServerRefusesDuringAMatch_FailsWithRoomInMatch()
        {
            _gameSessionConnection.BeginAvatarUploadReply = Task.FromResult(BeginAvatarUploadResult.RoomInMatch);
            var upload = new AvatarUpload(_gameSessionConnection, _preparedAvatar);

            upload.Update();

            Assert.Equal(AvatarUploadFailure.RoomInMatch, upload.Failure);
        }

        [Fact]
        public void Test_Update_WhenTheServerRefusesTheThumbnail_FailsWithServerError()
        {
            _gameSessionConnection.BeginAvatarUploadReply = Task.FromResult(BeginAvatarUploadResult.ThumbnailInvalid);
            var upload = new AvatarUpload(_gameSessionConnection, _preparedAvatar);

            upload.Update();

            Assert.Equal(AvatarUploadFailure.ServerError, upload.Failure);
        }

        [Fact]
        public void Test_Update_WhenBeginningFails_MarksTheUploadFailed()
        {
            _gameSessionConnection.BeginAvatarUploadReply = SampleClients.CreateFailedCallAsync<BeginAvatarUploadResult>();
            var upload = new AvatarUpload(_gameSessionConnection, _preparedAvatar);

            upload.Update();

            Assert.Equal(AvatarUploadState.Failed, upload.State);
        }

        [Fact]
        public void Test_Update_WhenThePartArrivesDuringAMatch_FailsWithRoomInMatch()
        {
            _gameSessionConnection.SendAvatarPartReply = Task.FromResult(SendAvatarPartResult.RoomInMatch);
            AvatarUpload upload = CreateUploadWithThePartSent();

            upload.Update();

            Assert.Equal(AvatarUploadFailure.RoomInMatch, upload.Failure);
        }

        [Fact]
        public void Test_Update_WhenThePartIsOutOfRange_FailsWithServerError()
        {
            _gameSessionConnection.SendAvatarPartReply = Task.FromResult(SendAvatarPartResult.OutOfRange);
            AvatarUpload upload = CreateUploadWithThePartSent();

            upload.Update();

            Assert.Equal(AvatarUploadFailure.ServerError, upload.Failure);
        }

        [Fact]
        public void Test_Update_WhenSendingThePartFails_MarksTheUploadFailed()
        {
            _gameSessionConnection.SendAvatarPartReply = SampleClients.CreateFailedCallAsync<SendAvatarPartResult>();
            AvatarUpload upload = CreateUploadWithThePartSent();

            upload.Update();

            Assert.Equal(AvatarUploadState.Failed, upload.State);
        }

        [Fact]
        public void Test_Cancel_WhileStarting_MarksTheUploadCancelled()
        {
            var upload = new AvatarUpload(_gameSessionConnection, _preparedAvatar);

            upload.Cancel();

            Assert.Equal(AvatarUploadState.Cancelled, upload.State);
        }

        [Fact]
        public void Test_Cancel_WhileStarting_AsksTheServerToDiscardTheParts()
        {
            var upload = new AvatarUpload(_gameSessionConnection, _preparedAvatar);

            upload.Cancel();

            Assert.Contains(CancellationCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_Cancel_AfterAFailure_DoesNotAskTheServer()
        {
            _gameSessionConnection.BeginAvatarUploadReply = Task.FromResult(BeginAvatarUploadResult.ThumbnailInvalid);
            var upload = new AvatarUpload(_gameSessionConnection, _preparedAvatar);
            upload.Update();

            upload.Cancel();

            Assert.DoesNotContain(CancellationCall, _gameSessionConnection.CalledMethods);
        }

        [Fact]
        public void Test_Cancel_WithTheSessionClosed_DoesNotAskTheServer()
        {
            var upload = new AvatarUpload(_gameSessionConnection, _preparedAvatar);
            _gameSessionConnection.IsSessionOpen = false;

            upload.Cancel();

            Assert.DoesNotContain(CancellationCall, _gameSessionConnection.CalledMethods);
        }

        private AvatarUpload CreateUploadWithThePartSent()
        {
            var upload = new AvatarUpload(_gameSessionConnection, _preparedAvatar);
            upload.Update();
            upload.Update();

            return upload;
        }
    }
}
