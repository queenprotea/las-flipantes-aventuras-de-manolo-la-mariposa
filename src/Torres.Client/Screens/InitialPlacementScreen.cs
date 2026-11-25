using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
        private readonly DoomLauncher _doomLauncher;
        private readonly MatchBoardLayoutFactory _layoutFactory = new MatchBoardLayoutFactory();
        private readonly BoardWidget _boardWidget;
        private readonly FigureIcons _figureIcons;
        private readonly VolumeTextures _volumeTextures;
        private readonly MatchPlayersBarView _playersBar;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly ForfeitView _forfeitView;
        private readonly ChatView _chatView;
        private readonly VerticalStackPanel _turnOrderPanel = new VerticalStackPanel();
        private readonly Label _clockLabel = new Label { Font = Fonts.Clock, TextColor = Theme.Ink, VerticalAlignment = VerticalAlignment.Center };
        private readonly ClockRing _clockRing;
        private readonly LocalizedLabel _instructionLabel = new LocalizedLabel(TextKeys.InitialPlacement.PlaceCaterpillarInstruction) { Font = Fonts.Small, TextColor = Theme.Surface };
        private readonly LocalizedLabel _turnLabel = new LocalizedLabel(TextKeys.InitialPlacement.CurrentTurnInstruction) { Font = Fonts.Small, TextColor = Theme.MintInk, VerticalAlignment = VerticalAlignment.Center };
        private readonly Panel _instructionPill;
        private readonly Panel _chatOverlay;

        private MatchView? _shownMatchView;
        private Task<MatchActionResult>? _placementRequest;
        private TimeSpan _shownStepTime;

        internal InitialPlacementScreen(GraphicsDevice graphicsDevice, GameSessionConnection gameSessionConnection, PlayerSession playerSession, DoomLauncher doomLauncher)
            : base(TextKeys.InitialPlacement.HeaderLabel, true)
        {
            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _doomLauncher = doomLauncher;
            _forfeitView = new ForfeitView(gameSessionConnection, playerSession, TextKeys.Common.ExitButton);
            _chatView = new ChatView(gameSessionConnection, playerSession, MatchLayout.SideColumnWidth);
            _boardWidget = new BoardWidget(graphicsDevice, BoardLayout.Empty);
            _figureIcons = new FigureIcons(graphicsDevice);
            _volumeTextures = new VolumeTextures(graphicsDevice);
            _playersBar = new MatchPlayersBarView(_figureIcons, _volumeTextures);
            _clockRing = new ClockRing(_volumeTextures.DotImage, MatchLayout.ClockRingSize) { VerticalAlignment = VerticalAlignment.Center };
            _instructionPill = new Panel
            {
                Padding = MatchLayout.HintPillPadding,
                Background = _volumeTextures.HintPillBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = MatchLayout.HintMargin,
            };
            _instructionPill.Widgets.Add(_instructionLabel);
            _noticeBox.HorizontalAlignment = HorizontalAlignment.Center;
            _noticeBox.VerticalAlignment = VerticalAlignment.Top;
            _noticeBox.Width = MatchLayout.NoticeWidth;
            _noticeBox.Margin = MatchLayout.NoticeMargin;
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
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            _forfeitView.Reset();
            _chatView.Open();
            _shownMatchView = null;
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
            _boardWidget.UpdateInput();
            FollowBoardClick(matchView);
            FollowPlacementRequest();
            ScreenId? forfeitScreen = _forfeitView.TakeNextScreen();
            if (forfeitScreen is not null)
            {
                RequestedScreen = forfeitScreen;
            }
            else if (matchView.Phase == MatchPhase.PlayerTurn)
            {
                _doomLauncher.LaunchIfEnabled();
                RequestedScreen = ScreenId.Match;
            }
            else if (matchView.Phase == MatchPhase.Finished)
            {
                RequestedScreen = ScreenId.Result;
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
            var clockPill = new HorizontalStackPanel
            {
                Spacing = MatchLayout.HeaderCardSpacing,
                Padding = MatchLayout.HeaderCardPadding,
                Background = _volumeTextures.PillBrush,
            };
            clockPill.Widgets.Add(_clockRing);
            clockPill.Widgets.Add(_clockLabel);
            clockPill.Widgets.Add(_turnLabel);
            var topBar = new HorizontalStackPanel
            {
                Spacing = MatchLayout.TopBarSpacing,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = MatchLayout.TableEdgeMargin,
            };
            topBar.Widgets.Add(_playersBar.Root);
            topBar.Widgets.Add(clockPill);

            var table = new Panel
            {
                Background = Theme.MatchBackgroundBrush,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            table.Widgets.Add(_boardWidget);
            table.Widgets.Add(topBar);
            table.Widgets.Add(_instructionPill);
            table.Widgets.Add(_noticeBox);
            table.Widgets.Add(BuildRightColumn());
            table.Widgets.Add(_chatOverlay);

            return table;
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
                MatchActionResult.Applied or MatchActionResult.NotEnoughActionPoints or MatchActionResult.NoCaterpillarsLeft => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
                MatchActionResult.NotYourCaterpillar or MatchActionResult.NotNeighborCell or MatchActionResult.ClimbTooHigh => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
                MatchActionResult.FlowerAtMaximumLevel or MatchActionResult.NoBuildsLeft or MatchActionResult.NotNextToPlanter => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
                MatchActionResult.JoinsPlanters or MatchActionResult.DeckEmpty or MatchActionResult.CardNotInHand => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
                MatchActionResult.PointsCardAlreadyUsed or MatchActionResult.WrongCardTarget => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
                _ => throw new ArgumentOutOfRangeException(nameof(placementRejection)),
            };

            return rejectionKey;
        }

        private HorizontalStackPanel BuildTurnOrderRow(MatchSeat seat, bool hasPlaced)
        {
            var nameLabel = new Label { Text = seat.DisplayName, Font = Fonts.Small, TextColor = Theme.Ink, VerticalAlignment = VerticalAlignment.Center };
            var turnOrderRow = new HorizontalStackPanel
            {
                Spacing = MatchLayout.PlayerRowSpacing,
                Padding = MatchLayout.PlayerRowPadding,
            };
            turnOrderRow.Widgets.Add(new Image
            {
                Width = MatchLayout.PlayerIconWidth,
                Height = MatchLayout.PlayerIconHeight,
                Renderable = _figureIcons.FindCaterpillarIcon(seat.Color),
                VerticalAlignment = VerticalAlignment.Center,
            });
            turnOrderRow.Widgets.Add(nameLabel);
            turnOrderRow.Widgets.Add(new Label
            {
                Text = hasPlaced ? DisplaySymbols.CheckMark : seat.SeatNumber.ToString(CultureInfo.CurrentCulture),
                Font = Fonts.Small,
                TextColor = hasPlaced ? Theme.ChosenAction : Theme.SoftInk,
                VerticalAlignment = VerticalAlignment.Center,
            });
            StackPanel.SetProportionType(nameLabel, ProportionType.Fill);

            return turnOrderRow;
        }

        private VerticalStackPanel BuildRightColumn()
        {
            var turnOrderCard = new Panel
            {
                Padding = MatchLayout.TableLabelPadding,
                Background = _volumeTextures.CardBrush,
            };
            turnOrderCard.Widgets.Add(CreateGameSection(TextKeys.InitialPlacement.TurnOrderLabel, _turnOrderPanel));
            var chatTabButton = new Button
            {
                Content = new Label { Text = DisplaySymbols.ChatTab, Font = Fonts.Body, TextColor = Theme.ChosenAction, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center },
                Width = MatchLayout.ChatTabWidth,
                Height = MatchLayout.ChatTabHeight,
                Background = _volumeTextures.PillBrush,
                OverBackground = _volumeTextures.HoveredPillBrush,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            chatTabButton.Click += (_, _) => _chatOverlay.Visible = !_chatOverlay.Visible;
            _forfeitView.Root.HorizontalAlignment = HorizontalAlignment.Right;
            var rightColumn = new VerticalStackPanel
            {
                Spacing = MatchLayout.RightColumnSpacing,
                Width = MatchLayout.ActionColumnWidth,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = MatchLayout.RightColumnMargin,
            };
            rightColumn.Widgets.Add(_forfeitView.Root);
            rightColumn.Widgets.Add(turnOrderCard);
            rightColumn.Widgets.Add(chatTabButton);

            return rightColumn;
        }

        private void ShowClock()
        {
            TimeSpan placementTimeLeft = TimeSpan.Zero;
            if (_playerSession.StepEndsAtUtc is DateTime placementEndsAtUtc)
            {
                placementTimeLeft = placementEndsAtUtc - DateTime.UtcNow;
            }

            _clockLabel.Text = DisplayFormats.FormatCountdown(placementTimeLeft);
            _clockRing.RemainingShare = _shownStepTime > TimeSpan.Zero ? (float)(placementTimeLeft / _shownStepTime) : 0f;
        }

        private void ShowMatchView(MatchView matchView)
        {
            bool isFirstView = _shownMatchView is null;
            _shownMatchView = matchView;
            _shownStepTime = matchView.StepTimeLeft ?? TimeSpan.Zero;
            bool isViewerPlacing = IsViewerPlacing(matchView);
            bool isButterflyPlacement = matchView.Phase == MatchPhase.ButterflyPlacement;
            _instructionPill.Visible = isViewerPlacing;
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
            _playersBar.Show(matchView);
            _turnOrderPanel.Widgets.Clear();
            foreach (MatchSeat seat in matchView.Seats)
            {
                bool hasPlaced = matchView.Caterpillars.Exists(caterpillar => caterpillar.SeatNumber == seat.SeatNumber);
                _turnOrderPanel.Widgets.Add(BuildTurnOrderRow(seat, hasPlaced));
            }

            MatchSeat viewerSeat = matchView.Seats.First(seat => seat.SeatNumber == matchView.ViewerSeatNumber);
            _boardWidget.ShowReserve(viewerSeat.CaterpillarsLeft, SeatColors.ColorFor(viewerSeat.Color), 0);
            _boardWidget.ShowHand(Array.Empty<HandCardState>(), matchView.DeckCount);
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

        private void ShowServerError()
        {
            string[]? noDetailArguments = null;
            _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, noDetailArguments);
        }
    }
}
