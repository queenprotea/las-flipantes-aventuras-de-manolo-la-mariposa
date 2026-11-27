using System;
using System.Collections.Generic;

using Game.Contracts;
using Game.Match;

using Microsoft.Extensions.Time.Testing;

using Xunit;

namespace Game.Services.Tests
{
    public sealed class MatchNoticeForwarderTests
    {
        private const int MatchId = 5;

        private static readonly TimeSpan _tokenLifetime = TimeSpan.FromMinutes(5);

        private readonly FakeTimeProvider _fakeTimeProvider = new FakeTimeProvider();
        private readonly CallerSessions _callerSessions = new CallerSessions();
        private readonly MatchReconnectionTokenIssuer _reconnectionTokenIssuer;
        private readonly MatchNoticeForwarder _noticeForwarder;

        public MatchNoticeForwarderTests()
        {
            var roomRegistry = new RoomRegistry();
            RoomService roomService = SampleServices.CreateRoomService(_callerSessions, roomRegistry, _fakeTimeProvider);
            _reconnectionTokenIssuer = new MatchReconnectionTokenIssuer(new SessionTokenIssuer(SampleServices.SigningKey, _fakeTimeProvider), _callerSessions.Registry);
            _noticeForwarder = new MatchNoticeForwarder(_callerSessions.Registry, new MatchRoomReturn(roomRegistry, roomService), _reconnectionTokenIssuer);
            _callerSessions.Open(SamplePlayers.Ana);
        }

        [Fact]
        public void Test_ReceiveMatchStarted_WithALifetime_AttachesAReconnectionToken()
        {
            _reconnectionTokenIssuer.UseLifetime(_tokenLifetime);
            var anaView = new MatchRecipientView(SamplePlayers.AnaId, MatchId, new MatchView());

            _noticeForwarder.ReceiveMatchStarted(new List<MatchRecipientView> { anaView });

            Assert.NotEmpty(anaView.MatchView.ReconnectionToken);
        }

        [Fact]
        public void Test_ReceiveMatchStateChanged_WithoutALifetime_AttachesNoToken()
        {
            var anaView = new MatchRecipientView(SamplePlayers.AnaId, MatchId, new MatchView());

            _noticeForwarder.ReceiveMatchStateChanged(new List<MatchRecipientView> { anaView });

            Assert.Empty(anaView.MatchView.ReconnectionToken);
        }

        [Fact]
        public void Test_ReceiveMatchStateChanged_ForAnEntrantWithoutSession_AttachesNoToken()
        {
            _reconnectionTokenIssuer.UseLifetime(_tokenLifetime);
            var betoView = new MatchRecipientView(SamplePlayers.BetoId, MatchId, new MatchView());

            _noticeForwarder.ReceiveMatchStateChanged(new List<MatchRecipientView> { betoView });

            Assert.Empty(betoView.MatchView.ReconnectionToken);
        }

        [Fact]
        public void Test_ReceiveMatchFinished_ForAnEntrantWithoutRoom_KeepsTheSessionOpen()
        {
            var releasedEntrantIds = new List<int> { SamplePlayers.AnaId };

            _noticeForwarder.ReceiveMatchFinished(releasedEntrantIds, new List<EntrantStanding>());

            bool hasOpenSession = _callerSessions.Registry.HasOpenSession(SamplePlayers.AnaId);
            Assert.True(hasOpenSession);
        }

        [Fact]
        public void Test_ReceiveSeatAbsent_OfAnOpenSession_ClosesIt()
        {
            _noticeForwarder.ReceiveSeatAbsent(SamplePlayers.AnaId);

            bool hasOpenSession = _callerSessions.Registry.HasOpenSession(SamplePlayers.AnaId);
            Assert.False(hasOpenSession);
        }

        [Fact]
        public void Test_UseLifetime_OfZero_ThrowsArgumentOutOfRangeException()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _reconnectionTokenIssuer.UseLifetime(TimeSpan.Zero));
        }
    }
}
