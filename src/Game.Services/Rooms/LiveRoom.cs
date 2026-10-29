using System;
using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

namespace Game.Services
{
    internal sealed class LiveRoom
    {
        internal const int Capacity = 4;

        private static readonly SeatColor[] _seatColorsInOrder = new SeatColor[] { SeatColor.Green, SeatColor.Blue, SeatColor.Red, SeatColor.Yellow };

        private readonly object _roomLock = new object();
        private readonly List<LiveRoomMember> _members = new List<LiveRoomMember>();

        private int _hostMemberId;
        private int _lastMemberId;
        private bool _isClosed;

        internal LiveRoom(int roomId, string code, RoomVisibility visibility)
        {
            RoomId = roomId;
            Code = code;
            Visibility = visibility;
        }

        internal int RoomId { get; }

        internal string Code { get; }

        internal RoomVisibility Visibility { get; }

        internal bool IsPublic => Visibility == RoomVisibility.Public;

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

        internal RoomDeparture Remove(int playerId)
        {
            RoomDeparture departure;
            lock (_roomLock)
            {
                LiveRoomMember? leavingMember = FindByPlayer(playerId);
                departure = leavingMember is null
                    ? new RoomDeparture(false, false, false)
                    : RemoveMember(leavingMember);
            }

            return departure;
        }

        internal RoomKick Kick(int callerPlayerId, int targetMemberId)
        {
            RoomKick kick;
            lock (_roomLock)
            {
                LiveRoomMember? caller = FindByPlayer(callerPlayerId);
                LiveRoomMember? target = _members.Find(member => member.MemberId == targetMemberId);
                KickMemberResult kickResult = JudgeKick(caller, target);
                kick = new RoomKick(kickResult, null, null);
                if ((kickResult == KickMemberResult.Kicked) && (target is not null))
                {
                    Room kickedView = BuildView(target.MemberId);
                    RemoveMember(target);
                    kick = new RoomKick(kickResult, target.PlayerId, kickedView);
                }
            }

            return kick;
        }

        internal RoomActivityChange MarkInactive(int playerId, DateTime nowUtc)
        {
            var activityChange = new RoomActivityChange(false, false);
            lock (_roomLock)
            {
                LiveRoomMember? lostMember = FindByPlayer(playerId);
                if ((lostMember is not null) && lostMember.IsActive)
                {
                    lostMember.MarkInactive(nowUtc);
                    bool hasHostChanged = PassHostIfHeldBy(lostMember);
                    activityChange = new RoomActivityChange(true, hasHostChanged);
                }
            }

            return activityChange;
        }

        internal bool MarkActive(int playerId)
        {
            bool wasInactive = false;
            lock (_roomLock)
            {
                LiveRoomMember? returningMember = FindByPlayer(playerId);
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
            var removedPlayerIds = new List<int>();
            bool hasHostChanged = false;
            bool isRoomEmpty = false;
            lock (_roomLock)
            {
                List<LiveRoomMember> expiredMembers = _members.Where(member => member.HasExpiredBefore(limitUtc)).ToList();
                foreach (LiveRoomMember expiredMember in expiredMembers)
                {
                    RoomDeparture departure = RemoveMember(expiredMember);
                    removedPlayerIds.Add(expiredMember.PlayerId);
                    hasHostChanged = hasHostChanged || departure.HasHostChanged;
                    isRoomEmpty = departure.IsRoomEmpty;
                }
            }

            var expiry = new RoomExpiry(removedPlayerIds, hasHostChanged, isRoomEmpty);

            return expiry;
        }

        internal Room? BuildViewFor(int playerId)
        {
            Room? view;
            lock (_roomLock)
            {
                LiveRoomMember? viewer = FindByPlayer(playerId);
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
                    MaxPlayers = Capacity,
                    State = _isClosed ? RoomState.Closed : RoomState.Waiting,
                };
            }

            return summary;
        }

        private JoinRoomStatus JudgeAdmission()
        {
            JoinRoomStatus admission;
            if (_isClosed)
            {
                admission = JoinRoomStatus.RoomNotFound;
            }
            else if (_members.Count >= Capacity)
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

        private LiveRoomMember? FindByPlayer(int playerId)
        {
            LiveRoomMember? member = _members.Find(candidate => candidate.PlayerId == playerId);

            return member;
        }

        private RoomRecipientView ToRecipientView(LiveRoomMember recipient)
        {
            var recipientView = new RoomRecipientView(recipient.PlayerId, BuildView(recipient.MemberId));

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
                State = _isClosed ? RoomState.Closed : RoomState.Waiting,
                MaxPlayers = Capacity,
                HostMemberId = _hostMemberId,
                ViewerMemberId = viewerMemberId,
                Members = members,
            };

            return view;
        }

        private static RoomMember ToMemberContract(LiveRoomMember member)
        {
            RoomMember memberContract = member.ToContract();

            return memberContract;
        }
    }
}
