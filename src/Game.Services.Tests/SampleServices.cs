using System;
using System.IO;

using Microsoft.Extensions.Time.Testing;

namespace Game.Services.Tests
{
    internal static class SampleServices
    {
        internal const string AvatarFolderName = "torres-avatares-de-prueba";

        private const int MessageMaxLength = 20;
        private const int MessagesKeptPerRoom = 50;
        private const int ThresholdOccasions = 5;
        private const int MatchesPerPage = 10;

        private static readonly TimeSpan[] _temporaryBanDurations = new TimeSpan[] { TimeSpan.FromDays(1) };
        private static readonly TimeSpan _uploadIdleLimit = TimeSpan.FromMinutes(1);

        internal static byte[] SigningKey { get; } = new byte[SessionTokenIssuer.ShortestSigningKeyBytes];

        internal static RoomService CreateRoomService(CallerSessions callerSessions, RoomRegistry roomRegistry, FakeTimeProvider fakeTimeProvider)
        {
            var roomService = new RoomService(roomRegistry, callerSessions.Registry, new RecordingRoomRecordRepository(), new RecordingSanctionRepository(), new RecordingPlayerRepository(), fakeTimeProvider);

            return roomService;
        }

        internal static MatchService CreateMatchService(CallerSessions callerSessions, RoomRegistry roomRegistry, RoomService roomService, FakeTimeProvider fakeTimeProvider)
        {
            var participantRepository = new RecordingMatchParticipantRepository();
            var matchService = new MatchService(
                roomRegistry,
                roomService,
                callerSessions.Registry,
                new SystemParameterReader(CreateParameterRepository()),
                CreateRuleReader(),
                new RecordingMatchRecordRepository(participantRepository),
                participantRepository,
                new RecordingMatchStateRepository(),
                fakeTimeProvider,
                new StartingBoard(SampleMatchRules.FiveFlowerCells),
                new GuestIdIssuer(),
                new SessionTokenIssuer(SigningKey, fakeTimeProvider),
                new Random(SampleMatchRules.ChanceSeed));

            return matchService;
        }

        internal static AvatarUploadRegistry CreateAvatarUploadRegistry(CallerSessions callerSessions, FakeTimeProvider fakeTimeProvider)
        {
            var avatarUploadRegistry = new AvatarUploadRegistry(callerSessions.Registry, fakeTimeProvider, new AvatarUploadTiming(_uploadIdleLimit));

            return avatarUploadRegistry;
        }

        internal static GameSessionService CreateGameSessionService(CallerSessions callerSessions, FakeTimeProvider fakeTimeProvider)
        {
            var roomRegistry = new RoomRegistry();
            var playerRepository = new RecordingPlayerRepository();
            var participantRepository = new RecordingMatchParticipantRepository();
            var sanctionRepository = new RecordingSanctionRepository();
            RoomService roomService = CreateRoomService(callerSessions, roomRegistry, fakeTimeProvider);
            MatchService matchService = CreateMatchService(callerSessions, roomRegistry, roomService, fakeTimeProvider);
            AvatarUploadRegistry avatarUploadRegistry = CreateAvatarUploadRegistry(callerSessions, fakeTimeProvider);
            var sessionService = new SessionService(callerSessions.Registry, new SessionTokenIssuer(SigningKey, fakeTimeProvider), roomService, matchService, avatarUploadRegistry);
            var friendsService = new FriendsService(callerSessions.Registry, playerRepository, new RecordingFriendshipRepository());
            var invitationService = new InvitationService(callerSessions.Registry, roomRegistry, roomService, new RecordingRoomInvitationRepository(), playerRepository, new RecordingEmailSender(), fakeTimeProvider);
            var reportRecorder = new ReportRecorder(new RecordingReportRepository(), sanctionRepository, fakeTimeProvider);
            reportRecorder.Start(new SanctionPolicy(ThresholdOccasions, _temporaryBanDurations));
            var chatService = new ChatService(roomRegistry, callerSessions.Registry, reportRecorder, fakeTimeProvider);
            chatService.Start(new ChatCapacity(MessageMaxLength, MessagesKeptPerRoom));
            var historyService = new HistoryService(callerSessions.Registry, new RecordingMatchRecordRepository(participantRepository), participantRepository, playerRepository, new HistoryPaging(MatchesPerPage));
            var avatarStore = new AvatarStore(Path.Combine(Path.GetTempPath(), AvatarFolderName));
            var profileService = new ProfileService(callerSessions.Registry, playerRepository, avatarStore, avatarUploadRegistry, roomService, new RecordingMatchRecordRepository(participantRepository));
            var gameSessionService = new GameSessionService(sessionService, roomService, friendsService, invitationService, chatService, historyService, matchService, profileService);

            return gameSessionService;
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
    }
}
