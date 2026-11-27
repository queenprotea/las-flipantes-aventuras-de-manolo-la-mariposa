using System;
using System.Linq;
using System.Threading.Tasks;

using Game.Persistence;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class ReportRecorderTests
    {
        private const int ThresholdOccasions = 2;
        private const int RoomId = 7;
        private const int MessageId = 3;
        private const string MessageText = "hola";
        private const short FirstBanLevel = 1;

        private static readonly TimeSpan[] _temporaryBanDurations = new TimeSpan[] { TimeSpan.FromDays(1) };
        private static readonly TimeSpan _timeBetweenReports = TimeSpan.FromMinutes(1);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly RecordingReportRepository _reportRepository = new RecordingReportRepository();
        private readonly RecordingSanctionRepository _sanctionRepository = new RecordingSanctionRepository();
        private readonly ReportRecorder _reportRecorder;
        private readonly ReportTarget _anaMessage = new ReportTarget(RoomId, MessageId, SamplePlayers.AnaId, false, MessageText, false);

        public ReportRecorderTests()
        {
            _reportRecorder = new ReportRecorder(_reportRepository, _sanctionRepository, _fakeTimeProvider);
        }

        [Fact]
        public async Task Test_RecordReportAsync_BeforeStart_ThrowsInvalidOperationException()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _reportRecorder.RecordReportAsync(SamplePlayers.BetoId, _anaMessage));
        }

        [Fact]
        public async Task Test_RecordReportAsync_AfterStart_ReturnsTrue()
        {
            StartPolicy();

            bool wasRecorded = await _reportRecorder.RecordReportAsync(SamplePlayers.BetoId, _anaMessage);

            Assert.True(wasRecorded);
        }

        [Fact]
        public async Task Test_RecordReportAsync_AfterStart_SavesTheReport()
        {
            StartPolicy();

            await _reportRecorder.RecordReportAsync(SamplePlayers.BetoId, _anaMessage);

            Assert.Single(_reportRepository.Reports);
        }

        [Fact]
        public async Task Test_RecordReportAsync_BelowThreshold_AppliesNoSanction()
        {
            StartPolicy();

            await _reportRecorder.RecordReportAsync(SamplePlayers.BetoId, _anaMessage);

            Assert.Empty(_sanctionRepository.Sanctions);
        }

        [Fact]
        public async Task Test_RecordReportAsync_ReachingThreshold_AppliesOneSanction()
        {
            StartPolicy();
            await RecordAfterAMinuteAsync();
            _fakeTimeProvider.Advance(_timeBetweenReports);

            await _reportRecorder.RecordReportAsync(SamplePlayers.DaniId, _anaMessage);

            Assert.Single(_sanctionRepository.Sanctions);
        }

        [Fact]
        public async Task Test_RecordReportAsync_ReachingThreshold_AppliesTheFirstBanLevel()
        {
            StartPolicy();
            await RecordAfterAMinuteAsync();
            _fakeTimeProvider.Advance(_timeBetweenReports);

            await _reportRecorder.RecordReportAsync(SamplePlayers.DaniId, _anaMessage);

            short appliedLevel = _sanctionRepository.Sanctions.Single().Level.GetValueOrDefault();
            Assert.Equal(FirstBanLevel, appliedLevel);
        }

        [Fact]
        public async Task Test_RecordReportAsync_AfterEveryTemporaryBan_AppliesAPermanentBan()
        {
            StartPolicy();
            await RecordAfterAMinuteAsync();
            await RecordAfterAMinuteAsync();
            await RecordAfterAMinuteAsync();
            _fakeTimeProvider.Advance(_timeBetweenReports);

            await _reportRecorder.RecordReportAsync(SamplePlayers.DaniId, _anaMessage);

            bool isPermanent = _sanctionRepository.Sanctions.Last().IsPermanent;
            Assert.True(isPermanent);
        }

        private void StartPolicy()
        {
            _reportRecorder.Start(new SanctionPolicy(ThresholdOccasions, _temporaryBanDurations));
        }

        private async Task RecordAfterAMinuteAsync()
        {
            _fakeTimeProvider.Advance(_timeBetweenReports);
            await _reportRecorder.RecordReportAsync(SamplePlayers.BetoId, _anaMessage);
        }
    }
}
