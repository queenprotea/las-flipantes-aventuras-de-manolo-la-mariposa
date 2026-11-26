using System;
using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Scene;
using Torres.Client.UI;

namespace Torres.Client.Widgets
{
    internal sealed class CardHandView
    {
        private const int CardCellCount = 2;
        private const float DisabledOpacity = 0.5f;

        private static readonly IReadOnlyList<BoardCell> _allBoardCells = CreateAllBoardCells();

        private readonly LocalizedLabel _drawLabel = new LocalizedLabel(TextKeys.Match.DrawButton) { Font = Fonts.Small, TextColor = Theme.Ink };
        private readonly LocalizedLabel _deckCountLabel = new LocalizedLabel(TextKeys.Match.DeckCountLabel) { Font = Fonts.Small, TextColor = Theme.MutedInk };
        private readonly Button _drawButton;
        private readonly List<BoardCell> _chosenCells = new List<BoardCell>();
        private readonly List<HandCardState> _handStates = new List<HandCardState>();

        private MatchCard? _chosenCard;
        private MatchCardPlay? _readyPlay;
        private bool _isDrawRequested;
        private bool _hasChoiceChanged;

        internal CardHandView(VolumeTextures volumeTextures)
        {
            _drawButton = new Button
            {
                Content = _drawLabel,
                Padding = MatchLayout.ActionPillPadding,
                Background = volumeTextures.PillBrush,
                OverBackground = volumeTextures.HoveredPillBrush,
                PressedBackground = volumeTextures.ChosenPillBrush,
                DisabledBackground = volumeTextures.PillBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            _drawButton.Click += DrawButtonOnClick;
            DeckRoot = BuildDeckRoot();
        }

        internal Widget DeckRoot { get; }

        internal bool IsChoosingCells => _chosenCard is not null;

        internal IReadOnlyList<HandCardState> HandStates => _handStates;

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
            _drawButton.Opacity = _drawButton.Enabled ? 1f : DisabledOpacity;
            _handStates.Clear();
            foreach (MatchCard handCard in matchView.Hand)
            {
                bool isPlayable = canAct && matchView.PlayableCards.Contains(handCard);
                _handStates.Add(new HandCardState(handCard, isPlayable, _chosenCard == handCard));
            }
        }

        internal void ChooseCardOnTable(MatchCard clickedCard)
        {
            if (_handStates.Exists(handState => (handState.Card == clickedCard) && handState.IsPlayable))
            {
                ChooseCard(clickedCard);
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
            var deckTitle = new HorizontalStackPanel { Spacing = MatchLayout.TableLabelSpacing, HorizontalAlignment = HorizontalAlignment.Center };
            deckTitle.Widgets.Add(new LocalizedLabel(TextKeys.Match.DeckLabel) { Font = Fonts.Small, TextColor = Theme.MutedInk });
            deckTitle.Widgets.Add(new Label { Text = DisplaySymbols.InlineSeparator, Font = Fonts.Small, TextColor = Theme.MutedInk });
            deckTitle.Widgets.Add(_deckCountLabel);
            var deckRoot = new VerticalStackPanel { Spacing = MatchLayout.HandSpacing };
            deckRoot.Widgets.Add(deckTitle);
            deckRoot.Widgets.Add(_drawButton);

            return deckRoot;
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

        private void DrawButtonOnClick(object sender, MyraEventArgs e)
        {
            _isDrawRequested = true;
        }
    }
}
