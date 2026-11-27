using System;

using Game.Contracts;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class AvatarUploadRegistryTests
    {
        private const int TwoPartThumbnailBytes = 50000;
        private const int SecondPartOffset = ProfileLimits.AvatarPartBytes;
        private const int SecondPartBytes = TwoPartThumbnailBytes - ProfileLimits.AvatarPartBytes;
        private const int MisalignedOffset = 7;
        private const int TooLargeThumbnailBytes = ProfileLimits.LargestThumbnailBytes + 1;
        private const int NoThumbnailBytes = 0;

        private static readonly TimeSpan _uploadIdleLimit = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan _afterTheIdleLimit = TimeSpan.FromMinutes(2);
        private static readonly TimeSpan _beforeTheIdleLimit = TimeSpan.FromSeconds(30);
        private static readonly byte[] _thumbnailHeader = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 13, 0x49, 0x48, 0x44, 0x52, 0, 0, 0, 128, 0, 0, 0, 128 };

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly AvatarUploadRegistry _uploadRegistry;

        public AvatarUploadRegistryTests()
        {
            _uploadRegistry = SampleServices.CreateAvatarUploadRegistry(_callerSessions, _fakeTimeProvider);
        }

        [Fact]
        public void Test_Begin_WithAnAcceptedSize_ReturnsStarted()
        {
            BeginAvatarUploadResult beginResult = _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);

            Assert.Equal(BeginAvatarUploadResult.Started, beginResult);
        }

        [Fact]
        public void Test_Begin_WithoutBytes_ReturnsThumbnailInvalid()
        {
            BeginAvatarUploadResult beginResult = _uploadRegistry.Begin(SamplePlayers.AnaId, NoThumbnailBytes);

            Assert.Equal(BeginAvatarUploadResult.ThumbnailInvalid, beginResult);
        }

        [Fact]
        public void Test_Begin_LargerThanTheLimit_ReturnsThumbnailInvalid()
        {
            BeginAvatarUploadResult beginResult = _uploadRegistry.Begin(SamplePlayers.AnaId, TooLargeThumbnailBytes);

            Assert.Equal(BeginAvatarUploadResult.ThumbnailInvalid, beginResult);
        }

        [Fact]
        public void Test_AcceptPart_WithoutUpload_ReturnsNoUploadInProgress()
        {
            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, CreateFirstPart());

            Assert.Equal(SendAvatarPartResult.NoUploadInProgress, partResult);
        }

        [Fact]
        public void Test_AcceptPart_WithAValidFirstPart_ReturnsAccepted()
        {
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);

            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, CreateFirstPart());

            Assert.Equal(SendAvatarPartResult.Accepted, partResult);
        }

        [Fact]
        public void Test_AcceptPart_AtAMisalignedOffset_ReturnsOutOfRange()
        {
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);

            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, new ReceivedAvatarPart(MisalignedOffset, new byte[SecondPartBytes]));

            Assert.Equal(SendAvatarPartResult.OutOfRange, partResult);
        }

        [Fact]
        public void Test_AcceptPart_WithAFirstPartThatIsNotAPng_ReturnsThumbnailInvalid()
        {
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);

            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, new ReceivedAvatarPart(0, new byte[ProfileLimits.AvatarPartBytes]));

            Assert.Equal(SendAvatarPartResult.ThumbnailInvalid, partResult);
        }

        [Fact]
        public void Test_AcceptPart_AfterARejectedImage_ReturnsNoUploadInProgress()
        {
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);
            _uploadRegistry.AcceptPart(SamplePlayers.AnaId, new ReceivedAvatarPart(0, new byte[ProfileLimits.AvatarPartBytes]));

            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, CreateFirstPart());

            Assert.Equal(SendAvatarPartResult.NoUploadInProgress, partResult);
        }

        [Fact]
        public void Test_AcceptPart_CompletingAnUndecodableImage_ReturnsThumbnailInvalid()
        {
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);
            _uploadRegistry.AcceptPart(SamplePlayers.AnaId, CreateFirstPart());

            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, new ReceivedAvatarPart(SecondPartOffset, new byte[SecondPartBytes]));

            Assert.Equal(SendAvatarPartResult.ThumbnailInvalid, partResult);
        }

        [Fact]
        public void Test_FindCompleted_OfAnUnfinishedUpload_ReturnsNull()
        {
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);
            _uploadRegistry.AcceptPart(SamplePlayers.AnaId, CreateFirstPart());

            CompletedAvatarUpload? completedUpload = _uploadRegistry.FindCompleted(SamplePlayers.AnaId);

            Assert.Null(completedUpload);
        }

        [Fact]
        public void Test_Cancel_OfAnUpload_DiscardsIt()
        {
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);

            _uploadRegistry.Cancel(SamplePlayers.AnaId);

            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, CreateFirstPart());
            Assert.Equal(SendAvatarPartResult.NoUploadInProgress, partResult);
        }

        [Fact]
        public void Test_DiscardForMatch_OfAnUpload_DiscardsIt()
        {
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);

            _uploadRegistry.DiscardForMatch(SamplePlayers.AnaId);

            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, CreateFirstPart());
            Assert.Equal(SendAvatarPartResult.NoUploadInProgress, partResult);
        }

        [Fact]
        public void Test_DiscardForClosedSession_OfAnUpload_DiscardsIt()
        {
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);

            _uploadRegistry.DiscardForClosedSession(SamplePlayers.AnaId);

            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, CreateFirstPart());
            Assert.Equal(SendAvatarPartResult.NoUploadInProgress, partResult);
        }

        [Fact]
        public void Test_Release_OfAnUpload_DiscardsIt()
        {
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);

            _uploadRegistry.Release(SamplePlayers.AnaId);

            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, CreateFirstPart());
            Assert.Equal(SendAvatarPartResult.NoUploadInProgress, partResult);
        }

        [Fact]
        public void Test_DiscardIdle_AfterTheIdleLimit_DiscardsTheUpload()
        {
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);
            _fakeTimeProvider.Advance(_afterTheIdleLimit);

            _uploadRegistry.DiscardIdle();

            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, CreateFirstPart());
            Assert.Equal(SendAvatarPartResult.NoUploadInProgress, partResult);
        }

        [Fact]
        public void Test_DiscardIdle_BeforeTheIdleLimit_KeepsTheUpload()
        {
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);
            _fakeTimeProvider.Advance(_beforeTheIdleLimit);

            _uploadRegistry.DiscardIdle();

            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, CreateFirstPart());
            Assert.Equal(SendAvatarPartResult.Accepted, partResult);
        }

        [Fact]
        public void Test_SessionLost_DuringAnUpload_DiscardsIt()
        {
            _callerSessions.Open(SamplePlayers.Ana);
            _uploadRegistry.Begin(SamplePlayers.AnaId, TwoPartThumbnailBytes);

            _callerSessions.Lose(SamplePlayers.Ana);

            SendAvatarPartResult partResult = _uploadRegistry.AcceptPart(SamplePlayers.AnaId, CreateFirstPart());
            Assert.Equal(SendAvatarPartResult.NoUploadInProgress, partResult);
        }

        private static ReceivedAvatarPart CreateFirstPart()
        {
            var firstPartBytes = new byte[ProfileLimits.AvatarPartBytes];
            _thumbnailHeader.CopyTo(firstPartBytes, 0);
            var firstPart = new ReceivedAvatarPart(0, firstPartBytes);

            return firstPart;
        }
    }
}
