using System;
using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Scene;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class CardHandView
    {
        private const int CardCellCount = 2;

        private static readonly IReadOnlyList<BoardCell> _allBoardCells = CreateAllBoardCells();

        private readonly VerticalStackPanel _handPanel = new VerticalStackPanel { Spacing = MatchLayout.HandSpacing };
        private readonly LocalizedLabel _drawLabel = new LocalizedLabel(TextKeys.Match.DrawButton) { Font = Fonts.Small, TextColor = Theme.SoftInk };
        private readonly LocalizedLabel _deckCountLabel = new LocalizedLabel(TextKeys.Match.DeckCountLabel) { Font = Fonts.Label, TextColor = Theme.MutedInk };
        private readonly Button _drawButton;
        private readonly List<BoardCell> _chosenCells = new List<BoardCell>();

        private MatchCard? _chosenCard;
        private MatchCardPlay? _readyPlay;
        private bool _isDrawRequested;
        private bool _hasChoiceChanged;

        internal CardHandView()
        {
            _drawButton = new Button
            {
                Content = _drawLabel,
                Padding = Metrics.SmallButtonPadding,
                Background = Theme.SurfaceBrush,
                OverBackground = Theme.SurfaceSunkenBrush,
                PressedBackground = Theme.SurfaceSunkenBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            _drawButton.Click += DrawButtonOnClick;
            DeckRoot = BuildDeckRoot();
        }

        internal Widget DeckRoot { get; }

        internal Widget HandRoot => _handPanel;

        internal bool IsChoosingCells => _chosenCard is not null;

        internal void Show(MatchView matchView, bool canAct)
        {
            bool isChosenCardGone = (_chosenCard is MatchCard chosenCard) && !matchView.Hand.Contains(chosenCard);
            if (!canAct || isChosenCardGone)
            {
                CancelChoice();
            }

            _drawLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(matchView.ActionCosts.CardDraw) };
            _deckCountLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(matchView.DeckCount) };
            _drawButton.Enabled = canAct && matchView.CanDrawCard;
            _handPanel.Widgets.Clear();
            foreach (MatchCard handCard in matchView.Hand)
            {
                bool isPlayable = canAct && matchView.PlayableCards.Contains(handCard);
                _handPanel.Widgets.Add(BuildHandCardButton(handCard, isPlayable));
            }
        }

        internal void CancelChoice()
        {
            _chosenCard = null;
            _chosenCells.Clear();
        }

        internal void ChooseCell(BoardCell clickedCell)
        {
            if ((_chosenCard is not MatchCard chosenCard) || _chosenCells.Contains(clickedCell))
            {
                return;
            }

            _chosenCells.Add(clickedCell);
            if (_chosenCells.Count == CardCellCount)
            {
                _readyPlay = CreatePlay(chosenCard);
                CancelChoice();
            }

            _hasChoiceChanged = true;
        }

        internal bool TakeDrawRequest()
        {
            bool isDrawRequested = _isDrawRequested;
            _isDrawRequested = false;

            return isDrawRequested;
        }

        internal MatchCardPlay? TakeReadyPlay()
        {
            MatchCardPlay? readyPlay = _readyPlay;
            _readyPlay = null;

            return readyPlay;
        }

        internal bool TakeChoiceChange()
        {
            bool hasChoiceChanged = _hasChoiceChanged;
            _hasChoiceChanged = false;

            return hasChoiceChanged;
        }

        internal IReadOnlyList<BoardCell> FindTargetCells(MatchView matchView)
        {
            IReadOnlyList<BoardCell> targetCells = Array.Empty<BoardCell>();
            if (_chosenCard is MatchCard chosenCard)
            {
                targetCells = chosenCard switch
                {
                    MatchCard.FlowerLevelRemoval => FindLevelRemovalTargets(matchView).Except(_chosenCells).ToList(),
                    MatchCard.FlowerLevelMove when _chosenCells.Count == 0 => FindLevelRemovalTargets(matchView),
                    MatchCard caterpillarCard when IsCaterpillarCard(caterpillarCard) && (_chosenCells.Count == 0) => FindOwnCaterpillars(matchView),
                    MatchCard destinationCard when IsCaterpillarCard(destinationCard) || (destinationCard == MatchCard.FlowerLevelMove) => _allBoardCells.Except(_chosenCells).ToList(),
                    MatchCard.SixActionPoints or MatchCard.SevenActionPoints => throw new ArgumentOutOfRangeException(nameof(chosenCard)),
                    _ => throw new ArgumentOutOfRangeException(nameof(chosenCard)),
                };
            }

            return targetCells;
        }

        internal string? FindHintKey()
        {
            string? hintKey = null;
            if (_chosenCard is MatchCard chosenCard)
            {
                hintKey = chosenCard switch
                {
                    MatchCard.FlowerLevelRemoval => TextKeys.Match.ChooseRemovalHint,
                    MatchCard.FlowerLevelMove when _chosenCells.Count == 0 => TextKeys.Match.ChooseLevelSourceHint,
                    MatchCard.FlowerLevelMove => TextKeys.Match.ChooseLevelDestinationHint,
                    MatchCard caterpillarCard when IsCaterpillarCard(caterpillarCard) && (_chosenCells.Count == 0) => TextKeys.Match.ChooseCardCaterpillarHint,
                    MatchCard caterpillarCard when IsCaterpillarCard(caterpillarCard) => TextKeys.Match.ChooseCardDestinationHint,
                    MatchCard.SixActionPoints or MatchCard.SevenActionPoints => throw new ArgumentOutOfRangeException(nameof(chosenCard)),
                    _ => throw new ArgumentOutOfRangeException(nameof(chosenCard)),
                };
            }

            return hintKey;
        }

        private static List<BoardCell> CreateAllBoardCells()
        {
            List<BoardCell> allBoardCells = Enumerable.Range(0, BoardLayout.CellsPerSide)
                .SelectMany(row => Enumerable.Range(0, BoardLayout.CellsPerSide).Select(column => new BoardCell(row, column)))
                .ToList();

            return allBoardCells;
        }

        private static bool IsPointsCard(MatchCard card)
        {
            bool isPointsCard = card is MatchCard.SixActionPoints or MatchCard.SevenActionPoints;

            return isPointsCard;
        }

        private static bool IsCaterpillarCard(MatchCard card)
        {
            bool isClimbOrLeapCard = card is MatchCard.DoubleClimb or MatchCard.GroundLeap;
            bool isRelocationCard = card is MatchCard.Relocation or MatchCard.PlanterCrossing;
            bool isCaterpillarCard = isClimbOrLeapCard || isRelocationCard;

            return isCaterpillarCard;
        }

        private static string ToCardKey(MatchCard card)
        {
            string cardKey = card switch
            {
                MatchCard.SixActionPoints => TextKeys.Match.SixActionPointsCard,
                MatchCard.SevenActionPoints => TextKeys.Match.SevenActionPointsCard,
                MatchCard.DoubleClimb => TextKeys.Match.DoubleClimbCard,
                MatchCard.GroundLeap => TextKeys.Match.GroundLeapCard,
                MatchCard.FlowerLevelMove => TextKeys.Match.FlowerLevelMoveCard,
                MatchCard.Relocation => TextKeys.Match.RelocationCard,
                MatchCard.PlanterCrossing => TextKeys.Match.PlanterCrossingCard,
                MatchCard.FlowerLevelRemoval => TextKeys.Match.FlowerLevelRemovalCard,
                _ => throw new ArgumentOutOfRangeException(nameof(card)),
            };

            return cardKey;
        }

        private static List<BoardCell> FindLevelRemovalTargets(MatchView matchView)
        {
            List<BoardCell> removalTargets = matchView.LevelRemovalTargets.Select(MatchCellConversion.ToBoardCell).ToList();

            return removalTargets;
        }

        private static List<BoardCell> FindOwnCaterpillars(MatchView matchView)
        {
            List<BoardCell> ownCaterpillars = matchView.Caterpillars
                .Where(caterpillar => caterpillar.SeatNumber == matchView.ViewerSeatNumber)
                .Select(caterpillar => MatchCellConversion.ToBoardCell(caterpillar.Cell))
                .ToList();

            return ownCaterpillars;
        }

        private VerticalStackPanel BuildDeckRoot()
        {
            var deckRoot = new VerticalStackPanel
            {
                Spacing = MatchLayout.HandSpacing,
            };
            deckRoot.Widgets.Add(_drawButton);
            deckRoot.Widgets.Add(_deckCountLabel);

            return deckRoot;
        }

        private LocalizedButton BuildHandCardButton(MatchCard handCard, bool isPlayable)
        {
            var cardButton = new LocalizedButton(ToCardKey(handCard))
            {
                LabelFont = Fonts.Small,
                LabelColor = Theme.Ink,
                Padding = Metrics.SmallButtonPadding,
                Background = _chosenCard == handCard ? Theme.MintTintBrush : Theme.SurfaceBrush,
                OverBackground = Theme.SurfaceSunkenBrush,
                PressedBackground = Theme.SurfaceSunkenBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Enabled = isPlayable,
                Tag = handCard,
            };
            cardButton.Click += CardButtonOnClick;

            return cardButton;
        }

        private MatchCardPlay CreatePlay(MatchCard playedCard)
        {
            var cardPlay = new MatchCardPlay
            {
                Card = playedCard,
                AffectedCells = _chosenCells.Select(MatchCellConversion.ToMatchCell).ToList(),
            };

            return cardPlay;
        }

        private void ChooseCard(MatchCard clickedCard)
        {
            bool isRemovalWithOneFlower = (_chosenCard == clickedCard) && (clickedCard == MatchCard.FlowerLevelRemoval) && (_chosenCells.Count > 0);
            if (IsPointsCard(clickedCard))
            {
                CancelChoice();
                _readyPlay = CreatePlay(clickedCard);
            }
            else if (isRemovalWithOneFlower)
            {
                _readyPlay = CreatePlay(clickedCard);
                CancelChoice();
            }
            else if (_chosenCard == clickedCard)
            {
                CancelChoice();
            }
            else
            {
                _chosenCard = clickedCard;
                _chosenCells.Clear();
            }

            _hasChoiceChanged = true;
        }

        private void CardButtonOnClick(object sender, MyraEventArgs e)
        {
            if (sender is Widget { Tag: MatchCard clickedCard })
            {
                ChooseCard(clickedCard);
            }
        }

        private void DrawButtonOnClick(object sender, MyraEventArgs e)
        {
            _isDrawRequested = true;
        }
    }
}
