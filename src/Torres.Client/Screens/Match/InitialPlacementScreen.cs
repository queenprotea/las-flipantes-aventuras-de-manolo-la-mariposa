using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.Scene;
using Torres.Client.UI;
using Torres.Client.Widgets;

namespace Torres.Client.Screens
{
    internal sealed class InitialPlacementScreen : Screen, IDisposable
    {
        private readonly InitialPlacementFlow _initialPlacementFlow;
        private readonly DoomLauncher _doomLauncher;
        private readonly BoardWidget _boardWidget;
        private readonly FigureIcons _figureIcons;
        private readonly VolumeTextures _volumeTextures;
        private readonly MatchPlayersBarView _playersBar;
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly ForfeitSection _forfeitSection;
        private readonly ChatSection _chatSection;
        private readonly VerticalStackPanel _turnOrderPanel = new VerticalStackPanel();
        private readonly Label _clockLabel = new Label
        {
            Font = Fonts.Clock,
            TextColor = Theme.Ink,
            VerticalAlignment = VerticalAlignment.Center,
        };
        private readonly FlowerClock _flowerClock;
        private readonly LocalizedLabel _instructionLabel = new LocalizedLabel(TextKeys.InitialPlacement.PlaceCaterpillarInstruction)
        {
            Font = Fonts.Small,
            TextColor = Theme.Surface,
        };
        private readonly LocalizedLabel _turnLabel = new LocalizedLabel(TextKeys.InitialPlacement.CurrentTurnInstruction)
        {
            Font = Fonts.Small,
            TextColor = Theme.MintInk,
            VerticalAlignment = VerticalAlignment.Center,
        };
        private readonly Panel _instructionPill;
        private readonly MatchTableFrame _tableFrame;

        private MatchView? _shownMatchView;
        private TimeSpan _shownStepTime;

        internal InitialPlacementScreen(GraphicsDevice graphicsDevice, InitialPlacementFlow initialPlacementFlow, ForfeitFlow forfeitFlow, ChatFlow chatFlow, DoomLauncher doomLauncher, MatchPause matchPause)
            : base(TextKeys.InitialPlacement.HeaderLabel, true)
        {
            _initialPlacementFlow = initialPlacementFlow;
            _doomLauncher = doomLauncher;
            _forfeitSection = new ForfeitSection(forfeitFlow, TextKeys.Match.ForfeitButton);
            _chatSection = new ChatSection(chatFlow, MatchLayout.SideColumnWidth);
            _boardWidget = new BoardWidget(graphicsDevice, BoardLayout.Empty);
            _figureIcons = matchPause.FigureIcons;
            _volumeTextures = matchPause.VolumeTextures;
            _tableFrame = new MatchTableFrame(_volumeTextures, _chatSection.Root, _noticeBox);
            _playersBar = new MatchPlayersBarView(_figureIcons, _volumeTextures);
            _flowerClock = new FlowerClock(_volumeTextures) { VerticalAlignment = VerticalAlignment.Center };
            _instructionPill = _tableFrame.CreateHintPill(_instructionLabel);
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            _forfeitSection.Reset();
            _chatSection.Open();
            _shownMatchView = null;
            RequestedScreen = _initialPlacementFlow.Open();
        }

        internal override void Update(GameTime gameTime)
        {
            _chatSection.Update();
            MatchView? matchView = _initialPlacementFlow.CurrentMatchView;
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
            FollowBoardClick();
            if (_initialPlacementFlow.TakeNotice() is Notice notice)
            {
                DisplayNotice(_noticeBox, notice);
            }

            ScreenId? nextScreen = _forfeitSection.TakeNextScreen() ?? InitialPlacementFlow.FindPhaseScreen(matchView);
            if (nextScreen == ScreenId.Match)
            {
                _doomLauncher.LaunchIfEnabled();
            }

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
            var clockPill = new HorizontalStackPanel
            {
                Spacing = MatchLayout.HeaderCardSpacing,
                Padding = MatchLayout.HeaderCardPadding,
                Background = _volumeTextures.PillBrush,
            };
            clockPill.Widgets.Add(_flowerClock);
            clockPill.Widgets.Add(_clockLabel);
            clockPill.Widgets.Add(_turnLabel);
            Panel tablePanel = MatchTableFrame.CreateTablePanel(_boardWidget);
            tablePanel.Widgets.Add(MatchTableFrame.CreateTopBar(_playersBar.Root, clockPill));
            tablePanel.Widgets.Add(_instructionPill);
            _tableFrame.CompleteTablePanel(tablePanel, BuildRightColumn());

            return tablePanel;
        }

        private static string GetPlacingPlayerName(MatchView matchView)
        {
            string placingName = matchView.Seats.First(seat => seat.SeatNumber == matchView.ActiveSeatNumber).DisplayName;

            return placingName;
        }

        private HorizontalStackPanel BuildTurnOrderRow(MatchSeat seat, bool hasPlaced)
        {
            var nameLabel = new Label
            {
                Text = seat.DisplayName,
                Font = Fonts.Small,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
            };
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
            turnOrderCard.Widgets.Add(PanelFactory.CreateGameSection(TextKeys.InitialPlacement.TurnOrderLabel, _turnOrderPanel));
            _forfeitSection.Root.HorizontalAlignment = HorizontalAlignment.Right;
            var rightColumn = new VerticalStackPanel
            {
                Spacing = MatchLayout.RightColumnSpacing,
                Width = MatchLayout.ActionColumnWidth,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = MatchLayout.RightColumnMargin,
            };
            rightColumn.Widgets.Add(_forfeitSection.Root);
            rightColumn.Widgets.Add(turnOrderCard);
            rightColumn.Widgets.Add(_tableFrame.ChatTabButton);

            return rightColumn;
        }

        private void ShowClock()
        {
            TimeSpan placementTimeLeft = _initialPlacementFlow.PlacementTimeLeft;

            _clockLabel.Text = DisplayFormats.FormatCountdown(placementTimeLeft);
            _flowerClock.RemainingProgress = _shownStepTime > TimeSpan.Zero ? (float)(placementTimeLeft / _shownStepTime) : 0f;
        }

        private void ShowMatchView(MatchView matchView)
        {
            bool isFirstView = _shownMatchView is null;
            _shownMatchView = matchView;
            _shownStepTime = matchView.StepTimeLeft ?? TimeSpan.Zero;
            bool isViewerPlacing = InitialPlacementFlow.IsViewerPlacing(matchView);
            bool isButterflyPlacement = matchView.Phase == MatchPhase.ButterflyPlacement;
            _instructionPill.Visible = isViewerPlacing;
            _instructionLabel.TextKey = isButterflyPlacement ? TextKeys.InitialPlacement.PlaceButterflyInstruction : TextKeys.InitialPlacement.PlaceCaterpillarInstruction;
            _turnLabel.TextKey = isButterflyPlacement ? TextKeys.InitialPlacement.ButterflyTurnInstruction : TextKeys.InitialPlacement.CurrentTurnInstruction;
            _turnLabel.TextArguments = new string[] { GetPlacingPlayerName(matchView) };

            BoardLayout boardLayout = MatchBoardLayoutFactory.Create(matchView);
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

        private void FollowBoardClick()
        {
            BoardCell? clickedCell = _boardWidget.TakeClickedCell();
            if ((clickedCell is BoardCell placementCell) && _initialPlacementFlow.Place(MatchCellConversion.ToMatchCell(placementCell)))
            {
                _noticeBox.Hide();
            }
        }
    }
}
