using System.Collections.Generic;

using Game.Contracts;

using Torres.Client.Scene;

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
        internal const string MatchClock = "1:12";
        internal const string DisconnectionClock = "0:41";
        internal const string ResumeClock = "2:14";

        internal static IReadOnlyList<string> SeatPlayers { get; } = new[] { SeatOnePlayer, SeatTwoPlayer, SeatThreePlayer, SeatFourPlayer };

        internal static IReadOnlyList<string> BoardPlanterRows { get; } = new string[] { "........", ".A....B.", "........", "...C....", "........", "........", ".D....E.", "........" };

        internal static IReadOnlyList<string> BoardLevelRows { get; } = new string[] { "........", ".1....1.", "........", "...1....", "........", "........", ".1....1.", "........" };

        internal static IReadOnlyList<BoardCaterpillar> BoardCaterpillars { get; } = new BoardCaterpillar[] { new BoardCaterpillar(SeatColor.Green, new BoardCell(1, 1), 200f), new BoardCaterpillar(SeatColor.Blue, new BoardCell(1, 6), 20f), new BoardCaterpillar(SeatColor.Red, new BoardCell(3, 3), 150f), new BoardCaterpillar(SeatColor.Yellow, new BoardCell(5, 4), 250f) };
    }
}
