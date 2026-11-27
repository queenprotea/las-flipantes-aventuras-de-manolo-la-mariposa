using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

namespace Game.Domain.Tests
{
    public sealed class ActionCardTableTests
    {
        private const int TwoActionPoints = 2;
        private const int ThreeActionPoints = 3;
        private const int FourActionPoints = 4;
        private const int SevenCardsLeft = 7;
        private const int TwoKeptCards = 2;
        private const int OneCardPoint = 1;
        private const int NoCardPoints = 0;
        private const int SamePoints = 5;

        private static BoardCell WestFlower { get; } = new BoardCell(3, 3);

        private static BoardCell MiddleFlower { get; } = new BoardCell(3, 4);

        private static BoardCell EastFlower { get; } = new BoardCell(3, 5);

        private static BoardCell FarFlower { get; } = new BoardCell(6, 6);

        private static BoardCell GroundAboveWestFlower { get; } = new BoardCell(2, 3);

        private static BoardCell LeapOrigin { get; } = new BoardCell(5, 0);

        private static BoardCell LeapMiddle { get; } = new BoardCell(5, 1);

        private static BoardCell LeapLanding { get; } = new BoardCell(5, 2);

        private static BoardCell FarGround { get; } = new BoardCell(7, 7);

        [Fact]
        public void Test_DrawCard_OnOwnTurn_ReturnsApplied()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(Array.Empty<ActionCard>(), MatchTable.ActionPointsPerTurn));

            TurnActionOutcome drawOutcome = table.CardTable.DrawCard(SampleRules.SeatNumberOne);

            Assert.Equal(TurnActionOutcome.Applied, drawOutcome);
        }

        [Fact]
        public void Test_DrawCard_OnOwnTurn_PutsOneCardInTheHand()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(Array.Empty<ActionCard>(), MatchTable.ActionPointsPerTurn));

            table.CardTable.DrawCard(SampleRules.SeatNumberOne);

            Assert.Single(table.CardTable.SeatCards[0].Hand);
        }

        [Fact]
        public void Test_DrawCard_OnOwnTurn_LeavesSevenCardsInTheDeck()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(Array.Empty<ActionCard>(), MatchTable.ActionPointsPerTurn));

            table.CardTable.DrawCard(SampleRules.SeatNumberOne);

            Assert.Equal(SevenCardsLeft, table.CardTable.SeatCards[0].Deck.Count);
        }

        [Fact]
        public void Test_DrawCard_OnOwnTurn_ChargesOneActionPoint()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(Array.Empty<ActionCard>(), MatchTable.ActionPointsPerTurn));

            table.CardTable.DrawCard(SampleRules.SeatNumberOne);

            Assert.Equal(FourActionPoints, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_DrawCard_OutOfTurn_ReturnsNotYourTurn()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(Array.Empty<ActionCard>(), MatchTable.ActionPointsPerTurn));

            TurnActionOutcome drawOutcome = table.CardTable.DrawCard(SampleRules.SeatNumberTwo);

            Assert.Equal(TurnActionOutcome.NotYourTurn, drawOutcome);
        }

        [Fact]
        public void Test_DrawCard_WithEmptyDeck_ReturnsDeckEmpty()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(ActionCardTable.FullDeck, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome drawOutcome = table.CardTable.DrawCard(SampleRules.SeatNumberOne);

            Assert.Equal(TurnActionOutcome.DeckEmpty, drawOutcome);
        }

        [Fact]
        public void Test_DrawCard_WithoutActionPoints_ReturnsNotEnoughActionPoints()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(Array.Empty<ActionCard>(), SampleRules.NoActionPoints));

            TurnActionOutcome drawOutcome = table.CardTable.DrawCard(SampleRules.SeatNumberOne);

            Assert.Equal(TurnActionOutcome.NotEnoughActionPoints, drawOutcome);
        }

        [Fact]
        public void Test_PlayCard_NotInHand_ReturnsCardNotInHand()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(Array.Empty<ActionCard>(), MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.SixActionPoints, Array.Empty<BoardCell>()));

            Assert.Equal(TurnActionOutcome.CardNotInHand, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_SixActionPoints_ReturnsApplied()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.SixActionPoints }, TwoActionPoints));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.SixActionPoints, Array.Empty<BoardCell>()));

            Assert.Equal(TurnActionOutcome.Applied, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_SixActionPoints_AddsOnePointToWhatIsLeft()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.SixActionPoints }, TwoActionPoints));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.SixActionPoints, Array.Empty<BoardCell>()));

            Assert.Equal(ThreeActionPoints, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_PlayCard_SixActionPoints_TakesTheCardOutOfTheHand()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.SixActionPoints }, TwoActionPoints));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.SixActionPoints, Array.Empty<BoardCell>()));

            Assert.Empty(table.CardTable.SeatCards[0].Hand);
        }

        [Fact]
        public void Test_PlayCard_SevenActionPoints_AddsTwoPointsToWhatIsLeft()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.SevenActionPoints }, TwoActionPoints));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.SevenActionPoints, Array.Empty<BoardCell>()));

            Assert.Equal(FourActionPoints, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_PlayCard_SecondPointsCardInTheSameTurn_ReturnsPointsCardAlreadyUsed()
        {
            var seatOneHand = new ActionCard[] { ActionCard.SevenActionPoints };
            var seatCards = new SeatCards[] { new SeatCards(SampleRules.SeatNumberOne, seatOneHand, ActionCardTable.FullDeck.Except(seatOneHand).Except(new ActionCard[] { ActionCard.SixActionPoints }).ToList(), true), new SeatCards(SampleRules.SeatNumberTwo, Array.Empty<ActionCard>(), ActionCardTable.FullDeck, false), };
            var snapshot = new MatchSnapshot(TablePhase.PlayerTurn, MatchTable.FirstRoundNumber, MatchTable.FirstTurnRoundNumber, SampleRules.TwoSeatCount, SampleRules.SeatNumberOne, ThreeActionPoints, SampleRules.ThreeBuildings, CreateFlatPlanter(), Array.Empty<Caterpillar>(), null, Array.Empty<SeatStanding>(), seatCards, Array.Empty<int>(), null);
            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.TwoSeatRules, SampleRules.CreateChanceSource());

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.SevenActionPoints, Array.Empty<BoardCell>()));

            Assert.Equal(TurnActionOutcome.PointsCardAlreadyUsed, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_SecondPointsCardInTheSameTurn_KeepsTheCardInTheHand()
        {
            var seatOneHand = new ActionCard[] { ActionCard.SevenActionPoints };
            var seatCards = new SeatCards[] { new SeatCards(SampleRules.SeatNumberOne, seatOneHand, ActionCardTable.FullDeck.Except(seatOneHand).Except(new ActionCard[] { ActionCard.SixActionPoints }).ToList(), true), new SeatCards(SampleRules.SeatNumberTwo, Array.Empty<ActionCard>(), ActionCardTable.FullDeck, false), };
            var snapshot = new MatchSnapshot(TablePhase.PlayerTurn, MatchTable.FirstRoundNumber, MatchTable.FirstTurnRoundNumber, SampleRules.TwoSeatCount, SampleRules.SeatNumberOne, ThreeActionPoints, SampleRules.ThreeBuildings, CreateFlatPlanter(), Array.Empty<Caterpillar>(), null, Array.Empty<SeatStanding>(), seatCards, Array.Empty<int>(), null);
            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.TwoSeatRules, SampleRules.CreateChanceSource());

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.SevenActionPoints, Array.Empty<BoardCell>()));

            Assert.Contains(ActionCard.SevenActionPoints, table.CardTable.SeatCards[0].Hand);
        }

        [Fact]
        public void Test_PlayCard_DoubleClimbFromLevelOneToNeighborLevelThree_ReturnsApplied()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(MiddleFlower, SampleLayouts.LevelThree) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, WestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.DoubleClimb }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.DoubleClimb, new BoardCell[] { WestFlower, MiddleFlower }));

            Assert.Equal(TurnActionOutcome.Applied, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_DoubleClimbFromLevelOneToNeighborLevelThree_MovesTheCaterpillar()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(MiddleFlower, SampleLayouts.LevelThree) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, WestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.DoubleClimb }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.DoubleClimb, new BoardCell[] { WestFlower, MiddleFlower }));

            Assert.Contains(new Caterpillar(SampleRules.SeatNumberOne, MiddleFlower), table.Caterpillars);
        }

        [Fact]
        public void Test_PlayCard_DoubleClimbFromLevelOneToNeighborLevelThree_ChargesNoActionPoints()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(MiddleFlower, SampleLayouts.LevelThree) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, WestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.DoubleClimb }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.DoubleClimb, new BoardCell[] { WestFlower, MiddleFlower }));

            Assert.Equal(MatchTable.ActionPointsPerTurn, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_PlayCard_DoubleClimbToLevelTwo_ReturnsWrongCardTarget()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(MiddleFlower, SampleLayouts.LevelTwo) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, WestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.DoubleClimb }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.DoubleClimb, new BoardCell[] { WestFlower, MiddleFlower }));

            Assert.Equal(TurnActionOutcome.WrongCardTarget, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_DoubleClimbToLevelTwo_KeepsTheCardInTheHand()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(MiddleFlower, SampleLayouts.LevelTwo) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, WestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.DoubleClimb }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.DoubleClimb, new BoardCell[] { WestFlower, MiddleFlower }));

            Assert.Contains(ActionCard.DoubleClimb, table.CardTable.SeatCards[0].Hand);
        }

        [Fact]
        public void Test_PlayCard_GroundLeapOverCaterpillar_ReturnsApplied()
        {
            var caterpillars = new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, LeapOrigin), new Caterpillar(SampleRules.SeatNumberTwo, LeapMiddle) };
            MatchTable table = CreateCardTable(CreateFlatPlanter(), caterpillars, new TurnHolding(new ActionCard[] { ActionCard.GroundLeap }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.GroundLeap, new BoardCell[] { LeapOrigin, LeapLanding }));

            Assert.Equal(TurnActionOutcome.Applied, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_GroundLeapOverCaterpillar_LandsTwoCellsAway()
        {
            var caterpillars = new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, LeapOrigin), new Caterpillar(SampleRules.SeatNumberTwo, LeapMiddle) };
            MatchTable table = CreateCardTable(CreateFlatPlanter(), caterpillars, new TurnHolding(new ActionCard[] { ActionCard.GroundLeap }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.GroundLeap, new BoardCell[] { LeapOrigin, LeapLanding }));

            Assert.Contains(new Caterpillar(SampleRules.SeatNumberOne, LeapLanding), table.Caterpillars);
        }

        [Fact]
        public void Test_PlayCard_GroundLeapOverEmptyCell_ReturnsWrongCardTarget()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, LeapOrigin) }, new TurnHolding(new ActionCard[] { ActionCard.GroundLeap }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.GroundLeap, new BoardCell[] { LeapOrigin, LeapLanding }));

            Assert.Equal(TurnActionOutcome.WrongCardTarget, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_FlowerLevelMoveOntoAnotherFlower_ReturnsApplied()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelTwo), new Flower(MiddleFlower, SampleLayouts.LevelOne) };
            MatchTable table = CreateCardTable(flowers, Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.FlowerLevelMove }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.FlowerLevelMove, new BoardCell[] { WestFlower, MiddleFlower }));

            Assert.Equal(TurnActionOutcome.Applied, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_FlowerLevelMoveOntoAnotherFlower_LowersTheSourceFlower()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelTwo), new Flower(MiddleFlower, SampleLayouts.LevelOne) };
            MatchTable table = CreateCardTable(flowers, Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.FlowerLevelMove }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.FlowerLevelMove, new BoardCell[] { WestFlower, MiddleFlower }));

            IReadOnlyList<Flower> flowersOnTable = table.FindFlowers();
            Assert.Contains(new Flower(WestFlower, SampleLayouts.LevelOne), flowersOnTable);
        }

        [Fact]
        public void Test_PlayCard_FlowerLevelMoveOntoAnotherFlower_RaisesTheTargetFlower()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelTwo), new Flower(MiddleFlower, SampleLayouts.LevelOne) };
            MatchTable table = CreateCardTable(flowers, Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.FlowerLevelMove }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.FlowerLevelMove, new BoardCell[] { WestFlower, MiddleFlower }));

            IReadOnlyList<Flower> flowersOnTable = table.FindFlowers();
            Assert.Contains(new Flower(MiddleFlower, SampleLayouts.LevelTwo), flowersOnTable);
        }

        [Fact]
        public void Test_PlayCard_FlowerLevelMoveOntoAnotherFlower_SpendsNoBuild()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelTwo), new Flower(MiddleFlower, SampleLayouts.LevelOne) };
            MatchTable table = CreateCardTable(flowers, Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.FlowerLevelMove }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.FlowerLevelMove, new BoardCell[] { WestFlower, MiddleFlower }));

            Assert.Equal(SampleRules.ThreeBuildings, table.BuildsLeft);
        }

        [Fact]
        public void Test_PlayCard_FlowerLevelMoveOntoGroundNextToPlanter_PlantsANewFlower()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelTwo), new Flower(MiddleFlower, SampleLayouts.LevelOne) };
            MatchTable table = CreateCardTable(flowers, Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.FlowerLevelMove }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.FlowerLevelMove, new BoardCell[] { WestFlower, GroundAboveWestFlower }));

            IReadOnlyList<Flower> flowersOnTable = table.FindFlowers();
            Assert.Contains(new Flower(GroundAboveWestFlower, SampleLayouts.LevelOne), flowersOnTable);
        }

        [Fact]
        public void Test_PlayCard_FlowerLevelMoveFromSingleLevelFlower_ReturnsWrongCardTarget()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.FlowerLevelMove }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.FlowerLevelMove, new BoardCell[] { WestFlower, MiddleFlower }));

            Assert.Equal(TurnActionOutcome.WrongCardTarget, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_RelocationToFarFreeCell_ReturnsApplied()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, WestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.Relocation }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.Relocation, new BoardCell[] { WestFlower, FarGround }));

            Assert.Equal(TurnActionOutcome.Applied, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_RelocationToFarFreeCell_MovesTheCaterpillar()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, WestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.Relocation }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.Relocation, new BoardCell[] { WestFlower, FarGround }));

            Assert.Contains(new Caterpillar(SampleRules.SeatNumberOne, FarGround), table.Caterpillars);
        }

        [Fact]
        public void Test_PlayCard_RelocationClimbingTwoLevels_ReturnsClimbTooHigh()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(MiddleFlower, SampleLayouts.LevelTwo) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, FarGround) }, new TurnHolding(new ActionCard[] { ActionCard.Relocation }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.Relocation, new BoardCell[] { FarGround, MiddleFlower }));

            Assert.Equal(TurnActionOutcome.ClimbTooHigh, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_PlanterCrossingWithinThePlanter_ReturnsApplied()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(MiddleFlower, SampleLayouts.LevelOne), new Flower(EastFlower, SampleLayouts.LevelTwo) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, WestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.PlanterCrossing }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.PlanterCrossing, new BoardCell[] { WestFlower, EastFlower }));

            Assert.Equal(TurnActionOutcome.Applied, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_PlanterCrossingWithinThePlanter_MovesTheCaterpillar()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(MiddleFlower, SampleLayouts.LevelOne), new Flower(EastFlower, SampleLayouts.LevelTwo) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, WestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.PlanterCrossing }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.PlanterCrossing, new BoardCell[] { WestFlower, EastFlower }));

            Assert.Contains(new Caterpillar(SampleRules.SeatNumberOne, EastFlower), table.Caterpillars);
        }

        [Fact]
        public void Test_PlayCard_PlanterCrossingWithinThePlanter_ChargesOneActionPoint()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(MiddleFlower, SampleLayouts.LevelOne), new Flower(EastFlower, SampleLayouts.LevelTwo) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, WestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.PlanterCrossing }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.PlanterCrossing, new BoardCell[] { WestFlower, EastFlower }));

            Assert.Equal(FourActionPoints, table.ActionPointsLeft);
        }

        [Fact]
        public void Test_PlayCard_PlanterCrossingFromGround_ReturnsApplied()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(MiddleFlower, SampleLayouts.LevelOne), new Flower(EastFlower, SampleLayouts.LevelOne) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, GroundAboveWestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.PlanterCrossing }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.PlanterCrossing, new BoardCell[] { GroundAboveWestFlower, EastFlower }));

            Assert.Equal(TurnActionOutcome.Applied, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_PlanterCrossingFromGround_ClimbsOntoTheTouchingPlanter()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(MiddleFlower, SampleLayouts.LevelOne), new Flower(EastFlower, SampleLayouts.LevelOne) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, GroundAboveWestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.PlanterCrossing }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.PlanterCrossing, new BoardCell[] { GroundAboveWestFlower, EastFlower }));

            Assert.Contains(new Caterpillar(SampleRules.SeatNumberOne, EastFlower), table.Caterpillars);
        }

        [Fact]
        public void Test_PlayCard_PlanterCrossingToAnotherPlanter_ReturnsWrongCardTarget()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(FarFlower, SampleLayouts.LevelOne) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, WestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.PlanterCrossing }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.PlanterCrossing, new BoardCell[] { WestFlower, FarFlower }));

            Assert.Equal(TurnActionOutcome.WrongCardTarget, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_FlowerLevelRemovalOnTwoFlowers_ReturnsApplied()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelThree), new Flower(MiddleFlower, SampleLayouts.LevelTwo) };
            MatchTable table = CreateCardTable(flowers, Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.FlowerLevelRemoval }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.FlowerLevelRemoval, new BoardCell[] { WestFlower, MiddleFlower }));

            Assert.Equal(TurnActionOutcome.Applied, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_FlowerLevelRemovalOnTwoFlowers_LowersTheWestFlower()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelThree), new Flower(MiddleFlower, SampleLayouts.LevelTwo) };
            MatchTable table = CreateCardTable(flowers, Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.FlowerLevelRemoval }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.FlowerLevelRemoval, new BoardCell[] { WestFlower, MiddleFlower }));

            IReadOnlyList<Flower> flowersOnTable = table.FindFlowers();
            Assert.Contains(new Flower(WestFlower, SampleLayouts.LevelTwo), flowersOnTable);
        }

        [Fact]
        public void Test_PlayCard_FlowerLevelRemovalOnTwoFlowers_LowersTheMiddleFlower()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelThree), new Flower(MiddleFlower, SampleLayouts.LevelTwo) };
            MatchTable table = CreateCardTable(flowers, Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.FlowerLevelRemoval }, MatchTable.ActionPointsPerTurn));

            table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.FlowerLevelRemoval, new BoardCell[] { WestFlower, MiddleFlower }));

            IReadOnlyList<Flower> flowersOnTable = table.FindFlowers();
            Assert.Contains(new Flower(MiddleFlower, SampleLayouts.LevelOne), flowersOnTable);
        }

        [Fact]
        public void Test_PlayCard_FlowerLevelRemovalTwiceOnTheSameFlower_ReturnsWrongCardTarget()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelThree), new Flower(MiddleFlower, SampleLayouts.LevelOne) };
            MatchTable table = CreateCardTable(flowers, Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.FlowerLevelRemoval }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.FlowerLevelRemoval, new BoardCell[] { WestFlower, WestFlower }));

            Assert.Equal(TurnActionOutcome.WrongCardTarget, playOutcome);
        }

        [Fact]
        public void Test_PlayCard_FlowerLevelRemovalUnderCaterpillar_ReturnsCellOccupied()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelTwo), new Flower(MiddleFlower, SampleLayouts.LevelOne) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberTwo, WestFlower) }, new TurnHolding(new ActionCard[] { ActionCard.FlowerLevelRemoval }, MatchTable.ActionPointsPerTurn));

            TurnActionOutcome playOutcome = table.CardTable.PlayCard(SampleRules.SeatNumberOne, new CardPlay(ActionCard.FlowerLevelRemoval, new BoardCell[] { WestFlower }));

            Assert.Equal(TurnActionOutcome.CellOccupied, playOutcome);
        }

        [Fact]
        public void Test_CanDrawCard_OnOwnTurnWithCardsInTheDeck_ReturnsTrue()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(Array.Empty<ActionCard>(), MatchTable.ActionPointsPerTurn));

            bool canDraw = table.CardTable.CanDrawCard(SampleRules.SeatNumberOne);

            Assert.True(canDraw);
        }

        [Fact]
        public void Test_CanDrawCard_OutOfTurn_ReturnsFalse()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(Array.Empty<ActionCard>(), MatchTable.ActionPointsPerTurn));

            bool canDraw = table.CardTable.CanDrawCard(SampleRules.SeatNumberTwo);

            Assert.False(canDraw);
        }

        [Fact]
        public void Test_FindPlayableCards_WithoutActionPoints_LeavesOutTheCardThatCosts()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.SevenActionPoints, ActionCard.PlanterCrossing }, SampleRules.NoActionPoints));

            IReadOnlyList<ActionCard> playableCards = table.CardTable.FindPlayableCards(SampleRules.SeatNumberOne);

            Assert.Equal(new ActionCard[] { ActionCard.SevenActionPoints }, playableCards);
        }

        [Fact]
        public void Test_FindPlayableCards_OutOfTurn_ReturnsNoCards()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(new ActionCard[] { ActionCard.SevenActionPoints }, MatchTable.ActionPointsPerTurn));

            IReadOnlyList<ActionCard> playableCards = table.CardTable.FindPlayableCards(SampleRules.SeatNumberTwo);

            Assert.Empty(playableCards);
        }

        [Fact]
        public void Test_FindLevelRemovalTargets_WithOccupiedAndSingleLevelFlowers_ReturnsOnlyTheFreeTallFlower()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelTwo), new Flower(MiddleFlower, SampleLayouts.LevelThree), new Flower(EastFlower, SampleLayouts.LevelOne) };
            MatchTable table = CreateCardTable(flowers, new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberTwo, WestFlower) }, new TurnHolding(Array.Empty<ActionCard>(), MatchTable.ActionPointsPerTurn));

            IReadOnlyList<BoardCell> removalTargets = table.CardTable.FindLevelRemovalTargets();

            Assert.Equal(new BoardCell[] { MiddleFlower }, removalTargets);
        }

        [Fact]
        public void Test_EndTurn_ClosingTheRoundWithKeptCards_TurnsEachKeptCardIntoOnePoint()
        {
            var seatOneHand = new ActionCard[] { ActionCard.DoubleClimb, ActionCard.GroundLeap };
            var seatCards = new SeatCards[] { new SeatCards(SampleRules.SeatNumberOne, seatOneHand, ActionCardTable.FullDeck.Except(seatOneHand).ToList(), false), new SeatCards(SampleRules.SeatNumberTwo, Array.Empty<ActionCard>(), ActionCardTable.FullDeck, false), };
            var caterpillars = new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower), new Caterpillar(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower) };
            var snapshot = new MatchSnapshot(TablePhase.PlayerTurn, MatchTable.FirstRoundNumber, MatchTable.FirstTurnRoundNumber, SampleRules.TwoSeatCount, SampleRules.SeatNumberTwo, MatchTable.ActionPointsPerTurn, SampleRules.ThreeBuildings, SampleLayouts.FiveStartingFlowers, caterpillars, SampleLayouts.SouthEastFlower, Array.Empty<SeatStanding>(), seatCards, Array.Empty<int>(), null);
            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.OneTurnPerRoundRules, SampleRules.CreateChanceSource());

            table.EndTurn(SampleRules.SeatNumberTwo);

            Assert.Equal(TwoKeptCards, table.Standings[0].CardPoints);
        }

        [Fact]
        public void Test_EndTurn_ClosingTheRoundWithKeptCards_EmptiesTheHand()
        {
            var seatOneHand = new ActionCard[] { ActionCard.DoubleClimb, ActionCard.GroundLeap };
            var seatCards = new SeatCards[] { new SeatCards(SampleRules.SeatNumberOne, seatOneHand, ActionCardTable.FullDeck.Except(seatOneHand).ToList(), false), new SeatCards(SampleRules.SeatNumberTwo, Array.Empty<ActionCard>(), ActionCardTable.FullDeck, false), };
            var caterpillars = new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower), new Caterpillar(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower) };
            var snapshot = new MatchSnapshot(TablePhase.PlayerTurn, MatchTable.FirstRoundNumber, MatchTable.FirstTurnRoundNumber, SampleRules.TwoSeatCount, SampleRules.SeatNumberTwo, MatchTable.ActionPointsPerTurn, SampleRules.ThreeBuildings, SampleLayouts.FiveStartingFlowers, caterpillars, SampleLayouts.SouthEastFlower, Array.Empty<SeatStanding>(), seatCards, Array.Empty<int>(), null);
            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.OneTurnPerRoundRules, SampleRules.CreateChanceSource());

            table.EndTurn(SampleRules.SeatNumberTwo);

            Assert.Empty(table.CardTable.SeatCards[0].Hand);
        }

        [Fact]
        public void Test_EndTurn_ClosingTheRoundWithKeptCards_RefillsTheDeck()
        {
            var seatOneHand = new ActionCard[] { ActionCard.DoubleClimb, ActionCard.GroundLeap };
            var seatCards = new SeatCards[] { new SeatCards(SampleRules.SeatNumberOne, seatOneHand, ActionCardTable.FullDeck.Except(seatOneHand).ToList(), false), new SeatCards(SampleRules.SeatNumberTwo, Array.Empty<ActionCard>(), ActionCardTable.FullDeck, false), };
            var caterpillars = new Caterpillar[] { new Caterpillar(SampleRules.SeatNumberOne, SampleLayouts.NorthWestFlower), new Caterpillar(SampleRules.SeatNumberTwo, SampleLayouts.NorthEastFlower) };
            var snapshot = new MatchSnapshot(TablePhase.PlayerTurn, MatchTable.FirstRoundNumber, MatchTable.FirstTurnRoundNumber, SampleRules.TwoSeatCount, SampleRules.SeatNumberTwo, MatchTable.ActionPointsPerTurn, SampleRules.ThreeBuildings, SampleLayouts.FiveStartingFlowers, caterpillars, SampleLayouts.SouthEastFlower, Array.Empty<SeatStanding>(), seatCards, Array.Empty<int>(), null);
            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.OneTurnPerRoundRules, SampleRules.CreateChanceSource());

            table.EndTurn(SampleRules.SeatNumberTwo);

            Assert.Equal(ActionCardTable.FullDeck.Count, table.CardTable.SeatCards[0].Deck.Count);
        }

        [Fact]
        public void Test_FindFinishingOrder_WithCardPoints_PutsTheHigherFinalScoreFirst()
        {
            var standings = new SeatStanding[] { new SeatStanding(SampleRules.SeatNumberOne, new SeatRoundScore[] { new SeatRoundScore(SamePoints, 0, SampleLayouts.OneScoredPlanter) }, false, NoCardPoints), new SeatStanding(SampleRules.SeatNumberTwo, new SeatRoundScore[] { new SeatRoundScore(SamePoints, 0, SampleLayouts.OneScoredPlanter) }, false, OneCardPoint) };
            var snapshot = new MatchSnapshot(TablePhase.Finished, MatchRules.RoundCount, MatchTable.FirstTurnRoundNumber, SampleRules.TwoSeatCount, SampleRules.SeatNumberOne, SampleRules.NoActionPoints, 0, CreateFlatPlanter(), Array.Empty<Caterpillar>(), null, standings, Array.Empty<SeatCards>(), Array.Empty<int>(), FinishCause.Completed);
            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.TwoSeatRules, SampleRules.CreateChanceSource());

            int winnerSeatNumber = table.FindFinishingOrder()[0].SeatNumber;

            Assert.Equal(SampleRules.SeatNumberTwo, winnerSeatNumber);
        }

        [Fact]
        public void Test_TakeSnapshot_AfterADraw_KeepsTheHand()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(Array.Empty<ActionCard>(), MatchTable.ActionPointsPerTurn));
            table.CardTable.DrawCard(SampleRules.SeatNumberOne);

            MatchTable restoredTable = MatchTable.RestoreFromSnapshot(table.TakeSnapshot(), SampleRules.TwoSeatRules, SampleRules.CreateChanceSource());

            Assert.Equal(table.CardTable.SeatCards[0].Hand, restoredTable.CardTable.SeatCards[0].Hand);
        }

        [Fact]
        public void Test_TakeSnapshot_AfterADraw_KeepsTheDeck()
        {
            MatchTable table = CreateCardTable(CreateFlatPlanter(), Array.Empty<Caterpillar>(), new TurnHolding(Array.Empty<ActionCard>(), MatchTable.ActionPointsPerTurn));
            table.CardTable.DrawCard(SampleRules.SeatNumberOne);

            MatchTable restoredTable = MatchTable.RestoreFromSnapshot(table.TakeSnapshot(), SampleRules.TwoSeatRules, SampleRules.CreateChanceSource());

            Assert.Equal(SevenCardsLeft, restoredTable.CardTable.SeatCards[0].Deck.Count);
        }

        [Fact]
        public void Test_RestoreFromSnapshot_WithCardsOfOneSeatOnly_ThrowsArgumentException()
        {
            var seatCards = new SeatCards[] { new SeatCards(SampleRules.SeatNumberOne, Array.Empty<ActionCard>(), ActionCardTable.FullDeck, false) };
            var snapshot = new MatchSnapshot(TablePhase.PlayerTurn, MatchTable.FirstRoundNumber, MatchTable.FirstTurnRoundNumber, SampleRules.TwoSeatCount, SampleRules.SeatNumberOne, MatchTable.ActionPointsPerTurn, SampleRules.ThreeBuildings, CreateFlatPlanter(), Array.Empty<Caterpillar>(), null, Array.Empty<SeatStanding>(), seatCards, Array.Empty<int>(), null);

            Assert.Throws<ArgumentException>(() => MatchTable.RestoreFromSnapshot(snapshot, SampleRules.TwoSeatRules, SampleRules.CreateChanceSource()));
        }

        private static Flower[] CreateFlatPlanter()
        {
            var flowers = new Flower[] { new Flower(WestFlower, SampleLayouts.LevelOne), new Flower(MiddleFlower, SampleLayouts.LevelOne) };

            return flowers;
        }

        private static MatchTable CreateCardTable(IReadOnlyList<Flower> flowers, IReadOnlyList<Caterpillar> caterpillars, TurnHolding seatOneTurn)
        {
            var seatCards = new SeatCards[]
            {
                new SeatCards(SampleRules.SeatNumberOne, seatOneTurn.Hand, ActionCardTable.FullDeck.Except(seatOneTurn.Hand).ToList(), false),
                new SeatCards(SampleRules.SeatNumberTwo, Array.Empty<ActionCard>(), ActionCardTable.FullDeck, false),
            };
            var snapshot = new MatchSnapshot(TablePhase.PlayerTurn, MatchTable.FirstRoundNumber, MatchTable.FirstTurnRoundNumber, SampleRules.TwoSeatCount, SampleRules.SeatNumberOne, seatOneTurn.ActionPointsLeft, SampleRules.ThreeBuildings, flowers, caterpillars, null, Array.Empty<SeatStanding>(), seatCards, Array.Empty<int>(), null);
            MatchTable table = MatchTable.RestoreFromSnapshot(snapshot, SampleRules.TwoSeatRules, SampleRules.CreateChanceSource());

            return table;
        }
    }
}
