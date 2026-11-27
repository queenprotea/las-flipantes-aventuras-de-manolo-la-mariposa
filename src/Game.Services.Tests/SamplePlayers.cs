using System;
using System.Collections.Generic;

using Game.Persistence;

namespace Game.Services.Tests
{
    internal static class SamplePlayers
    {
        internal const int AnaId = 10;
        internal const int BetoId = 20;
        internal const int DaniId = 40;
        internal const int EvaId = 50;
        internal const int CaroGuestId = -1;
        internal const int UnknownPlayerId = 99;
        internal const string AnaUsername = "ana_garcia";
        internal const string BetoUsername = "beto_ruiz";
        internal const string DaniUsername = "dani_lopez";
        internal const string EvaUsername = "eva_mora";
        internal const string CaroAlias = "Caro";
        internal const string AnaEmail = "ana@torres.mx";
        internal const string BetoEmail = "beto@torres.mx";
        internal const string DaniEmail = "dani@torres.mx";
        internal const string EvaEmail = "eva@torres.mx";
        internal const string Password = "Clave#2026";
        internal const string AnaAvatarReference = "avatar-ana";

        private const int QuickWorkFactor = 4;

        internal static string PasswordHash { get; } = BCrypt.Net.BCrypt.HashPassword(Password, QuickWorkFactor);

        internal static SessionOwner Ana { get; } = SessionOwner.ForPlayer(AnaId, AnaUsername);

        internal static SessionOwner Beto { get; } = SessionOwner.ForPlayer(BetoId, BetoUsername);

        internal static SessionOwner Dani { get; } = SessionOwner.ForPlayer(DaniId, DaniUsername);

        internal static SessionOwner Eva { get; } = SessionOwner.ForPlayer(EvaId, EvaUsername);

        internal static SessionOwner CaroGuest { get; } = SessionOwner.ForGuest(CaroGuestId, CaroAlias);

        internal static List<Player> CreateAccounts()
        {
            var accounts = new List<Player>
            {
                CreateAccount(AnaId, AnaUsername, AnaEmail),
                CreateAccount(BetoId, BetoUsername, BetoEmail),
                CreateAccount(DaniId, DaniUsername, DaniEmail),
                CreateAccount(EvaId, EvaUsername, EvaEmail),
            };
            accounts[0].AvatarReference = AnaAvatarReference;

            return accounts;
        }

        private static Player CreateAccount(int playerId, string username, string email)
        {
            var account = new Player
            {
                PlayerId = playerId,
                Username = username,
                Email = email,
                PasswordHash = PasswordHash,
                CreatedAt = DateTime.UnixEpoch,
            };

            return account;
        }
    }
}
