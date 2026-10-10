using System.Collections.Generic;

namespace Torres.Client.Screens
{
    internal static class PreviewSamples
    {
        internal const string SeatOnePlayer = "jesus";
        internal const string SeatTwoPlayer = "valentin";
        internal const string SeatThreePlayer = "salma";
        internal const string SeatFourPlayer = "scarleth";
        internal const string AnaPlayer = "ana_torres";
        internal const string LuisPlayer = "luis_m";
        internal const string SofiaPlayer = "sofia99";
        internal const string DiegoPlayer = "dgomez";
        internal const string MarcoPlayer = "marco_r";
        internal const string PauPlayer = "pau_v";
        internal const string InesPlayer = "ines_b";
        internal const string TomasPlayer = "tomas";
        internal const string NewPlayer = "nuevo_jugador";
        internal const string OrugaPlayer = "Oruga_23";
        internal const string LobbyRoomCode = "TR-8492";
        internal const string MatchRoomCode = "K7QM";
        internal const string PlacementClock = "1:30";
        internal const string MatchClock = "1:12";
        internal const string DisconnectionClock = "0:41";
        internal const string ResumeClock = "2:14";

        internal static IReadOnlyList<string> SeatPlayers { get; } = new[] { SeatOnePlayer, SeatTwoPlayer, SeatThreePlayer, SeatFourPlayer };
    }
}
