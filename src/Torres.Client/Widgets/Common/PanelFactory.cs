using FontStashSharp.RichText;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.UI;

namespace Torres.Client.Widgets
{
    internal static class PanelFactory
    {
        private const int FailureNoticeTopSpacing = 14;

        internal static VerticalStackPanel CreatePage()
        {
            var page = new VerticalStackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            return page;
        }

        internal static HorizontalStackPanel CreateColumns()
        {
            var columns = new HorizontalStackPanel
            {
                Spacing = Metrics.ColumnSpacing,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            return columns;
        }

        internal static VerticalStackPanel CreateCard(int width)
        {
            var card = new VerticalStackPanel
            {
                Spacing = Metrics.FieldSpacing,
                Width = width,
                Padding = Metrics.CardPadding,
                Background = Theme.SurfaceBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Top,
            };

            return card;
        }

        internal static VerticalStackPanel CreateCardHeader(string titleKey, string hintKey)
        {
            var cardHeader = new VerticalStackPanel { Spacing = Metrics.CardHeaderSpacing };
            cardHeader.Widgets.Add(LabelFactory.CreateTitle(titleKey));
            cardHeader.Widgets.Add(LabelFactory.CreateHint(hintKey));

            return cardHeader;
        }

        internal static HorizontalStackPanel CreateButtonRow()
        {
            var buttonRow = new HorizontalStackPanel { Spacing = Metrics.ButtonSpacing };

            return buttonRow;
        }

        internal static VerticalStackPanel CreateWrappedButtonRow()
        {
            var wrappedButtonRow = new VerticalStackPanel
            {
                Spacing = Metrics.ButtonSpacing,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            return wrappedButtonRow;
        }

        internal static HorizontalStackPanel CreateBackBar(LocalizedButton backButton)
        {
            HorizontalStackPanel backBar = PanelFactory.CreateButtonRow();
            backBar.Margin = new Thickness(0, Metrics.ColumnSpacing, 0, 0);
            backBar.VerticalAlignment = VerticalAlignment.Bottom;
            backBar.Widgets.Add(backButton);

            return backBar;
        }

        internal static Panel CreateBackBar(LocalizedButton backButton, LocalizedButton trailingButton)
        {
            var backBar = new Panel
            {
                Margin = new Thickness(0, Metrics.ColumnSpacing, 0, 0),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Bottom,
            };
            backButton.HorizontalAlignment = HorizontalAlignment.Left;
            trailingButton.HorizontalAlignment = HorizontalAlignment.Right;
            backBar.Widgets.Add(backButton);
            backBar.Widgets.Add(trailingButton);

            return backBar;
        }

        internal static Panel CreateAmbiencePanel()
        {
            var ambiencePanel = new Panel
            {
                Background = Theme.MintTintBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            return ambiencePanel;
        }

        internal static Panel CreateAvatar()
        {
            var avatar = new Panel
            {
                Width = Sizes.LargeAvatarSize,
                Height = Sizes.LargeAvatarSize,
                Background = Theme.MintAltBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Center,
            };

            return avatar;
        }

        internal static HorizontalStackPanel CreateListItem()
        {
            var listItem = new HorizontalStackPanel
            {
                Spacing = Metrics.ListItemSpacing,
                Padding = Metrics.ListItemPadding,
                Background = Theme.SurfaceBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
            };

            return listItem;
        }

        internal static Panel CreateSmallAvatar(IBrush avatarBackground)
        {
            var smallAvatar = new Panel
            {
                Width = Sizes.SmallAvatarSize,
                Height = Sizes.SmallAvatarSize,
                Background = avatarBackground,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return smallAvatar;
        }

        internal static Panel CreateChip(LocalizedLabel chipLabel, IBrush chipBackground, IBrush chipBorder)
        {
            chipLabel.Font = Fonts.Label;

            var chipPanel = new Panel
            {
                Padding = Metrics.ChipPadding,
                Background = chipBackground,
                Border = chipBorder,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            chipPanel.Widgets.Add(chipLabel);

            return chipPanel;
        }

        internal static VerticalStackPanel CreateGameSection(string labelKey, Widget sectionContent)
        {
            var gameSection = new VerticalStackPanel
            {
                Spacing = MatchLayout.SectionLabelSpacing,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            gameSection.Widgets.Add(new LocalizedLabel(labelKey)
            {
                Font = Fonts.Label,
                TextColor = Theme.MutedInk,
            });
            gameSection.Widgets.Add(sectionContent);

            return gameSection;
        }

        internal static Panel CreateDivider()
        {
            var divider = new Panel
            {
                Height = Sizes.BorderSize,
                Background = Theme.LineBrush,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            return divider;
        }

        internal static Panel CreateSeatSwatch(SeatColor seatColor)
        {
            var seatSwatch = new Panel
            {
                Width = Sizes.SeatSwatchSize,
                Height = Sizes.SeatSwatchSize,
                Background = SeatColors.BrushFor(seatColor),
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return seatSwatch;
        }

        internal static Panel CreateItemCard(Widget itemContent)
        {
            var itemCard = new Panel
            {
                Padding = Metrics.ListItemPadding,
                Background = Theme.SurfaceAltBrush,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            itemCard.Widgets.Add(itemContent);

            return itemCard;
        }

        internal static Panel CreateStatusBadge(string textKey)
        {
            var statusBadge = new Panel
            {
                Padding = Metrics.PillButtonPadding,
                Background = Theme.MintTintBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            statusBadge.Widgets.Add(new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
            });

            return statusBadge;
        }

        internal static HorizontalStackPanel CreateConnectionStatusRow(bool isOnline)
        {
            Color connectionDotColor = isOnline ? Theme.MintLine : Theme.Blush;
            string connectionTextKey = isOnline ? TextKeys.Friends.OnlineStatus : TextKeys.Friends.OfflineStatus;

            var connectionDot = new Label
            {
                Text = DisplaySymbols.ConnectionDot,
                Font = Fonts.Small,
                TextColor = connectionDotColor,
                VerticalAlignment = VerticalAlignment.Center,
            };
            var connectionLabel = new LocalizedLabel(connectionTextKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var connectionStatusRow = new HorizontalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            connectionStatusRow.Widgets.Add(connectionDot);
            connectionStatusRow.Widgets.Add(connectionLabel);

            return connectionStatusRow;
        }

        internal static VerticalStackPanel CreateEmptyState(string titleKey, string hintKey)
        {
            var emptyState = new VerticalStackPanel
            {
                Spacing = Metrics.FieldSpacing,
                Width = Sizes.EmptyStateWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            LocalizedLabel title = LabelFactory.CreateTitle(titleKey);
            title.TextAlign = TextHorizontalAlignment.Center;
            emptyState.Widgets.Add(title);

            LocalizedLabel hint = LabelFactory.CreateHint(hintKey);
            hint.TextAlign = TextHorizontalAlignment.Center;
            emptyState.Widgets.Add(hint);

            return emptyState;
        }

        internal static VerticalStackPanel CreateFailureBox(string titleKey)
        {
            var failureBox = new VerticalStackPanel
            {
                Padding = Metrics.CardPadding,
                Background = Theme.BlushTintBrush,
                Border = Theme.BlushBrush,
                BorderThickness = Sizes.Border,
            };
            failureBox.Widgets.Add(new LocalizedLabel(titleKey)
            {
                Font = Fonts.Body,
                TextColor = Theme.BlushInk,
                Wrap = true,
            });

            return failureBox;
        }

        internal static VerticalStackPanel CreateFailureMessage()
        {
            VerticalStackPanel failureMessage = CreateFailureBox(TextKeys.Common.ServerErrorTitle);
            failureMessage.Widgets.Add(LabelFactory.CreateParagraph(TextKeys.Common.ServerErrorDetail));

            return failureMessage;
        }

        internal static VerticalStackPanel CreateFailureNotice(LocalizedButton retryButton)
        {
            VerticalStackPanel failureMessage = CreateFailureMessage();

            var failureNotice = new VerticalStackPanel
            {
                Spacing = Metrics.FieldSpacing,
                Margin = new Thickness(0, FailureNoticeTopSpacing, 0, 0),
                Visible = false,
            };
            failureNotice.Widgets.Add(failureMessage);
            failureNotice.Widgets.Add(retryButton);

            return failureNotice;
        }
    }
}
