using System;
using System.Collections.Generic;
using System.Globalization;
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
    internal sealed class InitialPlacementScreen : Screen, IDisposable
    {
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly MatchBoardLayoutFactory _layoutFactory = new MatchBoardLayoutFactory();
        private readonly BoardWidget _boardWidget;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly VerticalStackPanel _playersPanel = new VerticalStackPanel();
        private readonly VerticalStackPanel _turnOrderPanel = new VerticalStackPanel();
        private readonly Label _clockLabel = new Label { Font = Fonts.Clock, TextColor = Theme.MintInk, HorizontalAlignment = HorizontalAlignment.Center };
        private readonly LocalizedLabel _instructionLabel = new LocalizedLabel(TextKeys.InitialPlacement.PlaceCaterpillarInstruction) { Font = Fonts.Title, TextColor = Theme.Ink, HorizontalAlignment = HorizontalAlignment.Center };
        private readonly LocalizedLabel _turnLabel = new LocalizedLabel(TextKeys.InitialPlacement.CurrentTurnInstruction) { Font = Fonts.Small, TextColor = Theme.MutedInk, HorizontalAlignment = HorizontalAlignment.Center };

        private MatchView? _shownMatchView;
        private Task<MatchActionResult>? _placementRequest;
        private Task<EndMatchResult>? _endMatchRequest;

        internal InitialPlacementScreen(GraphicsDevice graphicsDevice, GameSessionConnection gameSessionConnection, PlayerSession playerSession)
            : base(TextKeys.InitialPlacement.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);

            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _boardWidget = new BoardWidget(graphicsDevice, BoardLayout.Empty);
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            _shownMatchView = null;
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

            _clockLabel.Text = FormatPlacementTimeLeft();
            _boardWidget.Advance(gameTime.ElapsedGameTime);
            _boardWidget.UpdateInput();
            FollowBoardClick(matchView);
            FollowPlacementRequest();
            FollowEndMatchRequest();
            if (matchView.Phase == MatchPhase.PlayerTurn)
            {
                RequestedScreen = ScreenId.Match;
            }
            else if (matchView.Phase == MatchPhase.Finished)
            {
                RequestedScreen = ScreenId.Result;
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
            VerticalStackPanel centerPanel = BuildCenterPanel();
            HorizontalStackPanel screenColumns = CreateColumns();
            screenColumns.Widgets.Add(BuildSideBar());
            screenColumns.Widgets.Add(centerPanel);
            screenColumns.Widgets.Add(BuildSideColumn());
            StackPanel.SetProportionType(centerPanel, ProportionType.Fill);

            return screenColumns;
        }

        private static bool IsViewerPlacing(MatchView matchView)
        {
            bool isViewerPlacing = (matchView.Phase != MatchPhase.PlayerTurn) && (matchView.ActiveSeatNumber == matchView.ViewerSeatNumber);

            return isViewerPlacing;
        }

        private static string GetPlacingPlayerName(MatchView matchView)
        {
            string placingName = matchView.Seats.First(seat => seat.SeatNumber == matchView.ActiveSeatNumber).DisplayName;

            return placingName;
        }

        private static string ToPlacementRejectionKey(MatchActionResult placementRejection)
        {
            string rejectionKey = placementRejection switch
            {
                MatchActionResult.CellOccupied => TextKeys.InitialPlacement.FlowerOccupiedNotice,
                MatchActionResult.NotAFlower or MatchActionResult.OutsideBoard => TextKeys.InitialPlacement.NotAFlowerNotice,
                MatchActionResult.TimeExpired => TextKeys.InitialPlacement.TimeExpiredNotice,
                MatchActionResult.NotInMatch or MatchActionResult.NotIdentified => TextKeys.Common.ServerErrorTitle,
                MatchActionResult.NotYourTurn or MatchActionResult.WrongPhase => TextKeys.InitialPlacement.NotYourPlacementNotice,
                MatchActionResult.Applied or MatchActionResult.NotEnoughActionPoints or MatchActionResult.NoCaterpillarsLeft or MatchActionResult.NotYourCaterpillar or MatchActionResult.NotNeighborCell or MatchActionResult.ClimbTooHigh or MatchActionResult.FlowerAtMaximumLevel or MatchActionResult.NoBuildsLeft or MatchActionResult.NotNextToPlanter or MatchActionResult.JoinsPlanters => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
                _ => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
            };

            return rejectionKey;
        }

        private static HorizontalStackPanel BuildPlayerRow(MatchSeat seat, bool isViewer, bool isPlacing)
        {
            var playerIdentity = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
            };
            Color nameColor = isPlacing ? Theme.MintInk : Theme.Ink;
            playerIdentity.Widgets.Add(CreatePlayerNameLabel(seat.DisplayName, nameColor));

            if (isViewer)
            {
                playerIdentity.Widgets.Add(new LocalizedLabel(TextKeys.Common.YouTag)
                {
                    Font = Fonts.Label,
                    TextColor = Theme.MutedInk,
                });
            }

            HorizontalStackPanel playerRow = BuildColorRow(SeatColors.BrushFor(seat.Color), MatchLayout.PlayerIconSize);
            playerRow.Widgets.Add(playerIdentity);
            StackPanel.SetProportionType(playerIdentity, ProportionType.Fill);
            if (isPlacing)
            {
                playerRow.Background = Theme.MintTintBrush;
            }

            return playerRow;
        }

        private static HorizontalStackPanel BuildTurnOrderRow(MatchSeat seat, bool hasPlaced)
        {
            Label nameLabel = CreatePlayerNameLabel(seat.DisplayName, Theme.Ink);
            HorizontalStackPanel turnOrderRow = BuildColorRow(SeatColors.BrushFor(seat.Color), MatchLayout.TurnOrderIconSize);
            turnOrderRow.Widgets.Add(nameLabel);
            turnOrderRow.Widgets.Add(new Label
            {
                Text = hasPlaced ? DisplaySymbols.CheckMark : seat.SeatNumber.ToString(CultureInfo.CurrentCulture),
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            StackPanel.SetProportionType(nameLabel, ProportionType.Fill);

            return turnOrderRow;
        }

        private static HorizontalStackPanel BuildColorRow(IBrush colorBrush, int iconSize)
        {
            var colorRow = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
            };
            colorRow.Widgets.Add(new Panel
            {
                Width = iconSize,
                Height = iconSize,
                Background = colorBrush,
                VerticalAlignment = VerticalAlignment.Center,
            });

            return colorRow;
        }

        private static VerticalStackPanel BuildSideColumn()
        {
            VerticalStackPanel chatPanel = CreateChatPanel();
            var sideColumn = new VerticalStackPanel { Width = MatchLayout.SideColumnWidth, VerticalAlignment = VerticalAlignment.Stretch };
            sideColumn.Widgets.Add(chatPanel);
            StackPanel.SetProportionType(chatPanel, ProportionType.Fill);

            return sideColumn;
        }

        private VerticalStackPanel BuildSideBar()
        {
            var sideBar = new VerticalStackPanel { Spacing = Metrics.ColumnSpacing, Width = MatchLayout.SidebarWidth, VerticalAlignment = VerticalAlignment.Top };
            sideBar.Widgets.Add(CreateGameSection(TextKeys.InitialPlacement.PlayersLabel, _playersPanel));
            sideBar.Widgets.Add(CreateDivider());
            sideBar.Widgets.Add(CreateGameSection(TextKeys.InitialPlacement.TurnOrderLabel, _turnOrderPanel));
            sideBar.Widgets.Add(CreateDivider());
            sideBar.Widgets.Add(BuildEndMatchButton());

            return sideBar;
        }

        private LocalizedButton BuildEndMatchButton()
        {
            LocalizedButton endMatchButton = CreateDestructiveButton(TextKeys.Common.EndMatchButton);
            endMatchButton.LabelFont = Fonts.Small;
            endMatchButton.Padding = Metrics.SmallButtonPadding;
            endMatchButton.HorizontalAlignment = HorizontalAlignment.Stretch;
            endMatchButton.Click += EndMatchButtonOnClick;

            return endMatchButton;
        }

        private VerticalStackPanel BuildCenterPanel()
        {
            var centerPanel = new VerticalStackPanel
            {
                Spacing = MatchLayout.CenterSpacing,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            centerPanel.Widgets.Add(_noticeBox);
            centerPanel.Widgets.Add(_clockLabel);
            centerPanel.Widgets.Add(_instructionLabel);
            centerPanel.Widgets.Add(_turnLabel);
            centerPanel.Widgets.Add(_boardWidget);
            StackPanel.SetProportionType(_boardWidget, ProportionType.Fill);

            return centerPanel;
        }

        private string FormatPlacementTimeLeft()
        {
            TimeSpan placementTimeLeft = TimeSpan.Zero;
            if (_playerSession.StepEndsAtUtc is DateTime placementEndsAtUtc)
            {
                placementTimeLeft = placementEndsAtUtc - DateTime.UtcNow;
            }

            string clockText = DisplayFormats.FormatCountdown(placementTimeLeft);

            return clockText;
        }

        private void ShowMatchView(MatchView matchView)
        {
            bool isFirstView = _shownMatchView is null;
            _shownMatchView = matchView;
            bool isViewerPlacing = IsViewerPlacing(matchView);
            bool isButterflyPlacement = matchView.Phase == MatchPhase.ButterflyPlacement;
            _instructionLabel.Visible = isViewerPlacing;
            _instructionLabel.TextKey = isButterflyPlacement ? TextKeys.InitialPlacement.PlaceButterflyInstruction : TextKeys.InitialPlacement.PlaceCaterpillarInstruction;
            _turnLabel.TextKey = isButterflyPlacement ? TextKeys.InitialPlacement.ButterflyTurnInstruction : TextKeys.InitialPlacement.CurrentTurnInstruction;
            _turnLabel.TextArguments = new string[] { GetPlacingPlayerName(matchView) };

            BoardLayout boardLayout = _layoutFactory.Create(matchView);
            if (isFirstView)
            {
                _boardWidget.ShowLayoutWithoutMotion(boardLayout);
            }
            else
            {
                _boardWidget.ShowLayout(boardLayout);
            }
            IReadOnlyList<BoardCell> targetCells = Array.Empty<BoardCell>();
            if (isViewerPlacing)
            {
                targetCells = MatchCellConversion.ToBoardCells(matchView.PlacementTargets);
            }

            BoardCell? butterflyCell = null;
            if (matchView.HasButterfly)
            {
                butterflyCell = MatchCellConversion.ToBoardCell(matchView.ButterflyCell);
            }

            _boardWidget.ShowPlacementTargets(targetCells);
            _boardWidget.ShowButterflyCell(butterflyCell);
            ShowSeats(matchView);
        }

        private void ShowSeats(MatchView matchView)
        {
            _playersPanel.Widgets.Clear();
            _turnOrderPanel.Widgets.Clear();
            foreach (MatchSeat seat in matchView.Seats)
            {
                bool isViewer = seat.SeatNumber == matchView.ViewerSeatNumber;
                bool isPlacing = seat.SeatNumber == matchView.ActiveSeatNumber;
                bool hasPlaced = matchView.Caterpillars.Exists(caterpillar => caterpillar.SeatNumber == seat.SeatNumber);
                _playersPanel.Widgets.Add(BuildPlayerRow(seat, isViewer, isPlacing));
                _turnOrderPanel.Widgets.Add(BuildTurnOrderRow(seat, hasPlaced));
            }
        }

        private void FollowBoardClick(MatchView matchView)
        {
            BoardCell? clickedCell = _boardWidget.TakeClickedCell();
            bool canPlace = (_placementRequest is null) && IsViewerPlacing(matchView);
            if (!canPlace || (clickedCell is not BoardCell placementCell))
            {
                return;
            }

            _noticeBox.Hide();
            MatchCell destinationCell = MatchCellConversion.ToMatchCell(placementCell);
            MatchPiece placedPiece = matchView.Phase == MatchPhase.ButterflyPlacement ? MatchPiece.Butterfly : MatchPiece.Caterpillar;
            _placementRequest = _gameSessionConnection.PlacePieceAsync(new PiecePlacement(placedPiece, destinationCell));
        }

        private void FollowPlacementRequest()
        {
            if ((_placementRequest is null) || !_placementRequest.IsCompleted)
            {
                return;
            }

            Task<MatchActionResult> completedPlacement = _placementRequest;
            _placementRequest = null;
            if (!completedPlacement.IsCompletedSuccessfully)
            {
                ShowServerError();
            }
            else if (completedPlacement.Result != MatchActionResult.Applied)
            {
                _noticeBox.ShowTitle(ToPlacementRejectionKey(completedPlacement.Result));
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
    }
}
