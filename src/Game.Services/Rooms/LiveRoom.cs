using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Game.Contracts;
using Game.Match;

namespace Game.Services
{
    internal sealed class LiveRoom
    {
        private static readonly SeatColor[] _seatColorsInOrder = new SeatColor[]
        {
            SeatColor.Green, SeatColor.Blue, SeatColor.Red, SeatColor.Yellow
        };

        private readonly Lock _roomLock = new Lock();
        private readonly List<LiveRoomMember> _members = new List<LiveRoomMember>();
        private readonly RoomChat _chat;

        private int _hostMemberId;
        private int _lastMemberId;
        private bool _isClosed;
        private bool _isInMatch;

        internal LiveRoom(int roomId, string code, RoomVisibility visibility)
        {
            RoomId = roomId;
            Code = code;
            Visibility = visibility;
            _chat = new RoomChat(roomId);
        }

        internal int RoomId { get; }

        internal string Code { get; }

        internal RoomVisibility Visibility { get; }

        internal bool IsPublic => Visibility == RoomVisibility.Public;

        internal bool HasMatchInProgress()
        {
            bool isInMatch;
            lock (_roomLock)
            {
                isInMatch = _isInMatch;
            }

            return isInMatch;
        }

        internal int? FindMemberIdOf(int ownerId)
        {
            int? memberId;
            lock (_roomLock)
            {
                memberId = FindByOwner(ownerId)?.MemberId;
            }

            return memberId;
        }

        internal bool ApplyProfile(int ownerId, string username, string? avatarReference)
        {
            bool isApplied = false;
            lock (_roomLock)
            {
                LiveRoomMember? member = FindByOwner(ownerId);
                if ((member is not null) && !_isInMatch)
                {
                    member.ApplyProfile(username, avatarReference);
                    isApplied = true;
                }
                else
                {
                    member?.HoldProfile(username, avatarReference);
                }
            }

            return isApplied;
        }

        internal bool ApplyLoadedAvatar(int memberId, string avatarReference, int expectedProfileVersion)
        {
            bool isApplied = false;
            lock (_roomLock)
            {
                LiveRoomMember? member = _members.Find(candidate => candidate.MemberId == memberId);
                if ((member is not null) && (member.ProfileVersion == expectedProfileVersion))
                {
                    member.ApplyLoadedAvatar(avatarReference);
                    isApplied = true;
                }
            }

            return isApplied;
        }

        internal JoinRoomStatus Admit(SessionOwner owner)
        {
            JoinRoomStatus admission;
            lock (_roomLock)
            {
                admission = JudgeAdmission();
                if (admission == JoinRoomStatus.Joined)
                {
                    AddMember(owner);
                }
            }

            return admission;
        }

        internal RoomDeparture? RemoveUnlessInMatch(int ownerId)
        {
            RoomDeparture? roomDeparture = null;
            lock (_roomLock)
            {
                LiveRoomMember? leavingMember = FindByOwner(ownerId);
                if (!_isInMatch)
                {
                    roomDeparture = leavingMember is null ? new RoomDeparture(false, false, false) : RemoveMember(leavingMember);
                }
            }

            return roomDeparture;
        }

        internal RoomKick Kick(int callerOwnerId, int targetMemberId)
        {
            RoomKick kick;
            lock (_roomLock)
            {
                LiveRoomMember? caller = FindByOwner(callerOwnerId);
                LiveRoomMember? target = _members.Find(member => member.MemberId == targetMemberId);
                KickMemberResult kickResult = JudgeKick(caller, target);
                kick = new RoomKick(kickResult, null, null);
                if ((kickResult == KickMemberResult.Kicked) && (target is not null))
                {
                    Room kickedView = BuildView(target.MemberId);
                    RemoveMember(target);
                    kick = new RoomKick(kickResult, target.OwnerId, kickedView);
                }
            }

            return kick;
        }

        internal RoomActivityChange MarkInactive(int ownerId, DateTime nowUtc)
        {
            var activityChange = new RoomActivityChange(false, false);
            lock (_roomLock)
            {
                LiveRoomMember? lostMember = FindByOwner(ownerId);
                if ((lostMember is not null) && lostMember.IsActive)
                {
                    lostMember.MarkInactive(nowUtc);
                    bool hasHostChanged = PassHostIfHeldBy(lostMember);
                    activityChange = new RoomActivityChange(true, hasHostChanged);
                }
            }

            return activityChange;
        }

        internal bool MarkActive(int ownerId)
        {
            bool wasInactive = false;
            lock (_roomLock)
            {
                LiveRoomMember? returningMember = FindByOwner(ownerId);
                if ((returningMember is not null) && !returningMember.IsActive)
                {
                    returningMember.MarkActive();
                    wasInactive = true;
                }
            }

            return wasInactive;
        }

        internal RoomExpiry RemoveExpired(DateTime limitUtc)
        {
            var removedOwnerIds = new List<int>();
            bool hasHostChanged = false;
            bool isRoomEmpty = false;
            lock (_roomLock)
            {
                List<LiveRoomMember> expiredMembers = _isInMatch ? new List<LiveRoomMember>() : _members.Where(member => member.HasExpiredBefore(limitUtc)).ToList();
                foreach (LiveRoomMember expiredMember in expiredMembers)
                {
                    RoomDeparture departure = RemoveMember(expiredMember);
                    removedOwnerIds.Add(expiredMember.OwnerId);
                    hasHostChanged = hasHostChanged || departure.HasHostChanged;
                    isRoomEmpty = departure.IsRoomEmpty;
                }
            }

            var expiry = new RoomExpiry(removedOwnerIds, hasHostChanged, isRoomEmpty);

            return expiry;
        }

        internal MatchStartCheck CheckMatchStart(int callerOwnerId)
        {
            MatchStartCheck startCheck;
            lock (_roomLock)
            {
                startCheck = new MatchStartCheck(JudgeMatchStartStatus(callerOwnerId), ListMatchEntrants());
            }

            return startCheck;
        }

        internal MatchStartCheck StartMatch(int callerOwnerId)
        {
            MatchStartCheck startCheck;
            lock (_roomLock)
            {
                StartMatchStatus startStatus = JudgeMatchStartStatus(callerOwnerId);
                _isInMatch = startStatus == StartMatchStatus.Started;
                startCheck = new MatchStartCheck(startStatus, ListMatchEntrants());
            }

            return startCheck;
        }

        internal bool EndMatch()
        {
            bool wasInMatch;
            lock (_roomLock)
            {
                wasInMatch = _isInMatch;
                _isInMatch = false;
                _members.ForEach(ApplyHeldProfile);
            }

            return wasInMatch;
        }

        internal Room? BuildViewFor(int ownerId)
        {
            Room? view;
            lock (_roomLock)
            {
                LiveRoomMember? viewer = FindByOwner(ownerId);
                view = viewer is null ? null : BuildView(viewer.MemberId);
            }

            return view;
        }

        internal IReadOnlyList<RoomRecipientView> BuildViewsForMembers()
        {
            List<RoomRecipientView> recipientViews;
            lock (_roomLock)
            {
                recipientViews = _members.Select(ToRecipientView).ToList();
            }

            return recipientViews;
        }

        internal RoomSummary BuildSummary()
        {
            RoomSummary summary;
            lock (_roomLock)
            {
                LiveRoomMember? host = _members.Find(member => member.MemberId == _hostMemberId);
                summary = new RoomSummary
                {
                    RoomId = RoomId,
                    Code = Code,
                    Visibility = Visibility,
                    HostDisplayName = host?.DisplayName ?? string.Empty,
                    PlayerCount = _members.Count,
                    MaxPlayers = MatchCoordinator.MaximumEntrantCount,
                    State = GetRoomState(),
                };
            }

            return summary;
        }

        internal ChatPosting? PostChatMessage(int authorOwnerId, ChatDraft messageDraft, int messagesKeptPerRoom)
        {
            ChatPosting? posting = null;
            lock (_roomLock)
            {
                LiveRoomMember? author = FindByOwner(authorOwnerId);
                if (author is not null)
                {
                    ChatMessage postedMessage = _chat.Append(author, messageDraft, messagesKeptPerRoom);
                    List<int> recipientOwnerIds = _members.Select(member => member.OwnerId).ToList();
                    posting = new ChatPosting(postedMessage, recipientOwnerIds);
                }
            }

            return posting;
        }

        internal IReadOnlyList<ChatMessage>? ReadChatHistory(int readerOwnerId)
        {
            IReadOnlyList<ChatMessage>? keptMessages = null;
            lock (_roomLock)
            {
                if (FindByOwner(readerOwnerId) is not null)
                {
                    keptMessages = _chat.ListMessages();
                }
            }

            return keptMessages;
        }

        internal ReportTarget? FindReportTarget(int reporterOwnerId, int messageId)
        {
            ReportTarget? reportTarget = null;
            lock (_roomLock)
            {
                RoomChatEntry? reportedEntry = FindByOwner(reporterOwnerId) is null ? null : _chat.FindEntry(messageId);
                if (reportedEntry is not null)
                {
                    reportTarget = ToReportTarget(reportedEntry, reporterOwnerId);
                }
            }

            return reportTarget;
        }

        internal void RecordReporter(int messageId, int reporterOwnerId)
        {
            lock (_roomLock)
            {
                _chat.FindEntry(messageId)?.RecordReporter(reporterOwnerId);
            }
        }

        private ReportTarget ToReportTarget(RoomChatEntry reportedEntry, int reporterOwnerId)
        {
            ChatMessage reportedMessage = reportedEntry.Message;
            bool wasReportedByReporter = reportedEntry.WasReportedBy(reporterOwnerId);
            var reportTarget = new ReportTarget(RoomId, reportedMessage.MessageId, reportedEntry.AuthorOwnerId, reportedMessage.SenderIsGuest, reportedMessage.Text, wasReportedByReporter);

            return reportTarget;
        }

        private JoinRoomStatus JudgeAdmission()
        {
            JoinRoomStatus admission;
            if (_isClosed)
            {
                admission = JoinRoomStatus.RoomNotFound;
            }
            else if (_isInMatch)
            {
                admission = JoinRoomStatus.RoomInMatch;
            }
            else if (_members.Count >= MatchCoordinator.MaximumEntrantCount)
            {
                admission = JoinRoomStatus.RoomFull;
            }
            else
            {
                admission = JoinRoomStatus.Joined;
            }

            return admission;
        }

        private void AddMember(SessionOwner owner)
        {
            _lastMemberId++;
            SeatColor freeColor = _seatColorsInOrder.First(IsColorFree);
            var newMember = new LiveRoomMember(_lastMemberId, owner, freeColor);
            if (_members.Count == 0)
            {
                _hostMemberId = newMember.MemberId;
            }

            _members.Add(newMember);
        }

        private bool IsColorFree(SeatColor color)
        {
            bool isFree = _members.All(member => member.Color != color);

            return isFree;
        }

        private RoomDeparture RemoveMember(LiveRoomMember leavingMember)
        {
            int leavingIndex = _members.IndexOf(leavingMember);
            _members.RemoveAt(leavingIndex);
            bool isRoomEmpty = _members.Count == 0;
            bool hasHostChanged = (leavingMember.MemberId == _hostMemberId) && !isRoomEmpty;
            if (hasHostChanged)
            {
                LiveRoomMember nextHost = FindNextActive(leavingIndex) ?? _members[leavingIndex % _members.Count];
                _hostMemberId = nextHost.MemberId;
            }

            _isClosed = isRoomEmpty;
            var departure = new RoomDeparture(true, hasHostChanged, isRoomEmpty);

            return departure;
        }

        private bool PassHostIfHeldBy(LiveRoomMember lostMember)
        {
            if (lostMember.MemberId != _hostMemberId)
            {
                return false;
            }

            int lostIndex = _members.IndexOf(lostMember);
            LiveRoomMember? nextHost = FindNextActive(lostIndex + 1);
            if (nextHost is not null)
            {
                _hostMemberId = nextHost.MemberId;
            }

            bool hasHostChanged = nextHost is not null;

            return hasHostChanged;
        }

        private LiveRoomMember? FindNextActive(int startIndex)
        {
            for (var i = 0; i < _members.Count; i++)
            {
                LiveRoomMember candidate = _members[(startIndex + i) % _members.Count];
                if (candidate.IsActive)
                {
                    return candidate;
                }
            }

            return null;
        }

        private KickMemberResult JudgeKick(LiveRoomMember? caller, LiveRoomMember? target)
        {
            KickMemberResult kickResult;
            if (caller is null)
            {
                kickResult = KickMemberResult.NotInRoom;
            }
            else if (caller.MemberId != _hostMemberId)
            {
                kickResult = KickMemberResult.NotHost;
            }
            else if (_isInMatch)
            {
                kickResult = KickMemberResult.RoomInMatch;
            }
            else if (target is null)
            {
                kickResult = KickMemberResult.TargetNotInRoom;
            }
            else if (target.MemberId == caller.MemberId)
            {
                kickResult = KickMemberResult.CannotKickSelf;
            }
            else
            {
                kickResult = KickMemberResult.Kicked;
            }

            return kickResult;
        }

        private StartMatchStatus JudgeMatchStartStatus(int callerOwnerId)
        {
            LiveRoomMember? callerMember = FindByOwner(callerOwnerId);
            StartMatchStatus startStatus;
            if (callerMember is null)
            {
                startStatus = StartMatchStatus.NotInRoom;
            }
            else if (callerMember.MemberId != _hostMemberId)
            {
                startStatus = StartMatchStatus.NotHost;
            }
            else if (_isInMatch)
            {
                startStatus = StartMatchStatus.RoomInMatch;
            }
            else if (!HasEnoughActiveMembers())
            {
                startStatus = StartMatchStatus.NotEnoughPlayers;
            }
            else
            {
                startStatus = StartMatchStatus.Started;
            }

            return startStatus;
        }

        private List<MatchEntrant> ListMatchEntrants()
        {
            List<MatchEntrant> entrants = _members.Select(ToMatchEntrant).ToList();

            return entrants;
        }

        private bool HasEnoughActiveMembers()
        {
            int activeMemberCount = _members.Count(member => member.IsActive);
            bool hasEnoughActiveMembers = activeMemberCount >= MatchCoordinator.MinimumEntrantCount;

            return hasEnoughActiveMembers;
        }

        private bool CanStartMatch()
        {
            bool canStartMatch = !_isClosed && !_isInMatch && HasEnoughActiveMembers();

            return canStartMatch;
        }

        private RoomState GetRoomState()
        {
            RoomState roomState = RoomState.Waiting;
            if (_isClosed)
            {
                roomState = RoomState.Closed;
            }
            else if (_isInMatch)
            {
                roomState = RoomState.InMatch;
            }

            return roomState;
        }

        private LiveRoomMember? FindByOwner(int ownerId)
        {
            LiveRoomMember? member = _members.Find(candidate => candidate.OwnerId == ownerId);

            return member;
        }

        private RoomRecipientView ToRecipientView(LiveRoomMember recipient)
        {
            var recipientView = new RoomRecipientView(recipient.OwnerId, BuildView(recipient.MemberId));

            return recipientView;
        }

        private Room BuildView(int viewerMemberId)
        {
            List<RoomMember> members = _members.Select(ToMemberContract).ToList();
            var view = new Room
            {
                RoomId = RoomId,
                Code = Code,
                Visibility = Visibility,
                State = GetRoomState(),
                MaxPlayers = MatchCoordinator.MaximumEntrantCount,
                HostMemberId = _hostMemberId,
                ViewerMemberId = viewerMemberId,
                CanViewerStartMatch = (viewerMemberId == _hostMemberId) && CanStartMatch(),
                Members = members,
            };

            return view;
        }

        private static MatchEntrant ToMatchEntrant(LiveRoomMember member)
        {
            MatchEntrant entrant = member.ToMatchEntrant();

            return entrant;
        }

        private static void ApplyHeldProfile(LiveRoomMember member)
        {
            member.ApplyHeldProfile();
        }

        private static RoomMember ToMemberContract(LiveRoomMember member)
        {
            RoomMember memberContract = member.ToContract();

            return memberContract;
        }
    }
}
