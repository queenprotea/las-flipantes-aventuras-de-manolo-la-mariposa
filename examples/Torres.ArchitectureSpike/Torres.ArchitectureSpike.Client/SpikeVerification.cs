using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

using Torres.ArchitectureSpike.Contracts;

namespace Torres.ArchitectureSpike.Client
{
    internal sealed class SpikeVerification
    {
        private const string FirstPlayerName = "ana";
        private const string SecondPlayerName = "beto";
        private const string ThirdPlayerName = "dani";
        private const string VictimPlayerName = "carla";
        private const string ProbePlayerName = "probe";
        private const string DotnetHostName = "dotnet";
        private const int InvalidStepCount = 5;
        private const int MaximumLegalStepCount = 3;
        private const int SingleStep = 1;
        private const int FirstSeat = 1;
        private const int SecondSeat = 2;
        private const int NoSeat = 0;
        private const int NoPosition = 0;
        private const long MaximumStepMilliseconds = 1000;

        private const string InitiatingCheck = "IsInitiating blocks an operation before Connect";
        private const string ConnectCheck = "Connect {0} on the composite duplex endpoint";
        private const string DuplicateNameCheck = "A second session with the same name is rejected";
        private const string SessionCallbackCheck = "One-way callback from the session branch (ISessionCallback)";
        private const string HeartbeatCheck = "Request-reply heartbeat on the duplex channel";
        private const string JoinCheck = "Operations of the match branch on the same channel (IMatchOperations)";
        private const string RecipientViewCheck = "Each recipient gets its own view (seat 1 and seat 2)";
        private const string NotYourTurnCheck = "Domain rejection reaches the client as a contract code (not your turn)";
        private const string InvalidStepCheck = "Domain rejection reaches the client as a contract code (invalid step count)";
        private const string AppliedStepCheck = "Applied action is published to both players by one-way callback";
        private const string TurnExpiryCheck = "Notice without request: the turn timer in Match expires and both players are told";
        private const string ThreadingCheck = "Callbacks arrive on channel threads and are processed only on the loop thread";
        private const string GracefulCloseCheck = "A closed client is detected and the others are told";
        private const string VictimJoinedCheck = "A separate client process joins a match";
        private const string KilledProcessCheck = "A killed client process is detected and the others are told";
        private const string TableKeepsMovingCheck = "After the loss, the remaining player still plays without delay";

        private const string StatusDetail = "status {0}";
        private const string ExceptionDetail = "{0}: {1}";
        private const string HeartbeatRanDetail = "the heartbeat ran before Connect";
        private const string MillisecondsDetail = "{0} ms";
        private const string ChangeDetail = "reported as {0} after {1} ms";
        private const string MissingNoticeDetail = "no notice within the wait";
        private const string SeatsDetail = "{0} got seat {1}, {2} got seat {3}";
        private const string EntriesDetail = "{0} {1}, {2} {3}";
        private const string PositionDetail = "position {0}, seat in turn {1}";
        private const string ExpiryDetail = "seat in turn {0}, {1} ms after the previous step";
        private const string ThreadsDetail = "{0} notices; loop thread {1}; arrival threads {2}";
        private const string TableDetail = "result {0} in {1} ms";
        private const string MissingProcessPathMessage = "The path of the running process is unknown.";
        private const string VictimNotStartedMessage = "The victim process could not be started.";

        private readonly ClientSettings _clientSettings;
        private readonly EndpointAddress _serviceAddress;
        private readonly CheckReport _report = new CheckReport();
        private readonly GameLoop _gameLoop = new GameLoop();

        internal SpikeVerification(ClientSettings clientSettings)
        {
            _clientSettings = clientSettings;
            _serviceAddress = clientSettings.CreateEndpointAddress(ServiceEndpointPaths.GameSession);
        }

        internal async Task<int> RunAsync()
        {
            _gameLoop.Start();
            await CheckInitiatingRuleAsync();
            SpikeClient firstClient = await ConnectPlayerAsync(FirstPlayerName);
            SpikeClient secondClient = await ConnectPlayerAsync(SecondPlayerName);
            await CheckDuplicateNameAsync();
            await CheckSessionCallbackAsync(firstClient);
            await CheckHeartbeatAsync(firstClient);
            await CheckMatchStartAsync(firstClient, secondClient);
            await CheckRejectionsAsync(firstClient, secondClient);
            Stopwatch stepWatch = await CheckAppliedStepAsync(firstClient, secondClient);
            await CheckTurnExpiryAsync(firstClient, secondClient, stepWatch);
            CheckCallbackThreads(firstClient, secondClient);
            await CheckGracefulCloseAsync(firstClient, secondClient);
            await CheckKilledProcessAsync();
            firstClient.Close();
            _gameLoop.Stop();
            _report.PrintSummary();
            int failureCount = _report.FailureCount;

            return failureCount;
        }

        private SpikeClient CreateClient(string playerName)
        {
            var client = new SpikeClient(playerName, _serviceAddress);
            _gameLoop.Add(client);

            return client;
        }

        private async Task CheckInitiatingRuleAsync()
        {
            SpikeClient probeClient = CreateClient(ProbePlayerName);
            bool isPassed;
            string detail;
            try
            {
                await probeClient.Session.HeartbeatAsync();
                isPassed = false;
                detail = HeartbeatRanDetail;
            }
            catch (InvalidOperationException exception)
            {
                isPassed = true;
                detail = FormatDetail(ExceptionDetail, exception.GetType().Name, exception.Message);
            }
            catch (CommunicationException exception)
            {
                isPassed = false;
                detail = FormatDetail(ExceptionDetail, exception.GetType().Name, exception.Message);
            }

            probeClient.Abort();
            _report.Record(InitiatingCheck, isPassed, detail);
        }

        private async Task<SpikeClient> ConnectPlayerAsync(string playerName)
        {
            SpikeClient client = CreateClient(playerName);
            ConnectStatus connectStatus = await client.Session.ConnectAsync(playerName);
            string checkName = FormatDetail(ConnectCheck, playerName);
            _report.Record(checkName, connectStatus == ConnectStatus.Connected, FormatDetail(StatusDetail, connectStatus));

            return client;
        }

        private async Task CheckDuplicateNameAsync()
        {
            SpikeClient duplicateClient = CreateClient(FirstPlayerName);
            ConnectStatus connectStatus = await duplicateClient.Session.ConnectAsync(FirstPlayerName);
            duplicateClient.Close();
            _report.Record(DuplicateNameCheck, connectStatus == ConnectStatus.NameInUse, FormatDetail(StatusDetail, connectStatus));
        }

        private async Task CheckSessionCallbackAsync(SpikeClient firstClient)
        {
            ReceivedNotice? connectedNotice = await firstClient.WaitForAsync(NoticeQuery.ForConnectionOf(SecondPlayerName), _clientSettings.NotificationWait);
            string detail = DescribeConnectionNotice(connectedNotice, 0);
            _report.Record(SessionCallbackCheck, connectedNotice is not null, detail);
        }

        private async Task CheckHeartbeatAsync(SpikeClient firstClient)
        {
            Stopwatch heartbeatWatch = Stopwatch.StartNew();
            HeartbeatReply heartbeatReply = await firstClient.Session.HeartbeatAsync();
            heartbeatWatch.Stop();
            bool isPassed = heartbeatReply.ServerTimeUtc != default;
            _report.Record(HeartbeatCheck, isPassed, FormatDetail(MillisecondsDetail, heartbeatWatch.ElapsedMilliseconds));
        }

        private async Task CheckMatchStartAsync(SpikeClient firstClient, SpikeClient secondClient)
        {
            MatchEntryStatus firstEntry = await firstClient.Session.JoinMatchAsync();
            MatchEntryStatus secondEntry = await secondClient.Session.JoinMatchAsync();
            bool areEntriesExpected = (firstEntry == MatchEntryStatus.WaitingForOpponent) && (secondEntry == MatchEntryStatus.MatchStarted);
            _report.Record(JoinCheck, areEntriesExpected, FormatDetail(EntriesDetail, FirstPlayerName, firstEntry, SecondPlayerName, secondEntry));

            ReceivedNotice? firstStart = await firstClient.WaitForAsync(NoticeQuery.ForMatchEvent(MatchEventCode.MatchStarted), _clientSettings.NotificationWait);
            ReceivedNotice? secondStart = await secondClient.WaitForAsync(NoticeQuery.ForMatchEvent(MatchEventCode.MatchStarted), _clientSettings.NotificationWait);
            int firstSeat = firstStart?.MatchUpdate?.RecipientSeat ?? NoSeat;
            int secondSeat = secondStart?.MatchUpdate?.RecipientSeat ?? NoSeat;
            bool areSeatsExpected = (firstSeat == FirstSeat) && (secondSeat == SecondSeat);
            string detail = FormatDetail(SeatsDetail, FirstPlayerName, firstSeat, SecondPlayerName, secondSeat);
            _report.Record(RecipientViewCheck, areSeatsExpected, detail);
        }

        private async Task CheckRejectionsAsync(SpikeClient firstClient, SpikeClient secondClient)
        {
            ActionResult outOfTurnResult = await secondClient.Session.AdvanceTokenAsync(SingleStep);
            _report.Record(NotYourTurnCheck, outOfTurnResult == ActionResult.NotYourTurn, FormatDetail(StatusDetail, outOfTurnResult));

            ActionResult invalidStepResult = await firstClient.Session.AdvanceTokenAsync(InvalidStepCount);
            _report.Record(InvalidStepCheck, invalidStepResult == ActionResult.InvalidStepCount, FormatDetail(StatusDetail, invalidStepResult));
        }

        private async Task<Stopwatch> CheckAppliedStepAsync(SpikeClient firstClient, SpikeClient secondClient)
        {
            ActionResult stepResult = await firstClient.Session.AdvanceTokenAsync(MaximumLegalStepCount);
            Stopwatch stepWatch = Stopwatch.StartNew();
            ReceivedNotice? firstUpdate = await firstClient.WaitForAsync(NoticeQuery.ForMatchEvent(MatchEventCode.StepApplied), _clientSettings.NotificationWait);
            ReceivedNotice? secondUpdate = await secondClient.WaitForAsync(NoticeQuery.ForMatchEvent(MatchEventCode.StepApplied), _clientSettings.NotificationWait);
            bool isFirstExpected = IsAtPosition(firstUpdate, MaximumLegalStepCount, SecondSeat);
            bool isSecondExpected = IsAtPosition(secondUpdate, MaximumLegalStepCount, SecondSeat);
            bool isPassed = (stepResult == ActionResult.Applied) && isFirstExpected && isSecondExpected;
            string detail = FormatDetail(PositionDetail, secondUpdate?.MatchUpdate?.TokenPosition ?? NoPosition, secondUpdate?.MatchUpdate?.SeatInTurn ?? NoSeat);
            _report.Record(AppliedStepCheck, isPassed, detail);

            return stepWatch;
        }

        private async Task CheckTurnExpiryAsync(SpikeClient firstClient, SpikeClient secondClient, Stopwatch stepWatch)
        {
            ReceivedNotice? firstExpiry = await firstClient.WaitForAsync(NoticeQuery.ForMatchEvent(MatchEventCode.TurnExpired), _clientSettings.NotificationWait);
            long expiryMilliseconds = stepWatch.ElapsedMilliseconds;
            ReceivedNotice? secondExpiry = await secondClient.WaitForAsync(NoticeQuery.ForMatchEvent(MatchEventCode.TurnExpired), _clientSettings.NotificationWait);
            bool isPassed = IsAtPosition(firstExpiry, MaximumLegalStepCount, FirstSeat) && IsAtPosition(secondExpiry, MaximumLegalStepCount, FirstSeat);
            string detail = FormatDetail(ExpiryDetail, firstExpiry?.MatchUpdate?.SeatInTurn ?? NoSeat, expiryMilliseconds);
            _report.Record(TurnExpiryCheck, isPassed, detail);
        }

        private void CheckCallbackThreads(SpikeClient firstClient, SpikeClient secondClient)
        {
            List<ReceivedNotice> processedNotices = firstClient.CopyProcessedHistory();
            processedNotices.AddRange(secondClient.CopyProcessedHistory());
            int loopThreadId = _gameLoop.LoopThreadId;
            bool arrivedOffLoop = processedNotices.All(notice => notice.ArrivalThreadId != loopThreadId);
            bool processedOnLoop = processedNotices.All(notice => notice.ProcessedThreadId == loopThreadId);
            bool isPassed = (processedNotices.Count > 0) && arrivedOffLoop && processedOnLoop;
            string arrivalThreads = string.Join(',', processedNotices.Select(notice => notice.ArrivalThreadId).Distinct());
            _report.Record(ThreadingCheck, isPassed, FormatDetail(ThreadsDetail, processedNotices.Count, loopThreadId, arrivalThreads));
        }

        private async Task CheckGracefulCloseAsync(SpikeClient firstClient, SpikeClient secondClient)
        {
            Stopwatch closeWatch = Stopwatch.StartNew();
            secondClient.Close();
            ReceivedNotice? lossNotice = await firstClient.WaitForAsync(NoticeQuery.ForLossOf(SecondPlayerName), _clientSettings.NotificationWait);
            string detail = DescribeConnectionNotice(lossNotice, closeWatch.ElapsedMilliseconds);
            _report.Record(GracefulCloseCheck, lossNotice is not null, detail);
        }

        private async Task CheckKilledProcessAsync()
        {
            SpikeClient survivorClient = await ConnectPlayerAsync(ThirdPlayerName);
            await survivorClient.Session.JoinMatchAsync();
            using Process victimProcess = StartVictimProcess();
            ReceivedNotice? startNotice = await survivorClient.WaitForAsync(NoticeQuery.ForMatchEvent(MatchEventCode.MatchStarted), _clientSettings.NotificationWait);
            _report.Record(VictimJoinedCheck, startNotice is not null, FormatDetail(SeatsDetail, ThirdPlayerName, startNotice?.MatchUpdate?.RecipientSeat ?? NoSeat, VictimPlayerName, SecondSeat));

            Stopwatch killWatch = Stopwatch.StartNew();
            victimProcess.Kill(true);
            ReceivedNotice? lossNotice = await survivorClient.WaitForAsync(NoticeQuery.ForLossOf(VictimPlayerName), _clientSettings.NotificationWait);
            _report.Record(KilledProcessCheck, lossNotice is not null, DescribeConnectionNotice(lossNotice, killWatch.ElapsedMilliseconds));

            Stopwatch tableWatch = Stopwatch.StartNew();
            ActionResult stepResult = await survivorClient.Session.AdvanceTokenAsync(SingleStep);
            tableWatch.Stop();
            ReceivedNotice? stepNotice = await survivorClient.WaitForAsync(NoticeQuery.ForMatchEvent(MatchEventCode.StepApplied), _clientSettings.NotificationWait);
            bool isPassed = (stepResult == ActionResult.Applied) && (tableWatch.ElapsedMilliseconds < MaximumStepMilliseconds) && (stepNotice is not null);
            _report.Record(TableKeepsMovingCheck, isPassed, FormatDetail(TableDetail, stepResult, tableWatch.ElapsedMilliseconds));
            survivorClient.Close();
        }

        private static Process StartVictimProcess()
        {
            string processPath = Environment.ProcessPath ?? throw new InvalidOperationException(MissingProcessPathMessage);
            var startInfo = new ProcessStartInfo(processPath) { UseShellExecute = false };
            if (Path.GetFileNameWithoutExtension(processPath) == DotnetHostName)
            {
                startInfo.ArgumentList.Add(typeof(SpikeVerification).Assembly.Location);
            }

            startInfo.ArgumentList.Add(SpikeArguments.Victim);
            startInfo.ArgumentList.Add(VictimPlayerName);
            Process victimProcess = Process.Start(startInfo) ?? throw new InvalidOperationException(VictimNotStartedMessage);

            return victimProcess;
        }

        private static bool IsAtPosition(ReceivedNotice? notice, int tokenPosition, int seatInTurn)
        {
            bool isAtPosition = (notice?.MatchUpdate is MatchUpdate update) && (update.TokenPosition == tokenPosition) && (update.SeatInTurn == seatInTurn);

            return isAtPosition;
        }

        private static string DescribeConnectionNotice(ReceivedNotice? notice, long elapsedMilliseconds)
        {
            string description = MissingNoticeDetail;
            if (notice?.ConnectionNotice is PlayerConnectionNotice connectionNotice)
            {
                description = FormatDetail(ChangeDetail, connectionNotice.Change, elapsedMilliseconds);
            }

            return description;
        }

        private static string FormatDetail(string detailFormat, params object[] detailValues)
        {
            string detail = string.Format(CultureInfo.InvariantCulture, detailFormat, detailValues);

            return detail;
        }
    }
}
