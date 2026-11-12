using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Scene;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class MatchScreen : Screen, IDisposable
    {
        private const float SpentPipOpacity = 0.28f;

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly MatchBoardLayoutFactory _layoutFactory = new MatchBoardLayoutFactory();
        private readonly BoardWidget _boardWidget;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly VerticalStackPanel _playersPanel = new VerticalStackPanel();
        private readonly HorizontalStackPanel _caterpillarPips = new HorizontalStackPanel { Spacing = MatchLayout.PipSpacing };
        private readonly HorizontalStackPanel _buildPips = new HorizontalStackPanel { Spacing = MatchLayout.PipSpacing };
        private readonly Label _clockLabel = new Label { Font = Fonts.Clock, TextColor = Theme.MintInk, HorizontalAlignment = HorizontalAlignment.Center };
        private readonly Label _actionPointsLabel = new Label { Font = Fonts.ActionPoints, TextColor = Theme.Ink };
        private readonly Label _caterpillarCostLabel = CreateCostLabel();
        private readonly Label _moveCostLabel = CreateCostLabel();
        private readonly Label _buildCostLabel = CreateCostLabel();
        private readonly LocalizedLabel _roundTurnLabel = new LocalizedLabel(TextKeys.Match.RoundTurnInstruction) { Font = Fonts.Small, TextColor = Theme.MutedInk };
        private readonly LocalizedLabel _turnStatusLabel = new LocalizedLabel(TextKeys.Match.YourTurnStatus) { Font = Fonts.Small, TextColor = Theme.Ink };
        private readonly LocalizedLabel _hintLabel = new LocalizedLabel(TextKeys.Match.ChoosePlacementHint) { Font = Fonts.Small, TextColor = Theme.SoftInk, HorizontalAlignment = HorizontalAlignment.Center, Visible = false };
        private readonly Button _caterpillarButton;
        private readonly Button _moveButton;
        private readonly Button _buildButton;
        private readonly LocalizedButton _endTurnButton;

        private MatchView? _shownMatchView;
        private TurnActionMode _actionMode;
        private BoardCell? _moveOrigin;
        private Task<MatchActionResult>? _actionRequest;
        private Task<EndMatchResult>? _endMatchRequest;

        internal MatchScreen(GraphicsDevice graphicsDevice, GameSessionConnection gameSessionConnection, PlayerSession playerSession)
            : base(TextKeys.Match.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);

            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _boardWidget = new BoardWidget(graphicsDevice, BoardLayout.Empty);
            _caterpillarButton = CreateActionButton(TextKeys.Match.CaterpillarActionButton, _caterpillarCostLabel);
            _caterpillarButton.Click += CaterpillarButtonOnClick;
            _moveButton = CreateActionButton(TextKeys.Match.MoveActionButton, _moveCostLabel);
            _moveButton.Click += MoveButtonOnClick;
            _buildButton = CreateActionButton(TextKeys.Match.BuildActionButton, _buildCostLabel);
            _buildButton.Click += BuildButtonOnClick;
            _endTurnButton = CreatePrimaryButton(TextKeys.Match.EndTurnButton);
            _endTurnButton.Padding = MatchLayout.EndTurnPadding;
            _endTurnButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            _endTurnButton.LabelAlignment = HorizontalAlignment.Center;
            _endTurnButton.Click += EndTurnButtonOnClick;
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            _shownMatchView = null;
            _actionMode = TurnActionMode.None;
            _moveOrigin = null;
            string roomCode = _playerSession.CurrentRoom?.Code ?? string.Empty;
            HeaderArguments = new string[] { roomCode };
            if (_playerSession.CurrentMatchView is null)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
        }

        internal override void Update(GameTime gameTime)
        {
            MatchView? matchView = _playerSession.CurrentMatchView;
            if (matchView is null)
            {
                return;
            }

            if (!ReferenceEquals(matchView, _shownMatchView))
            {
                ShowMatchView(matchView);
            }

            _clockLabel.Text = FormatStepTimeLeft();
            _boardWidget.Advance(gameTime.ElapsedGameTime);
            _boardWidget.UpdateInput();
            FollowBoardClick(matchView);
            FollowActionRequest();
            FollowEndMatchRequest();
            ScreenId? nextScreen = FindNextScreen(matchView);
            if (nextScreen is not null)
            {
                RequestedScreen = nextScreen;
            }
        }

        internal override void RenderScene()
        {
            _boardWidget.RenderScene();
        }

        public void Dispose()
        {
            _boardWidget.Dispose();
        }

        protected override Widget Build()
        {
            HorizontalStackPanel screenColumns = CreateColumns();
            VerticalStackPanel centerPanel = BuildCenterPanel();
            screenColumns.Widgets.Add(BuildSideBar());
            screenColumns.Widgets.Add(centerPanel);
            screenColumns.Widgets.Add(BuildSideColumn());
            StackPanel.SetProportionType(centerPanel, ProportionType.Fill);

            VerticalStackPanel page = CreatePage();
            page.Spacing = MatchLayout.HudSpacing;
            page.Widgets.Add(screenColumns);
            page.Widgets.Add(CreateDivider());
            page.Widgets.Add(BuildActionPoints());
            StackPanel.SetProportionType(screenColumns, ProportionType.Fill);

            return page;
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
                MatchActionResult.NotInMatch or MatchActionResult.NotIdentified => TextKeys.Common.ServerErrorTitle,
                MatchActionResult.Applied => throw new ArgumentOutOfRangeException(nameof(actionRejection)),
                _ => throw new ArgumentOutOfRangeException(nameof(actionRejection)),
            };

            return rejectionKey;
        }

        private static Label CreateCostLabel()
        {
            var costLabel = new Label
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return costLabel;
        }

        private static Button CreateActionButton(string textKey, Label costLabel)
        {
            var buttonContent = new HorizontalStackPanel
            {
                Spacing = Metrics.FieldLabelSpacing,
            };
            buttonContent.Widgets.Add(new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
            });
            buttonContent.Widgets.Add(costLabel);

            var actionButton = new Button
            {
                Content = buttonContent,
                Padding = Metrics.SmallButtonPadding,
                Background = Theme.SurfaceBrush,
                OverBackground = Theme.SurfaceSunkenBrush,
                PressedBackground = Theme.SurfaceSunkenBrush,
                Border = Theme.StrongLineBrush,
                BorderThickness = Sizes.Border,
            };

            return actionButton;
        }

        private static HorizontalStackPanel BuildPlayerRow(MatchSeat seat, bool isViewer, bool isPlaying)
        {
            var playerIdentity = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            Color nameColor = isPlaying ? Theme.MintInk : Theme.Ink;
            playerIdentity.Widgets.Add(CreatePlayerNameLabel(seat.DisplayName, nameColor));
            if (isViewer)
            {
                playerIdentity.Widgets.Add(new LocalizedLabel(TextKeys.Common.YouTag) { Font = Fonts.Label, TextColor = Theme.MutedInk });
            }

            if (seat.IsRetired)
            {
                playerIdentity.Widgets.Add(new LocalizedLabel(TextKeys.Common.WithdrewTag) { Font = Fonts.Label, TextColor = Theme.BlushInk });
            }

            var playerRow = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
            };
            playerRow.Widgets.Add(new Panel
            {
                Width = MatchLayout.PlayerIconSize,
                Height = MatchLayout.PlayerIconSize,
                Background = SeatColors.BrushFor(seat.Color),
                VerticalAlignment = VerticalAlignment.Center,
            });
            playerRow.Widgets.Add(playerIdentity);
            playerRow.Widgets.Add(new Label
            {
                Text = DisplayFormats.FormatInteger(seat.TotalScore),
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            StackPanel.SetProportionType(playerIdentity, ProportionType.Fill);
            if (isPlaying)
            {
                playerRow.Background = Theme.MintTintBrush;
            }

            return playerRow;
        }

        private static void ShowPips(HorizontalStackPanel pipRow, PipRowContent pipRowContent, IBrush pipBrush)
        {
            pipRow.Widgets.Clear();
            for (var i = 0; i < pipRowContent.Total; i++)
            {
                pipRow.Widgets.Add(new Panel
                {
                    Width = pipRowContent.PipSize,
                    Height = pipRowContent.PipSize,
                    Background = pipBrush,
                    Border = Theme.StrongLineBrush,
                    BorderThickness = Sizes.Border,
                    Opacity = i < pipRowContent.Available ? 1f : SpentPipOpacity,
                });
            }
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

        private static VerticalStackPanel CreateSideColumnPanel(LocalizedButton endTurnButton, LocalizedButton endMatchButton)
        {
            VerticalStackPanel chatPanel = CreateChatPanel();
            var sideColumn = new VerticalStackPanel
            {
                Spacing = Metrics.ColumnSpacing,
                Width = MatchLayout.SideColumnWidth,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            sideColumn.Widgets.Add(chatPanel);
            sideColumn.Widgets.Add(endTurnButton);
            sideColumn.Widgets.Add(CreateGameSection(TextKeys.Match.MatchLabel, endMatchButton));
            StackPanel.SetProportionType(chatPanel, ProportionType.Fill);

            return sideColumn;
        }

        private VerticalStackPanel BuildSideBar()
        {
            var sideBar = new VerticalStackPanel
            {
                Spacing = Metrics.ColumnSpacing,
                Width = MatchLayout.SidebarWidth,
                VerticalAlignment = VerticalAlignment.Top,
            };
            sideBar.Widgets.Add(CreateGameSection(TextKeys.Match.PlayersLabel, _playersPanel));
            sideBar.Widgets.Add(CreateDivider());
            sideBar.Widgets.Add(CreateGameSection(TextKeys.Match.YourCaterpillarsLabel, _caterpillarPips));
            sideBar.Widgets.Add(CreateGameSection(TextKeys.Match.BuildingsLabel, _buildPips));

            return sideBar;
        }

        private VerticalStackPanel BuildCenterPanel()
        {
            var turnStatus = new HorizontalStackPanel
            {
                Spacing = Metrics.FieldLabelSpacing,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            turnStatus.Widgets.Add(_roundTurnLabel);
            turnStatus.Widgets.Add(_turnStatusLabel);

            var actionRow = new HorizontalStackPanel
            {
                Spacing = MatchLayout.ActionSpacing,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            actionRow.Widgets.Add(_caterpillarButton);
            actionRow.Widgets.Add(_moveButton);
            actionRow.Widgets.Add(_buildButton);

            var centerPanel = new VerticalStackPanel
            {
                Spacing = MatchLayout.CenterSpacing,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            centerPanel.Widgets.Add(_noticeBox);
            centerPanel.Widgets.Add(_clockLabel);
            centerPanel.Widgets.Add(turnStatus);
            centerPanel.Widgets.Add(_boardWidget);
            centerPanel.Widgets.Add(_hintLabel);
            centerPanel.Widgets.Add(actionRow);
            StackPanel.SetProportionType(_boardWidget, ProportionType.Fill);

            return centerPanel;
        }

        private VerticalStackPanel BuildSideColumn()
        {
            LocalizedButton endMatchButton = CreateDestructiveButton(TextKeys.Common.EndMatchButton);
            endMatchButton.LabelFont = Fonts.Small;
            endMatchButton.Padding = Metrics.SmallButtonPadding;
            endMatchButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            endMatchButton.Click += EndMatchButtonOnClick;
            VerticalStackPanel sideColumn = CreateSideColumnPanel(_endTurnButton, endMatchButton);

            return sideColumn;
        }

        private HorizontalStackPanel BuildActionPoints()
        {
            var actionPoints = new HorizontalStackPanel
            {
                Spacing = MatchLayout.ActionPointsSpacing,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            actionPoints.Widgets.Add(_actionPointsLabel);
            actionPoints.Widgets.Add(new LocalizedLabel(TextKeys.Match.ActionPointsLabel)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Bottom,
            });

            return actionPoints;
        }

        private string FormatStepTimeLeft()
        {
            TimeSpan stepTimeLeft = TimeSpan.Zero;
            if (_playerSession.StepEndsAtUtc is DateTime stepEndsAtUtc)
            {
                stepTimeLeft = stepEndsAtUtc - DateTime.UtcNow;
            }

            string clockText = DisplayFormats.FormatCountdown(stepTimeLeft);

            return clockText;
        }

        private void ShowMatchView(MatchView matchView)
        {
            bool isFirstView = _shownMatchView is null;
            _shownMatchView = matchView;
            _moveOrigin = null;
            if (!IsViewerPlaying(matchView))
            {
                _actionMode = TurnActionMode.None;
            }

            MatchSeat activeSeat = matchView.Seats.First(seat => seat.SeatNumber == matchView.ActiveSeatNumber);
            MatchSeat viewerSeat = matchView.Seats.First(seat => seat.SeatNumber == matchView.ViewerSeatNumber);
            _roundTurnLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(matchView.RoundNumber), DisplayFormats.FormatInteger(matchView.TurnRoundNumber), DisplayFormats.FormatInteger(matchView.RoundTurnCount) };
            _turnStatusLabel.TextKey = IsViewerPlaying(matchView) ? TextKeys.Match.YourTurnStatus : TextKeys.Match.OtherPlayerTurnStatus;
            _turnStatusLabel.TextArguments = new string[] { activeSeat.DisplayName };
            _actionPointsLabel.Text = DisplayFormats.FormatInteger(matchView.ActionPointsLeft);
            _caterpillarCostLabel.Text = DisplayFormats.FormatActionCost(matchView.ActionCosts.CaterpillarPlacement);
            _moveCostLabel.Text = DisplayFormats.FormatActionCost(matchView.ActionCosts.Movement);
            _buildCostLabel.Text = DisplayFormats.FormatActionCost(matchView.ActionCosts.FlowerLevelAddition);

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
            _playersPanel.Widgets.Clear();
            foreach (MatchSeat seat in matchView.Seats)
            {
                _playersPanel.Widgets.Add(BuildPlayerRow(seat, seat.SeatNumber == matchView.ViewerSeatNumber, seat.SeatNumber == matchView.ActiveSeatNumber));
            }

            ShowPips(_caterpillarPips, new PipRowContent(matchView.CaterpillarsPerSeat, viewerSeat.CaterpillarsLeft, MatchLayout.CaterpillarPipSize), SeatColors.BrushFor(viewerSeat.Color));
            ShowPips(_buildPips, new PipRowContent(matchView.BuildsLeft, matchView.BuildsLeft, MatchLayout.BuildingPipSize), Theme.SurfaceAltBrush);
            RefreshActionControls(matchView);
        }

        private void RefreshActionControls(MatchView matchView)
        {
            bool canAct = IsViewerPlaying(matchView) && (_actionRequest is null);
            _caterpillarButton.Enabled = canAct && (matchView.PlacementTargets.Count > 0);
            _moveButton.Enabled = canAct && (matchView.MoveTargets.Count > 0);
            _buildButton.Enabled = canAct && (matchView.BuildTargets.Count > 0);
            _endTurnButton.Enabled = canAct;

            IReadOnlyList<BoardCell> targetCells = FindTargetCells(matchView);
            if (targetCells.Count == 0)
            {
                _actionMode = TurnActionMode.None;
            }

            _caterpillarButton.Background = _actionMode == TurnActionMode.NewCaterpillar ? Theme.MintTintBrush : Theme.SurfaceBrush;
            _moveButton.Background = _actionMode == TurnActionMode.Move ? Theme.MintTintBrush : Theme.SurfaceBrush;
            _buildButton.Background = _actionMode == TurnActionMode.Build ? Theme.MintTintBrush : Theme.SurfaceBrush;
            _hintLabel.Visible = _actionMode != TurnActionMode.None;
            _hintLabel.TextKey = FindHintKey();
            _hintLabel.TextArguments = new string[] { DisplayFormats.FormatInteger(matchView.ActionCosts.LevelClimb) };
            _boardWidget.ShowPlacementTargets(FindTargetCells(matchView));
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
            if ((clickedCell is not BoardCell clickedTargetCell) || !CanChooseCell(matchView, clickedTargetCell))
            {
                return;
            }

            _noticeBox.Hide();
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

        private void FollowEndMatchRequest()
        {
            if ((_endMatchRequest is null) || !_endMatchRequest.IsCompleted)
            {
                return;
            }

            Task<EndMatchResult> completedEndMatch = _endMatchRequest;
            _endMatchRequest = null;
            if (!completedEndMatch.IsCompletedSuccessfully || (completedEndMatch.Result == EndMatchResult.NotIdentified))
            {
                ShowServerError();
            }
        }

        private void ChooseActionMode(TurnActionMode chosenMode)
        {
            _noticeBox.Hide();
            _moveOrigin = null;
            _actionMode = _actionMode == chosenMode ? TurnActionMode.None : chosenMode;
            if (_shownMatchView is not null)
            {
                RefreshActionControls(_shownMatchView);
            }
        }

        private void CaterpillarButtonOnClick(object sender, MyraEventArgs e)
        {
            ChooseActionMode(TurnActionMode.NewCaterpillar);
        }

        private void MoveButtonOnClick(object sender, MyraEventArgs e)
        {
            ChooseActionMode(TurnActionMode.Move);
        }

        private void BuildButtonOnClick(object sender, MyraEventArgs e)
        {
            ChooseActionMode(TurnActionMode.Build);
        }

        private void EndTurnButtonOnClick(object sender, MyraEventArgs e)
        {
            bool canEndTurn = (_actionRequest is null) && (_shownMatchView is not null) && IsViewerPlaying(_shownMatchView);
            if (canEndTurn && (_shownMatchView is MatchView matchView))
            {
                _noticeBox.Hide();
                _actionMode = TurnActionMode.None;
                StartActionRequest(_gameSessionConnection.EndTurnAsync(), matchView);
            }
        }

        private void EndMatchButtonOnClick(object sender, MyraEventArgs e)
        {
            _noticeBox.Hide();
            _endMatchRequest ??= _gameSessionConnection.EndMatchAsync();
        }

        private void ShowServerError()
        {
            string[]? noDetailArguments = null;
            _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, noDetailArguments);
        }

        private readonly record struct PipRowContent(int Total, int Available, int PipSize);
    }
}
