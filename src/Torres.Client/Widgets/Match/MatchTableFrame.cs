using Myra.Graphics2D.UI;

using Torres.Client.UI;

namespace Torres.Client.Widgets
{
    internal sealed class MatchTableFrame
    {
        private readonly VolumeTextures _volumeTextures;
        private readonly NoticeBox _noticeBox;
        private readonly Panel _chatOverlay;

        internal MatchTableFrame(VolumeTextures volumeTextures, Widget chatRoot, NoticeBox noticeBox)
        {
            _volumeTextures = volumeTextures;
            _noticeBox = noticeBox;
            _noticeBox.HorizontalAlignment = HorizontalAlignment.Center;
            _noticeBox.VerticalAlignment = VerticalAlignment.Top;
            _noticeBox.Width = Metrics.NoticeWidth;
            _noticeBox.Margin = MatchLayout.NoticeMargin;
            _chatOverlay = new Panel
            {
                Width = MatchLayout.ChatOverlayWidth + MatchLayout.ChatOverlayMargin.Right,
                Padding = MatchLayout.TableLabelPadding,
                Background = volumeTextures.CardBrush,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = MatchLayout.ChatOverlayMargin,
                Visible = false,
            };
            _chatOverlay.Widgets.Add(chatRoot);
            ChatTabButton = new Button
            {
                Content = new Label
                {
                    Text = DisplaySymbols.ChatTab,
                    Font = Fonts.Body,
                    TextColor = Theme.ChosenAction,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
                Width = MatchLayout.ChatTabWidth,
                Height = MatchLayout.ChatTabHeight,
                Background = volumeTextures.PillBrush,
                OverBackground = volumeTextures.HoveredPillBrush,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            ChatTabButton.Click += (sender, e) => _chatOverlay.Visible = !_chatOverlay.Visible;
        }

        internal Button ChatTabButton { get; }

        internal static HorizontalStackPanel CreateTopBar(Widget playersBar, Widget clock)
        {
            var topBar = new HorizontalStackPanel
            {
                Spacing = MatchLayout.TopBarSpacing,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = MatchLayout.TableEdgeMargin,
            };
            topBar.Widgets.Add(playersBar);
            topBar.Widgets.Add(clock);

            return topBar;
        }

        internal static Panel CreateTablePanel(Widget boardWidget)
        {
            var tablePanel = new Panel
            {
                Background = Theme.MatchBackgroundBrush,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            tablePanel.Widgets.Add(boardWidget);

            return tablePanel;
        }

        internal Panel CreateHintPill(Widget hintLabel)
        {
            var hintPill = new Panel
            {
                Padding = MatchLayout.HintPillPadding,
                Background = _volumeTextures.HintPillBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                Margin = MatchLayout.HintMargin,
            };
            hintPill.Widgets.Add(hintLabel);

            return hintPill;
        }

        internal void CompleteTablePanel(Panel tablePanel, Widget rightColumn)
        {
            tablePanel.Widgets.Add(_noticeBox);
            tablePanel.Widgets.Add(rightColumn);
            tablePanel.Widgets.Add(_chatOverlay);
        }
    }
}
