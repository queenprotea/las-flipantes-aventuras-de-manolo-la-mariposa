using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Scene;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MatchScreen : Screen, IDisposable
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly MatchBoardLayoutFactory _layoutFactory = new MatchBoardLayoutFactory();
        private readonly BoardWidget _boardWidget;
        private readonly FigureIcons _figureIcons;
        private readonly VolumeTextures _volumeTextures;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly CardHandView _cardHand;
        private readonly LocalizedLabel _hintLabel = new LocalizedLabel(TextKeys.Match.ChoosePlacementHint) { Font = Fonts.Small, TextColor = Theme.Surface };
        private readonly Panel _hintPill;
        private readonly MatchPlayersBarView _playersBar;
        private readonly MatchClockView _clockView;
        private readonly LocalizedLabel _reserveCaterpillarsLabel = new LocalizedLabel(TextKeys.Match.YourCaterpillarsLabel) { Font = Fonts.Small, TextColor = Theme.Ink };
        private readonly Label _reserveCaterpillarsCount = new Label { Font = Fonts.Small, TextColor = Theme.Ink };
        private readonly Label _reserveBuildsCount = new Label { Font = Fonts.Small, TextColor = Theme.Ink };
        private readonly Label _pointsLabel = new Label { Font = Fonts.Title, TextColor = Theme.Ink, VerticalAlignment = VerticalAlignment.Center };
        private readonly Panel _reserveTag;
        private readonly Panel _pointsTag;
        private readonly Panel _deckTag;
        private readonly ActionBarView _actionBar;
        private readonly Button _endTurnButton;
        private readonly Button _chatTabButton;
        private readonly Panel _chatOverlay;
        private readonly ForfeitView _forfeitView;
        private readonly ChatView _chatView;

        private MatchView? _shownMatchView;
        private TurnActionMode _actionMode;
        private BoardCell? _moveOrigin;
        private Task<MatchActionResult>? _actionRequest;
        private TimeSpan _shownStepTime;
        private bool _wasEscapeDown;
        private bool _isShowingCardEffect;

        internal MatchScreen(GraphicsDevice graphicsDevice, GameSessionConnection gameSessionConnection, PlayerSession playerSession)
            : base(TextKeys.Match.HeaderLabel, true)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _forfeitView = new ForfeitView(gameSessionConnection, playerSession, TextKeys.Common.ExitButton);
            _chatView = new ChatView(gameSessionConnection, playerSession, MatchLayout.SideColumnWidth);
            _boardWidget = new BoardWidget(graphicsDevice, BoardLayout.Empty);
            _figureIcons = new FigureIcons(graphicsDevice);
            _volumeTextures = new VolumeTextures(graphicsDevice);
            _cardHand = new CardHandView(_volumeTextures);
            _playersBar = new MatchPlayersBarView(_figureIcons, _volumeTextures);
            _clockView = new MatchClockView(_volumeTextures);
            _reserveTag = BuildReserveTag();
            _pointsTag = BuildPointsTag();
            _deckTag = CreateTableTag(_cardHand.DeckRoot);
            _actionBar = new ActionBarView(_figureIcons, _volumeTextures);
            _hintPill = new Panel
            {
                Padding = MatchLayout.HintPillPadding,
                Background = _volumeTextures.HintPillBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = MatchLayout.HintMargin,
                Visible = false,
            };
            _hintPill.Widgets.Add(_hintLabel);
            _noticeBox.HorizontalAlignment = HorizontalAlignment.Center;
            _noticeBox.VerticalAlignment = VerticalAlignment.Top;
            _noticeBox.Width = MatchLayout.NoticeWidth;
            _noticeBox.Margin = MatchLayout.NoticeMargin;
            _endTurnButton = new Button
            {
                Content = new LocalizedLabel(TextKeys.Match.EndTurnButton) { Font = Fonts.Body, TextColor = Theme.Surface, HorizontalAlignment = HorizontalAlignment.Center },
                Padding = MatchLayout.EndTurnPillPadding,
                Background = _volumeTextures.ChosenPillBrush,
                OverBackground = _volumeTextures.ChosenPillBrush,
                PressedBackground = _volumeTextures.ChosenPillBrush,
                DisabledBackground = _volumeTextures.ChosenPillBrush,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            _endTurnButton.Click += EndTurnButtonOnClick;
            _chatTabButton = new Button
            {
                Content = new Label { Text = DisplaySymbols.ChatTab, Font = Fonts.Body, TextColor = Theme.ChosenAction, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
                Width = MatchLayout.ChatTabWidth,
                Height = MatchLayout.ChatTabHeight,
                Background = _volumeTextures.PillBrush,
                OverBackground = _volumeTextures.HoveredPillBrush,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            _chatOverlay = new Panel
            {
                Width = MatchLayout.ChatOverlayWidth + MatchLayout.ChatOverlayMargin.Right,
                Padding = MatchLayout.TableLabelPadding,
                Background = _volumeTextures.CardBrush,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = MatchLayout.ChatOverlayMargin,
                Visible = false,
            };
            _chatOverlay.Widgets.Add(_chatView.Root);
            _chatTabButton.Click += (_, _) => _chatOverlay.Visible = !_chatOverlay.Visible;
        }

        internal override bool KeepsEscapeKey => true;

        internal override void Open()
        {
            _noticeBox.Hide();
            _forfeitView.Reset();
            _chatView.Open();
            _shownMatchView = null;
            _actionMode = TurnActionMode.None;
            _moveOrigin = null;
            _cardHand.CancelChoice();
            _boardWidget.RefreshCardFaces();
            _wasEscapeDown = Keyboard.GetState().IsKeyDown(Keys.Escape);
            string roomCode = _playerSession.CurrentRoom?.Code ?? string.Empty;
            HeaderArguments = new string[] { roomCode };
            if (_playerSession.CurrentMatchView is null)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
        }

        internal override void Update(GameTime gameTime)
        {
            _chatView.Update();
            MatchView? matchView = _playerSession.CurrentMatchView;
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
            FollowActionRequest();
            ScreenId? nextScreen = _forfeitView.TakeNextScreen() ?? FindNextScreen(matchView);
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
            _figureIcons.Dispose();
            _volumeTextures.Dispose();
        }

        protected override Widget Build()
        {
            var topBar = new HorizontalStackPanel
            {
                Spacing = MatchLayout.TopBarSpacing,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = MatchLayout.TableEdgeMargin,
            };
            topBar.Widgets.Add(_playersBar.Root);
            topBar.Widgets.Add(_clockView.Root);


            var table = new Panel
            {
                Background = Theme.MatchBackgroundBrush,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            table.Widgets.Add(_boardWidget);
            table.Widgets.Add(_reserveTag);
            table.Widgets.Add(_pointsTag);
            table.Widgets.Add(_deckTag);
            table.Widgets.Add(topBar);
            table.Widgets.Add(_hintPill);
            table.Widgets.Add(_noticeBox);
            table.Widgets.Add(BuildRightColumn());
            table.Widgets.Add(_chatOverlay);

            return table;
        }

        private static bool IsViewerPlaying(MatchView matchView)
        {
            bool isViewerPlaying = (matchView.Phase == MatchPhase.PlayerTurn) && (matchView.ActiveSeatNumber == matchView.ViewerSeatNumber);

            return isViewerPlaying;
        }

        private static ScreenId? FindNextScreen(MatchView matchView)
        {
            ScreenId? nextScreen = matchView.Phase switch
            {
                MatchPhase.ButterflyPlacement => ScreenId.RoundSummary,
                MatchPhase.Finished => ScreenId.Result,
                MatchPhase.CaterpillarPlacement or MatchPhase.PlayerTurn => null,
                _ => throw new ArgumentOutOfRangeException(nameof(matchView)),
            };

            return nextScreen;
        }

        private static string ToRejectionKey(MatchActionResult actionRejection)
        {
            string rejectionKey = actionRejection switch
            {
                MatchActionResult.NotYourTurn or MatchActionResult.WrongPhase => TextKeys.Match.NotYourTurnNotice,
                MatchActionResult.TimeExpired => TextKeys.Match.TimeExpiredNotice,
                MatchActionResult.NotEnoughActionPoints => TextKeys.Match.NotEnoughPointsNotice,
                MatchActionResult.NoCaterpillarsLeft => TextKeys.Match.NoCaterpillarsLeftNotice,
                MatchActionResult.NotAFlower or MatchActionResult.OutsideBoard => TextKeys.Match.NotAFlowerNotice,
                MatchActionResult.CellOccupied => TextKeys.Match.CellOccupiedNotice,
                MatchActionResult.NotYourCaterpillar => TextKeys.Match.NotYourCaterpillarNotice,
                MatchActionResult.NotNeighborCell => TextKeys.Match.NotNeighborNotice,
                MatchActionResult.ClimbTooHigh => TextKeys.Match.ClimbTooHighNotice,
                MatchActionResult.FlowerAtMaximumLevel => TextKeys.Match.FlowerAtMaximumNotice,
                MatchActionResult.NoBuildsLeft => TextKeys.Match.NoBuildsLeftNotice,
                MatchActionResult.NotNextToPlanter => TextKeys.Match.NotNextToPlanterNotice,
                MatchActionResult.JoinsPlanters => TextKeys.Match.JoinsPlantersNotice,
                MatchActionResult.DeckEmpty => TextKeys.Match.DeckEmptyNotice,
                MatchActionResult.CardNotInHand => TextKeys.Match.CardNotInHandNotice,
                MatchActionResult.PointsCardAlreadyUsed => TextKeys.Match.PointsCardUsedNotice,
                MatchActionResult.WrongCardTarget => TextKeys.Match.WrongCardTargetNotice,
                MatchActionResult.NotInMatch or MatchActionResult.NotIdentified => TextKeys.Common.ServerErrorTitle,
                MatchActionResult.Applied => throw new ArgumentOutOfRangeException(nameof(actionRejection)),
                _ => throw new ArgumentOutOfRangeException(nameof(actionRejection)),
            };

            return rejectionKey;
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
            _forfeitView.Root.HorizontalAlignment = HorizontalAlignment.Right;
            var lowerSpace = new Panel();
            var rightColumn = new VerticalStackPanel
            {
                Spacing = MatchLayout.RightColumnSpacing,
                Width = MatchLayout.ActionColumnWidth,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = MatchLayout.RightColumnMargin,
            };
            rightColumn.Widgets.Add(_forfeitView.Root);
            rightColumn.Widgets.Add(_actionBar.Root);
            rightColumn.Widgets.Add(_chatTabButton);
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

        private Panel BuildReserveTag()
        {
            var caterpillarsRow = new HorizontalStackPanel();
            caterpillarsRow.Widgets.Add(_reserveCaterpillarsLabel);
            caterpillarsRow.Widgets.Add(_reserveCaterpillarsCount);
            var buildsRow = new HorizontalStackPanel();
            buildsRow.Widgets.Add(new LocalizedLabel(TextKeys.Match.BuildingsLabel) { Font = Fonts.Small, TextColor = Theme.Ink });
            buildsRow.Widgets.Add(_reserveBuildsCount);
            var reserveLines = new VerticalStackPanel { Spacing = MatchLayout.TableLabelSpacing };
            reserveLines.Widgets.Add(caterpillarsRow);
            reserveLines.Widgets.Add(buildsRow);
            Panel reserveTag = CreateTableTag(reserveLines);

            return reserveTag;
        }

        private Panel BuildPointsTag()
        {
            var pointsRow = new HorizontalStackPanel { Spacing = MatchLayout.TableLabelSpacing };
            pointsRow.Widgets.Add(_pointsLabel);
            pointsRow.Widgets.Add(new LocalizedLabel(TextKeys.Match.ActionPointsLabel) { Font = Fonts.Title, TextColor = Theme.PointsInk, VerticalAlignment = VerticalAlignment.Center });
            Panel pointsTag = CreateTableTag(pointsRow);

            return pointsTag;
        }

        private void PlaceTableTags()
        {
            PlaceTableTag(_reserveTag, ReserveFigure.LabelAnchor, true, false);
            PlaceTableTag(_pointsTag, DewLeafFigure.LabelAnchor, false, false);
            PlaceTableTag(_deckTag, DeckFigure.LabelAnchor, false, true);
        }

        private void PlaceTableTag(Widget tableTag, Vector3 tagAnchor, bool isAboveAnchor, bool isOnHandLayer)
        {
            if (_boardWidget.ProjectToWidget(tagAnchor, isOnHandLayer) is Point anchorPoint)
            {
                tableTag.Left = anchorPoint.X - (tableTag.ActualBounds.Width / 2);
                tableTag.Top = isAboveAnchor ? anchorPoint.Y - tableTag.ActualBounds.Height : anchorPoint.Y;
            }
        }

        private void ShowActionPoints(MatchView matchView, int previewCost)
        {
            bool isPreviewShown = (previewCost > 0) && (previewCost <= matchView.ActionPointsLeft);
            _boardWidget.ShowActionPoints(matchView.ActionPointsLeft, isPreviewShown ? previewCost : 0);
            _pointsLabel.Text = isPreviewShown ? DisplayFormats.FormatPointsPreview(matchView.ActionPointsLeft, matchView.ActionPointsLeft - previewCost) : DisplayFormats.FormatInteger(matchView.ActionPointsLeft);
        }

        private void ShowClock()
        {
            TimeSpan stepTimeLeft = TimeSpan.Zero;
            if (_playerSession.StepEndsAtUtc is DateTime stepEndsAtUtc)
            {
                stepTimeLeft = stepEndsAtUtc - DateTime.UtcNow;
            }

            float remainingShare = _shownStepTime > TimeSpan.Zero ? (float)(stepTimeLeft / _shownStepTime) : 0f;
            _clockView.ShowClock(stepTimeLeft, remainingShare);
        }

        private void ShowMatchView(MatchView matchView)
        {
            bool isFirstView = _shownMatchView is null;
            _shownMatchView = matchView;
            _shownStepTime = matchView.StepTimeLeft ?? TimeSpan.Zero;
            _moveOrigin = null;
            if (!IsViewerPlaying(matchView))
            {
                _actionMode = TurnActionMode.None;
            }

            MatchSeat viewerSeat = matchView.Seats.First(seat => seat.SeatNumber == matchView.ViewerSeatNumber);
            _clockView.Show(matchView, IsViewerPlaying(matchView));
            _playersBar.Show(matchView);
            _boardWidget.ShowReserve(viewerSeat.CaterpillarsLeft, SeatColors.ColorFor(viewerSeat.Color), matchView.BuildsLeft);
            _reserveCaterpillarsCount.Text = DisplayFormats.FormatLabeledCount(viewerSeat.CaterpillarsLeft);
            _reserveBuildsCount.Text = DisplayFormats.FormatLabeledCount(matchView.BuildsLeft);
            BoardLayout boardLayout = _layoutFactory.Create(matchView);
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
            bool canAct = IsViewerPlaying(matchView) && (_actionRequest is null);
            _endTurnButton.Enabled = canAct;
            _endTurnButton.Opacity = canAct ? 1f : MatchLayout.DisabledPillOpacity;
            _cardHand.Show(matchView, canAct);
            _boardWidget.ShowHand(_cardHand.HandStates, matchView.DeckCount);
            IReadOnlyList<BoardCell> targetCells = FindTargetCells(matchView);
            if (targetCells.Count == 0)
            {
                _actionMode = TurnActionMode.None;
            }

            MatchSeat viewerSeat = matchView.Seats.First(seat => seat.SeatNumber == matchView.ViewerSeatNumber);
            _actionBar.Show(matchView, viewerSeat.Color, canAct, _actionMode);
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
                && (_actionRequest is null)
                && IsViewerPlaying(matchView)
                && FindMoveOrigins(matchView).Contains(clickedCell);

            return isShortcutToMove;
        }

        private void FollowCardClick()
        {
            if ((_boardWidget.TakeClickedCard() is MatchCard clickedCard) && (_actionRequest is null))
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
                    StartActionRequest(_gameSessionConnection.PlacePieceAsync(new PiecePlacement(MatchPiece.Caterpillar, targetMatchCell)), matchView);
                    break;

                case TurnActionMode.Build:
                    StartActionRequest(_gameSessionConnection.AddFlowerLevelAsync(targetMatchCell), matchView);
                    break;

                case TurnActionMode.None:
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(_actionMode));
            }
        }

        private bool CanChooseCell(MatchView matchView, BoardCell clickedTargetCell)
        {
            bool canChoose = (_actionRequest is null) && IsViewerPlaying(matchView) && FindTargetCells(matchView).Contains(clickedTargetCell);

            return canChoose;
        }

        private void ChooseMoveCell(MatchView matchView, BoardCell clickedTargetCell)
        {
            if (_moveOrigin is BoardCell originCell)
            {
                var move = new MatchMove { Origin = MatchCellConversion.ToMatchCell(originCell), Destination = MatchCellConversion.ToMatchCell(clickedTargetCell) };
                StartActionRequest(_gameSessionConnection.MoveCaterpillarAsync(move), matchView);
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
            bool canAct = (_actionRequest is null) && IsViewerPlaying(matchView);
            if (canAct && isDrawRequested)
            {
                StartActionRequest(_gameSessionConnection.DrawCardAsync(), matchView);
            }
            else if (canAct && (readyPlay is not null))
            {
                StartActionRequest(_gameSessionConnection.PlayCardAsync(readyPlay), matchView);
            }
        }

        private void StartActionRequest(Task<MatchActionResult> actionRequest, MatchView matchView)
        {
            _actionRequest = actionRequest;
            _moveOrigin = null;
            RefreshActionControls(matchView);
        }

        private void FollowActionRequest()
        {
            if ((_actionRequest is null) || !_actionRequest.IsCompleted)
            {
                return;
            }

            Task<MatchActionResult> completedAction = _actionRequest;
            _actionRequest = null;
            if (!completedAction.IsCompletedSuccessfully)
            {
                ShowServerError();
            }
            else if (completedAction.Result != MatchActionResult.Applied)
            {
                _noticeBox.ShowTitle(ToRejectionKey(completedAction.Result));
            }

            if (_shownMatchView is not null)
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
            bool canEndTurn = (_actionRequest is null) && (_shownMatchView is not null) && IsViewerPlaying(_shownMatchView);
            if (canEndTurn && (_shownMatchView is MatchView matchView))
            {
                _noticeBox.Hide();
                _actionMode = TurnActionMode.None;
                _cardHand.CancelChoice();
                StartActionRequest(_gameSessionConnection.EndTurnAsync(), matchView);
            }
        }

        private void ShowServerError()
        {
            string[]? noDetailArguments = null;
            _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, noDetailArguments);
        }
    }
}
