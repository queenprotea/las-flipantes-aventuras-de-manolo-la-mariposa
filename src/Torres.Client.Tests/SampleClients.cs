using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Torres.Client.Connection;
using Torres.Client.Session;

namespace Torres.Client.Tests
{
    internal static class SampleClients
    {
        internal const int AnaId = 10;
        internal const int BetoId = 20;
        internal const int RoomId = 30;
        internal const int MatchId = 5;
        internal const int FirstPage = 1;
        internal const int SeatNumberOne = 1;
        internal const string AnaUsername = "ana_garcia";
        internal const string AnaEmail = "ana@torres.mx";
        internal const string BetoUsername = "beto_ruiz";
        internal const string Password = "Clave#2026";
        internal const string SessionToken = "token-de-prueba";
        internal const string GuestAlias = "Lupita";
        internal const string RoomCode = "K7Q2";
        internal const string ServerUnreachableMessage = "No se pudo conectar con el servidor.";

        internal static TimeSpan RefreshPeriod { get; } = TimeSpan.FromSeconds(5);

        internal static PlayerIdentity Ana { get; } = new PlayerIdentity { PlayerId = AnaId, Username = AnaUsername, Email = AnaEmail };

        internal static PlayerSession CreateSessionOfAna()
        {
            var playerSession = new PlayerSession();
            playerSession.Start(Ana);

            return playerSession;
        }

        internal static Room CreateRoom()
        {
            var room = new Room { RoomId = RoomId, Code = RoomCode };

            return room;
        }

        internal static Task<OpenSessionResult> CreateOpenedSessionAsync()
        {
            Task<OpenSessionResult> opening = Task.FromResult(new OpenSessionResult { Status = OpenSessionStatus.SessionOpened });

            return opening;
        }

        internal static Task<TReply> CreateFailedCallAsync<TReply>()
        {
            Task<TReply> failedCall = Task.FromException<TReply>(new CommunicationException(ServerUnreachableMessage));

            return failedCall;
        }

        internal static MatchChange CreatePlacementChange()
        {
            var placementView = new MatchView { Phase = MatchPhase.CaterpillarPlacement, ActiveSeatNumber = SeatNumberOne, ViewerSeatNumber = SeatNumberOne, RoomCode = RoomCode };
            var placementChange = new MatchChange(MatchChangeKind.Started, placementView, DateTime.UtcNow);

            return placementChange;
        }
    }
}
