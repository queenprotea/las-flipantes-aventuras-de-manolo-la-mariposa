using System;
using System.Threading.Tasks;

using Game.Contracts;
using Game.Persistence;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class MatchServiceTests
    {
        private const int NoRoomId = 0;

        private static readonly byte[] _signingKey = new byte[SessionTokenIssuer.ShortestSigningKeyBytes];
        private static readonly TimeSpan _temporaryBanLength = TimeSpan.FromDays(1);
        private static readonly TimeSpan _noticeWaitLimit = TimeSpan.FromSeconds(5);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly RecordingSanctionRepository _sanctionRepository = new RecordingSanctionRepository();
        private readonly RoomService _roomService;
        private readonly MatchService _matchService;

        public MatchServiceTests()
        {
            var roomRegistry = new RoomRegistry();
            _roomService = new RoomService(roomRegistry, _callerSessions.Registry, new RecordingRoomRecordRepository(), _sanctionRepository, new RecordingPlayerRepository(), _fakeTimeProvider);
            var participantRepository = new RecordingMatchParticipantRepository();
            _matchService = new MatchService(
                roomRegistry,
                _roomService,
                _callerSessions.Registry,
                new SystemParameterReader(CreateParameterRepository()),
                CreateRuleReader(),
                new RecordingMatchRecordRepository(participantRepository),
                participantRepository,
                new RecordingMatchStateRepository(),
                _fakeTimeProvider,
                new StartingBoard(SampleMatchRules.FiveFlowerCells),
                new GuestIdIssuer(),
                new SessionTokenIssuer(_signingKey, _fakeTimeProvider),
                new Random(SampleMatchRules.ChanceSeed));
            _callerSessions.Open(SamplePlayers.Beto);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public async Task Test_StartMatchAsync_AsTheHostWithTwoActiveMembers_ReturnsStarted()
        {
            await PrepareRoomOfAnaAndBetoAsync();

            StartMatchResult startResult = await _matchService.StartMatchAsync();

            Assert.Equal(StartMatchStatus.Started, startResult.Status);
        }

        [Fact]
        public async Task Test_StartMatchAsync_AsTheHostWithTwoActiveMembers_TellsTheMembersTheMatchStarted()
        {
            await PrepareRoomOfAnaAndBetoAsync();

            await _matchService.StartMatchAsync();

            Room betoRoom = await _callerSessions.FindReceiverOf(SamplePlayers.Beto).WaitForRoomChangeAsync(RoomChangeReason.MatchStarted).WaitAsync(_noticeWaitLimit);
            Assert.Equal(RoomState.InMatch, betoRoom.State);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithOneActiveMember_ReturnsNotEnoughPlayers()
        {
            await _matchService.LoadRulesAsync();
            await CreateRoomAsAnaAsync();

            StartMatchResult startResult = await _matchService.StartMatchAsync();

            Assert.Equal(StartMatchStatus.NotEnoughPlayers, startResult.Status);
        }

        [Fact]
        public async Task Test_StartMatchAsync_WithABannedMember_ReturnsPlayerBanned()
        {
            await PrepareRoomOfAnaAndBetoAsync();
            DateTime nowUtc = _fakeTimeProvider.GetUtcNow().UtcDateTime;
            _sanctionRepository.Sanctions.Add(new Sanction { PlayerId = SamplePlayers.BetoId, EffectiveFrom = nowUtc, EffectiveUntil = nowUtc + _temporaryBanLength });

            StartMatchResult startResult = await _matchService.StartMatchAsync();

            Assert.Equal(StartMatchStatus.PlayerBanned, startResult.Status);
        }

        [Fact]
        public async Task Test_StartMatchAsync_AfterAMemberLostItsSession_ReturnsNotEnoughPlayers()
        {
            await PrepareRoomOfAnaAndBetoAsync();
            _callerSessions.Lose(SamplePlayers.Beto);
            _callerSessions.ActAs(SamplePlayers.Ana);

            StartMatchResult startResult = await _matchService.StartMatchAsync();

            Assert.Equal(StartMatchStatus.NotEnoughPlayers, startResult.Status);
        }

        [Fact]
        public async Task Test_StartMatchAsync_AsAMemberWhoIsNotTheHost_ReturnsNotHost()
        {
            await PrepareRoomOfAnaAndBetoAsync();
            _callerSessions.ActAs(SamplePlayers.Beto);

            StartMatchResult startResult = await _matchService.StartMatchAsync();

            Assert.Equal(StartMatchStatus.NotHost, startResult.Status);
        }

        private static RecordingSystemParameterRepository CreateParameterRepository()
        {
            var parameterRepository = new RecordingSystemParameterRepository();
            parameterRepository.ValuesByCode.Add(SampleMatchRules.TurnSecondsCode, SampleMatchRules.TurnSeconds);
            parameterRepository.ValuesByCode.Add(SampleMatchRules.ReconnectionSecondsCode, SampleMatchRules.ReconnectionSeconds);
            parameterRepository.ValuesByCode.Add(SampleMatchRules.AbsentStepsCode, SampleMatchRules.AbsentSteps);
            parameterRepository.ValuesByCode.Add(SampleMatchRules.ResumeMinutesCode, SampleMatchRules.ResumeMinutes);

            return parameterRepository;
        }

        private static MatchRuleReader CreateRuleReader()
        {
            var turnLayoutRepository = new RecordingTurnLayoutRepository();
            var buildAllowanceRepository = new RecordingBuildAllowanceRepository();
            var actionCostRepository = new RecordingActionCostRepository();
            turnLayoutRepository.TurnLayouts.AddRange(SampleMatchRules.TurnLayouts);
            buildAllowanceRepository.BuildAllowances.AddRange(SampleMatchRules.BuildAllowances);
            actionCostRepository.ActionCosts.AddRange(SampleMatchRules.ActionCosts);
            var ruleReader = new MatchRuleReader(turnLayoutRepository, buildAllowanceRepository, actionCostRepository);

            return ruleReader;
        }

        private async Task<int> CreateRoomAsAnaAsync()
        {
            _callerSessions.ActAs(SamplePlayers.Ana);
            CreateRoomResult creation = await _roomService.CreateRoomAsync(RoomVisibility.Private);
            int roomId = creation.Room?.RoomId ?? NoRoomId;

            return roomId;
        }

        private async Task PrepareRoomOfAnaAndBetoAsync()
        {
            await _matchService.LoadRulesAsync();
            int roomId = await CreateRoomAsAnaAsync();
            _callerSessions.ActAs(SamplePlayers.Beto);
            await _roomService.JoinRoomAsync(roomId);
            _callerSessions.ActAs(SamplePlayers.Ana);
        }
    }
}
