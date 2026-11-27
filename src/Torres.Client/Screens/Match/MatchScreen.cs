using System;
using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Scene;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class MatchScreen : Screen, IDisposable
    {
        private const int CenteringDivisor = 2;

        private readonly MatchFlow _matchFlow;
        private readonly BoardWidget _boardWidget;
        private readonly FigureIcons _figureIcons;
        private readonly VolumeTextures _volumeTextures;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly CardHandView _cardHand;
        private readonly LocalizedLabel _hintLabel = new LocalizedLabel(TextKeys.Match.ChoosePlacementHint)
        {
            Font = Fonts.Small,
            TextColor = Theme.Surface,
        };
        private readonly Panel _hintPill;
        private readonly MatchPlayersBarView _playersBar;
        private readonly MatchClockView _clockView;
        private readonly LocalizedLabel _reserveCaterpillarsLabel = new LocalizedLabel(TextKeys.Match.YourCaterpillarsLabel)
        {
            Font = Fonts.Small,
            TextColor = Theme.Ink,
        };
        private readonly Label _reserveCaterpillarsCount = new Label
        {
            Font = Fonts.Small,
            TextColor = Theme.Ink,
        };
        private readonly Label _reserveBuildsCount = new Label
        {
            Font = Fonts.Small,
            TextColor = Theme.Ink,
        };
        private readonly Label _pointsLabel = new Label
        {
            Font = Fonts.Title,
            TextColor = Theme.Ink,
            VerticalAlignment = VerticalAlignment.Center,
        };
        private readonly Panel _caterpillarsTag;
        private readonly Panel _buildsTag;
        private readonly Panel _pointsTag;
        private readonly Panel _deckTag;
        private readonly ActionBarView _actionBar;
        private readonly Button _endTurnButton;
        private readonly MatchTableFrame _tableFrame;
        private readonly ForfeitSection _forfeitSection;
        private readonly ChatSection _chatSection;

        private MatchView? _shownMatchView;
        private TurnActionMode _actionMode;
        private BoardCell? _moveOrigin;
        private TimeSpan _shownStepTime;
        private bool _wasEscapeDown;
        private bool _isShowingCardEffect;

        internal MatchScreen(GraphicsDevice graphicsDevice, MatchFlow matchFlow, ForfeitFlow forfeitFlow, ChatFlow chatFlow, MatchPause matchPause)
            : base(TextKeys.Match.HeaderLabel, true)
        {
            _matchFlow = matchFlow;
            _forfeitSection = new ForfeitSection(forfeitFlow, TextKeys.Match.ForfeitButton);
            _chatSection = new ChatSection(chatFlow, MatchLayout.SideColumnWidth);
            _boardWidget = new BoardWidget(graphicsDevice, BoardLayout.Empty);
            _figureIcons = matchPause.FigureIcons;
            _volumeTextures = matchPause.VolumeTextures;
            _tableFrame = new MatchTableFrame(_volumeTextures, _chatSection.Root, _noticeBox);
            _cardHand = new CardHandView(_volumeTextures);
            _playersBar = new MatchPlayersBarView(_figureIcons, _volumeTextures);
            _clockView = new MatchClockView(_volumeTextures);
            _caterpillarsTag = BuildCountTag(_reserveCaterpillarsLabel, _reserveCaterpillarsCount);
            _buildsTag = BuildCountTag(new LocalizedLabel(TextKeys.Match.BuildingsLabel)
            {
                Font = Fonts.Small,
                TextColor = Theme.Ink,
            }, _reserveBuildsCount);
            _pointsTag = BuildPointsTag();
            _deckTag = CreateTableTag(_cardHand.DeckRoot);
            _actionBar = new ActionBarView(_figureIcons, _volumeTextures);
            _hintPill = _tableFrame.CreateHintPill(_hintLabel);
            _hintPill.Visible = false;
            _endTurnButton = new Button
            {
                Content = new LocalizedLabel(TextKeys.Match.EndTurnButton)
                {
                    Font = Fonts.Body,
                    TextColor = Theme.Surface,
                    HorizontalAlignment = HorizontalAlignment.Center,
                },
                Padding = MatchLayout.EndTurnPillPadding,
                Background = _volumeTextures.ChosenPillBrush,
                OverBackground = _volumeTextures.ChosenPillBrush,
                PressedBackground = _volumeTextures.ChosenPillBrush,
                DisabledBackground = _volumeTextures.ChosenPillBrush,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            _endTurnButton.Click += EndTurnButtonOnClick;
        }

        internal override bool KeepsEscapeKey => true;

        internal override void Open()
        {
            _noticeBox.Hide();
            _forfeitSection.Reset();
            _chatSection.Open();
            _shownMatchView = null;
            _actionMode = TurnActionMode.None;
            _moveOrigin = null;
            _cardHand.CancelChoice();
            _boardWidget.RefreshCardFaces();
            _wasEscapeDown = Keyboard.GetState().IsKeyDown(Keys.Escape);
            HeaderArguments = new string[] { _matchFlow.RoomCode };
            RequestedScreen = _matchFlow.Open();
        }

        internal override void Update(GameTime gameTime)
        {
            _chatSection.Update();
            MatchView? matchView = _matchFlow.CurrentMatchView;
            if (matchView is null)
            {
                return;
            }

            if (!ReferenceEquals(matchView, _shownMatchView))
            {
                ShowMatchView(matchView);
            }

            ShowClock();
            _boardWidget.Advance(gameTime.ElapsedGameTime);
            ShowActionPoints(matchView, _actionBar.HoveredCost);
            PlaceTableTags();
            ShowCardEffect(matchView);
            _boardWidget.UpdateInput();
            FollowEscapeKey(matchView);
            FollowActionBar();
            FollowCardClick();
            FollowBoardClick(matchView);
            FollowCardHand(matchView);
            FollowActionOutcome();
            ScreenId? nextScreen = _forfeitSection.TakeNextScreen() ?? MatchFlow.FindNextScreen(matchView);
            if (nextScreen is not null)
            {
                RequestedScreen = nextScreen;
            }
        }

        internal override void RenderScene()
        {
            _figureIcons.RenderOnce();
            _boardWidget.RenderScene();
        }

        public void Dispose()
        {
            _boardWidget.Dispose();
        }

        protected override Widget Build()
        {
            Panel tablePanel = MatchTableFrame.CreateTablePanel(_boardWidget);
            tablePanel.Widgets.Add(_caterpillarsTag);
            tablePanel.Widgets.Add(_buildsTag);
            tablePanel.Widgets.Add(_pointsTag);
            tablePanel.Widgets.Add(_deckTag);
            tablePanel.Widgets.Add(MatchTableFrame.CreateTopBar(_playersBar.Root, _clockView.Root));
            tablePanel.Widgets.Add(_hintPill);
            _tableFrame.CompleteTablePanel(tablePanel, BuildRightColumn());

            return tablePanel;
        }

        private static List<BoardCell> FindMoveOrigins(MatchView matchView)
        {
            List<BoardCell> moveOrigins = matchView.MoveTargets.Select(moveTarget => MatchCellConversion.ToBoardCell(moveTarget.Origin)).Distinct().ToList();

            return moveOrigins;
        }

        private static List<BoardCell> FindMoveDestinations(MatchView matchView, BoardCell originCell)
        {
            List<BoardCell> moveDestinations = matchView.MoveTargets.Where(moveTarget => MatchCellConversion.ToBoardCell(moveTarget.Origin) == originCell).Select(moveTarget => MatchCellConversion.ToBoardCell(moveTarget.Destination)).ToList();

            return moveDestinations;
        }

        private VerticalStackPanel BuildRightColumn()
        {
            _forfeitSection.Root.HorizontalAlignment = HorizontalAlignment.Right;
            var lowerSpace = new Panel();
            var rightColumn = new VerticalStackPanel
            {
                Spacing = MatchLayout.RightColumnSpacing,
                Width = MatchLayout.ActionColumnWidth,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = MatchLayout.RightColumnMargin,
            };
            rightColumn.Widgets.Add(_forfeitSection.Root);
            rightColumn.Widgets.Add(_actionBar.Root);
            rightColumn.Widgets.Add(_tableFrame.ChatTabButton);
            rightColumn.Widgets.Add(lowerSpace);
            rightColumn.Widgets.Add(_endTurnButton);
            StackPanel.SetProportionType(lowerSpace, ProportionType.Fill);

            return rightColumn;
        }

        private Panel CreateTableTag(Widget tagContent)
        {
            var tableTag = new Panel
            {
                Padding = MatchLayout.TableLabelPadding,
                Background = _volumeTextures.PillBrush,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
            };
            tableTag.Widgets.Add(tagContent);

            return tableTag;
        }

        private Panel BuildCountTag(Widget countLabel, Widget countValue)
        {
            var countRow = new HorizontalStackPanel();
            countRow.Widgets.Add(countLabel);
            countRow.Widgets.Add(countValue);
            Panel countTag = CreateTableTag(countRow);

            return countTag;
        }

        private Panel BuildPointsTag()
        {
            var pointsRow = new HorizontalStackPanel { Spacing = MatchLayout.TableLabelSpacing };
            pointsRow.Widgets.Add(_pointsLabel);
            pointsRow.Widgets.Add(new LocalizedLabel(TextKeys.Match.ActionPointsLabel)
            {
                Font = Fonts.Title,
                TextColor = Theme.PointsInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            Panel pointsTag = CreateTableTag(pointsRow);

            return pointsTag;
        }

        private void PlaceTableTags()
        {
            PlaceTableTag(_caterpillarsTag, SproutFigure.CaterpillarsLabelAnchor);
            PlaceTableTag(_buildsTag, SproutFigure.BuildsLabelAnchor);
            PlaceTableTag(_pointsTag, SproutFigure.PointsLabelAnchor);
            PlaceTableTag(_deckTag, DeckFigure.LabelAnchor);
        }

        private void PlaceTableTag(Widget tableTag, Vector3 tagAnchor)
        {
            if (_boardWidget.ProjectToWidget(tagAnchor, true) is Point anchorPoint)
            {
                tableTag.Left = anchorPoint.X - (tableTag.ActualBounds.Width / CenteringDivisor);
                tableTag.Top = anchorPoint.Y;
            }
        }

        private void ShowActionPoints(MatchView matchView, int previewCost)
        {
            bool isPreviewShown = (previewCost > 0) && (previewCost <= matchView.ActionPointsLeft);
            int shownCost = isPreviewShown ? previewCost : 0;
            _boardWidget.ShowActionPoints(matchView.ActionPointsLeft, shownCost);
            _pointsLabel.Text = isPreviewShown ? DisplayFormats.FormatPointsPreview(matchView.ActionPointsLeft, matchView.ActionPointsLeft - previewCost) : DisplayFormats.FormatInteger(matchView.ActionPointsLeft);
        }

        private void ShowClock()
        {
            TimeSpan stepTimeLeft = _matchFlow.StepTimeLeft;

            float remainingProgress = _shownStepTime > TimeSpan.Zero ? (float)(stepTimeLeft / _shownStepTime) : 0f;
            _clockView.ShowClock(stepTimeLeft, remainingProgress);
        }

        private void ShowMatchView(MatchView matchView)
        {
            bool isFirstView = _shownMatchView is null;
            _shownMatchView = matchView;
            _shownStepTime = matchView.StepTimeLeft ?? TimeSpan.Zero;
            _moveOrigin = null;
            if (!MatchFlow.IsViewerPlaying(matchView))
            {
                _actionMode = TurnActionMode.None;
            }

            MatchSeat viewerSeat = matchView.Seats.First(seat => seat.SeatNumber == matchView.ViewerSeatNumber);
            _clockView.Show(matchView, MatchFlow.IsViewerPlaying(matchView));
            _playersBar.Show(matchView);
            _boardWidget.ShowReserve(viewerSeat.CaterpillarsLeft, SeatColors.ColorFor(viewerSeat.Color), matchView.BuildsLeft);
            _reserveCaterpillarsCount.Text = DisplayFormats.FormatLabeledCount(viewerSeat.CaterpillarsLeft);
            _reserveBuildsCount.Text = DisplayFormats.FormatLabeledCount(matchView.BuildsLeft);
            BoardLayout boardLayout = MatchBoardLayoutFactory.Create(matchView);
            if (isFirstView)
            {
                _boardWidget.ShowLayoutWithoutMotion(boardLayout);
            }
            else
            {
                _boardWidget.ShowLayout(boardLayout);
            }

            BoardCell? butterflyCell = null;
            if (matchView.HasButterfly)
            {
                butterflyCell = MatchCellConversion.ToBoardCell(matchView.ButterflyCell);
            }

            _boardWidget.ShowButterflyCell(butterflyCell);
            RefreshActionControls(matchView);
        }

        private void RefreshActionControls(MatchView matchView)
        {
            bool canAct = MatchFlow.IsViewerPlaying(matchView) && !_matchFlow.IsActing;
            _endTurnButton.Enabled = canAct;
            _endTurnButton.Opacity = canAct ? 1f : MatchLayout.DisabledPillOpacity;
            _cardHand.Show(matchView, canAct);
            _boardWidget.ShowHand(_cardHand.HandStates, matchView.DeckCount);
            IReadOnlyList<BoardCell> targetCells = FindTargetCells(matchView);
            if (targetCells.Count == 0)
            {
                _actionMode = TurnActionMode.None;
            }

            _actionBar.Show(matchView, canAct, _actionMode);
            RefreshHint(matchView);
            _boardWidget.ShowPlacementTargets(FindTargetCells(matchView));
        }

        private void ShowCardEffect(MatchView matchView)
        {
            string? effectKey = _boardWidget.HoveredCard is MatchCard hoveredCard ? CardTexts.ToEffectKey(hoveredCard) : null;
            if (effectKey is not null)
            {
                _hintPill.Visible = true;
                _hintLabel.TextArguments = null;
                _hintLabel.TextKey = effectKey;
                _isShowingCardEffect = true;
            }
            else if (_isShowingCardEffect)
            {
                _isShowingCardEffect = false;
                RefreshHint(matchView);
            }
        }

        private void RefreshHint(MatchView matchView)
        {
            string? cardHintKey = _cardHand.FindHintKey();
            _hintPill.Visible = (_actionMode != TurnActionMode.None) || (cardHintKey is not null);
            _hintLabel.TextKey = cardHintKey ?? FindHintKey();
            _hintLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(matchView.ActionCosts.LevelClimb) };
        }

        private string FindHintKey()
        {
            string hintKey = _actionMode switch
            {
                TurnActionMode.NewCaterpillar => TextKeys.Match.ChoosePlacementHint,
                TurnActionMode.Move when _moveOrigin is null => TextKeys.Match.ChooseCaterpillarHint,
                TurnActionMode.Move => TextKeys.Match.ChooseDestinationHint,
                TurnActionMode.Build => TextKeys.Match.ChooseBuildHint,
                TurnActionMode.None => TextKeys.Match.ChoosePlacementHint,
                _ => throw new ArgumentOutOfRangeException(nameof(_actionMode)),
            };

            return hintKey;
        }

        private IReadOnlyList<BoardCell> FindTargetCells(MatchView matchView)
        {
            IReadOnlyList<BoardCell> targetCells = _cardHand.IsChoosingCells ? _cardHand.FindTargetCells(matchView) : FindActionTargetCells(matchView);

            return targetCells;
        }

        private IReadOnlyList<BoardCell> FindActionTargetCells(MatchView matchView)
        {
            IReadOnlyList<BoardCell> targetCells = _actionMode switch
            {
                TurnActionMode.NewCaterpillar => MatchCellConversion.ToBoardCells(matchView.PlacementTargets),
                TurnActionMode.Move when _moveOrigin is BoardCell originCell => FindMoveDestinations(matchView, originCell),
                TurnActionMode.Move => FindMoveOrigins(matchView),
                TurnActionMode.Build => MatchCellConversion.ToBoardCells(matchView.BuildTargets),
                TurnActionMode.None => Array.Empty<BoardCell>(),
                _ => throw new ArgumentOutOfRangeException(nameof(_actionMode)),
            };

            return targetCells;
        }

        private void FollowBoardClick(MatchView matchView)
        {
            BoardCell? clickedCell = _boardWidget.TakeClickedCell();
            if (clickedCell is not BoardCell clickedTargetCell)
            {
                return;
            }

            if (IsShortcutToMove(matchView, clickedTargetCell))
            {
                _noticeBox.Hide();
                _actionMode = TurnActionMode.Move;
                _moveOrigin = clickedTargetCell;
                RefreshActionControls(matchView);
            }
            else if (CanChooseCell(matchView, clickedTargetCell))
            {
                _noticeBox.Hide();
                ChooseClickedCell(matchView, clickedTargetCell);
            }
        }

        private void ChooseClickedCell(MatchView matchView, BoardCell clickedTargetCell)
        {
            if (_cardHand.IsChoosingCells)
            {
                _cardHand.ChooseCell(clickedTargetCell);
            }
            else
            {
                ChooseActionCell(matchView, clickedTargetCell);
            }
        }

        private bool IsShortcutToMove(MatchView matchView, BoardCell clickedCell)
        {
            bool isShortcutToMove = (_actionMode == TurnActionMode.None)
                && !_cardHand.IsChoosingCells
                && !_matchFlow.IsActing
                && MatchFlow.IsViewerPlaying(matchView)
                && FindMoveOrigins(matchView).Contains(clickedCell);

            return isShortcutToMove;
        }

        private void FollowCardClick()
        {
            if ((_boardWidget.TakeClickedCard() is MatchCard clickedCard) && !_matchFlow.IsActing)
            {
                _cardHand.ChooseCardOnTable(clickedCard);
            }
        }

        private void FollowEscapeKey(MatchView matchView)
        {
            bool isEscapeDown = Keyboard.GetState().IsKeyDown(Keys.Escape);
            bool isEscapePressed = isEscapeDown && !_wasEscapeDown;
            _wasEscapeDown = isEscapeDown;
            if (isEscapePressed)
            {
                _noticeBox.Hide();
                _actionMode = TurnActionMode.None;
                _moveOrigin = null;
                _cardHand.CancelChoice();
                RefreshActionControls(matchView);
            }
        }

        private void FollowActionBar()
        {
            if (_actionBar.TakeChosenMode() is TurnActionMode chosenMode)
            {
                ChooseActionMode(chosenMode);
            }
        }

        private void ChooseActionCell(MatchView matchView, BoardCell clickedTargetCell)
        {
            MatchCell targetMatchCell = MatchCellConversion.ToMatchCell(clickedTargetCell);
            switch (_actionMode)
            {
                case TurnActionMode.Move:
                    ChooseMoveCell(matchView, clickedTargetCell);
                    break;

                case TurnActionMode.NewCaterpillar:
                    _matchFlow.PlaceCaterpillar(targetMatchCell);
                    ShowActionStarted(matchView);
                    break;

                case TurnActionMode.Build:
                    _matchFlow.AddFlowerLevel(targetMatchCell);
                    ShowActionStarted(matchView);
                    break;

                case TurnActionMode.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(_actionMode));
            }
        }

        private bool CanChooseCell(MatchView matchView, BoardCell clickedTargetCell)
        {
            bool canChoose = !_matchFlow.IsActing && MatchFlow.IsViewerPlaying(matchView) && FindTargetCells(matchView).Contains(clickedTargetCell);

            return canChoose;
        }

        private void ChooseMoveCell(MatchView matchView, BoardCell clickedTargetCell)
        {
            if (_moveOrigin is BoardCell originCell)
            {
                var move = new MatchMove
                {
                    Origin = MatchCellConversion.ToMatchCell(originCell),
                    Destination = MatchCellConversion.ToMatchCell(clickedTargetCell),
                };
                _matchFlow.MoveCaterpillar(move);
                ShowActionStarted(matchView);
            }
            else
            {
                _moveOrigin = clickedTargetCell;
                RefreshActionControls(matchView);
            }
        }

        private void FollowCardHand(MatchView matchView)
        {
            if (_cardHand.TakeChoiceChange())
            {
                _noticeBox.Hide();
                _actionMode = TurnActionMode.None;
                _moveOrigin = null;
                RefreshActionControls(matchView);
            }

            bool isDrawRequested = _cardHand.TakeDrawRequest();
            MatchCardPlay? readyPlay = _cardHand.TakeReadyPlay();
            bool canAct = !_matchFlow.IsActing && MatchFlow.IsViewerPlaying(matchView);
            if (canAct && isDrawRequested)
            {
                _matchFlow.DrawCard();
                ShowActionStarted(matchView);
            }
            else if (canAct && (readyPlay is not null))
            {
                _matchFlow.PlayCard(readyPlay);
                ShowActionStarted(matchView);
            }
        }

        private void ShowActionStarted(MatchView matchView)
        {
            _moveOrigin = null;
            RefreshActionControls(matchView);
        }

        private void FollowActionOutcome()
        {
            MatchActionOutcome? outcome = _matchFlow.TakeOutcome();
            if (outcome?.Notice is Notice notice)
            {
                DisplayNotice(_noticeBox, notice);
            }

            if ((outcome is not null) && (_shownMatchView is not null))
            {
                RefreshActionControls(_shownMatchView);
            }
        }

        private void ChooseActionMode(TurnActionMode chosenMode)
        {
            _noticeBox.Hide();
            _moveOrigin = null;
            _cardHand.CancelChoice();
            _actionMode = _actionMode == chosenMode ? TurnActionMode.None : chosenMode;
            if (_shownMatchView is not null)
            {
                RefreshActionControls(_shownMatchView);
            }
        }

        private void EndTurnButtonOnClick(object sender, MyraEventArgs e)
        {
            bool canEndTurn = !_matchFlow.IsActing && (_shownMatchView is not null) && MatchFlow.IsViewerPlaying(_shownMatchView);
            if (canEndTurn && (_shownMatchView is MatchView matchView))
            {
                _noticeBox.Hide();
                _actionMode = TurnActionMode.None;
                _cardHand.CancelChoice();
                _matchFlow.EndTurn();
                ShowActionStarted(matchView);
            }
        }
    }
}
