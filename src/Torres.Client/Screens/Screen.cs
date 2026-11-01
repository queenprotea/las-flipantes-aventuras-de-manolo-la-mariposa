using FontStashSharp.RichText;

using Microsoft.Xna.Framework;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal abstract class Screen
    {
        private Widget? _rootWidget;
        private VerticalStackPanel? _confirmationPlaceholder;

        protected Screen(string headerKey, bool showsTopBar)
        {
            HeaderKey = headerKey;
            ShowsTopBar = showsTopBar;
        }

        internal string HeaderKey { get; }

        internal bool ShowsTopBar { get; }

        internal ScreenId? RequestedScreen { get; set; }

        internal string[]? HeaderArguments { get; private protected set; }

        internal Widget Root => _rootWidget ??= Build();

        internal bool IsBuilt => _rootWidget is not null;

        internal virtual void Update(GameTime gameTime)
        {
        }

        internal virtual void Open()
        {
        }

        protected VerticalStackPanel ConfirmationPlaceholder => _confirmationPlaceholder ??= new VerticalStackPanel { Spacing = Metrics.FieldSpacing, Visible = false };

        protected abstract Widget Build();

        protected static VerticalStackPanel CreatePage()
        {
            var page = new VerticalStackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            return page;
        }

        protected static HorizontalStackPanel CreateColumns()
        {
            var columns = new HorizontalStackPanel
            {
                Spacing = Metrics.ColumnSpacing,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            return columns;
        }

        protected static VerticalStackPanel CreateCard(int width)
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

        protected static VerticalStackPanel CreateCardHeader(string titleKey, string hintKey)
        {
            var cardHeader = new VerticalStackPanel { Spacing = Metrics.CardHeaderSpacing };
            cardHeader.Widgets.Add(CreateTitle(titleKey));
            cardHeader.Widgets.Add(CreateHint(hintKey));

            return cardHeader;
        }

        protected static LocalizedLabel CreateTitle(string textKey)
        {
            var title = new LocalizedLabel(textKey)
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
                Wrap = true,
            };

            return title;
        }

        protected static LocalizedLabel CreateHint(string textKey)
        {
            var hint = new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                Wrap = true,
            };

            return hint;
        }

        protected static LocalizedLabel CreateParagraph(string textKey)
        {
            var paragraph = new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
                Wrap = true,
            };

            return paragraph;
        }

        protected static LocalizedLabel CreateErrorMessage(string textKey)
        {
            var errorMessage = new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.BlushInk,
                Wrap = true,
                Visible = false,
            };

            return errorMessage;
        }

        protected static LocalizedLabel CreateSuccessMessage(string textKey)
        {
            var successMessage = new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
                Wrap = true,
            };

            return successMessage;
        }

        protected static LocalizedButton CreatePrimaryButton(string textKey)
        {
            LocalizedButton primaryButton = CreateBaseButton(textKey, Theme.MintInk);
            primaryButton.Background = Theme.MintBrush;
            primaryButton.OverBackground = Theme.MintAltBrush;
            primaryButton.PressedBackground = Theme.MintAltBrush;
            primaryButton.Border = Theme.MintLineBrush;

            return primaryButton;
        }

        protected static LocalizedButton CreateSecondaryButton(string textKey)
        {
            LocalizedButton secondaryButton = CreateBaseButton(textKey, Theme.Ink);
            secondaryButton.Background = Theme.SurfaceBrush;
            secondaryButton.OverBackground = Theme.SurfaceSunkenBrush;
            secondaryButton.PressedBackground = Theme.SurfaceSunkenBrush;
            secondaryButton.Border = Theme.StrongLineBrush;

            return secondaryButton;
        }

        protected static LocalizedButton CreateSmallSecondaryButton(string textKey)
        {
            LocalizedButton smallSecondaryButton = CreateSecondaryButton(textKey);
            smallSecondaryButton.LabelFont = Fonts.Small;
            smallSecondaryButton.Padding = Metrics.SmallButtonPadding;

            return smallSecondaryButton;
        }

        protected static LocalizedButton CreateDestructiveButton(string textKey)
        {
            LocalizedButton destructiveButton = CreateBaseButton(textKey, Theme.BlushInk);
            destructiveButton.Background = Theme.BlushTintBrush;
            destructiveButton.OverBackground = Theme.BlushTintBrush;
            destructiveButton.PressedBackground = Theme.BlushTintBrush;
            destructiveButton.Border = Theme.BlushBrush;

            return destructiveButton;
        }

        protected static LocalizedButton CreateLavenderButton(string textKey)
        {
            LocalizedButton lavenderButton = CreateBaseButton(textKey, Theme.LavenderInk);
            lavenderButton.Background = Theme.LavenderBrush;
            lavenderButton.OverBackground = Theme.LavenderTintBrush;
            lavenderButton.PressedBackground = Theme.LavenderTintBrush;
            lavenderButton.Border = Theme.LavenderLineBrush;

            return lavenderButton;
        }

        protected static LocalizedButton CreateLinkButton(string textKey)
        {
            var linkButton = new LocalizedButton(textKey)
            {
                LabelColor = Theme.LavenderInk,
                LabelFont = Fonts.Small,
                Padding = Metrics.LinkButtonPadding,
                Background = null,
                OverBackground = null,
                PressedBackground = null,
                Border = null,
                BorderThickness = new Thickness(0),
            };

            return linkButton;
        }

        protected static HorizontalStackPanel CreateButtonRow()
        {
            var buttonRow = new HorizontalStackPanel { Spacing = Metrics.ButtonSpacing };

            return buttonRow;
        }

        protected static VerticalStackPanel CreateWrappedButtonRow()
        {
            var wrappedButtonRow = new VerticalStackPanel
            {
                Spacing = Metrics.ButtonSpacing,
                HorizontalAlignment = HorizontalAlignment.Left,
            };

            return wrappedButtonRow;
        }

        protected static HorizontalStackPanel CreateBackBar(LocalizedButton backButton)
        {
            HorizontalStackPanel backBar = CreateButtonRow();
            backBar.Margin = new Thickness(0, Metrics.ColumnSpacing, 0, 0);
            backBar.VerticalAlignment = VerticalAlignment.Bottom;
            backBar.Widgets.Add(backButton);

            return backBar;
        }

        protected static Panel CreateBackBar(LocalizedButton backButton, LocalizedButton trailingButton)
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

        protected static Panel CreateAmbiencePanel()
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

        protected static Panel CreateAvatar()
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

        protected static HorizontalStackPanel CreateListItem()
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

        protected static Panel CreateSmallAvatar(IBrush avatarBackground)
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

        protected static Panel CreateChip(LocalizedLabel chipLabel, IBrush chipBackground, IBrush chipBorder)
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

        protected static Label CreatePlayerNameLabel(string playerName, Color textColor)
        {
            var playerNameLabel = new Label
            {
                Text = playerName,
                Font = Fonts.Control,
                TextColor = textColor,
                AutoEllipsisMethod = AutoEllipsisMethod.Character,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return playerNameLabel;
        }

        protected static VerticalStackPanel CreateGameSection(string labelKey, Widget sectionContent)
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

        protected static VerticalStackPanel CreateChatPanel()
        {
            var chatPanel = new VerticalStackPanel
            {
                Spacing = Metrics.ChatSpacing,
                Padding = Metrics.ChatPadding,
                MinHeight = Metrics.ChatMinHeight,
                Background = Theme.SurfaceSunkenBrush,
            };

            var spacer = new Panel();
            chatPanel.Widgets.Add(new LocalizedLabel(TextKeys.Chat.NoMessages)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
            });
            chatPanel.Widgets.Add(spacer);
            chatPanel.Widgets.Add(CreateDivider());
            chatPanel.Widgets.Add(new LocalizedLabel(TextKeys.Chat.InputPlaceholder)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
            });
            StackPanel.SetProportionType(spacer, ProportionType.Fill);

            return chatPanel;
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

        private static LocalizedButton CreateBaseButton(string textKey, Color labelColor)
        {
            var baseButton = new LocalizedButton(textKey)
            {
                LabelColor = labelColor,
                LabelFont = Fonts.Control,
                Padding = Metrics.ButtonPadding,
                BorderThickness = Sizes.Border,
            };

            return baseButton;
        }

        protected static Panel CreateItemCard(Widget itemContent)
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

        protected static Panel CreateStatusBadge(string textKey)
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

        protected static Label CreateUsernameLabel(string username)
        {
            var usernameLabel = new Label
            {
                Text = username,
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return usernameLabel;
        }

        protected static VerticalStackPanel CreateEmptyState(string titleKey, string hintKey)
        {
            var emptyState = new VerticalStackPanel
            {
                Spacing = Metrics.FieldSpacing,
                Width = Sizes.EmptyStateWidth,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            LocalizedLabel title = CreateTitle(titleKey);
            title.TextAlign = TextHorizontalAlignment.Center;
            emptyState.Widgets.Add(title);

            LocalizedLabel hint = CreateHint(hintKey);
            hint.TextAlign = TextHorizontalAlignment.Center;
            emptyState.Widgets.Add(hint);

            return emptyState;
        }

        protected NoticeBox ShowConfirmation(string titleKey, string bodyKey)
        {
            ConfirmationPlaceholder.Widgets.Clear();
            var question = new NoticeBox();
            question.ShowMessage(titleKey, bodyKey, null);
            ConfirmationPlaceholder.Widgets.Add(question);
            ConfirmationPlaceholder.Visible = true;

            return question;
        }

        protected void AddConfirmationButtons(LocalizedButton confirmButton, LocalizedButton cancelButton)
        {
            HorizontalStackPanel answerRow = CreateButtonRow();
            answerRow.Widgets.Add(confirmButton);
            answerRow.Widgets.Add(cancelButton);
            ConfirmationPlaceholder.Widgets.Add(answerRow);
        }

        protected void HideConfirmation()
        {
            ConfirmationPlaceholder.Widgets.Clear();
            ConfirmationPlaceholder.Visible = false;
        }
    }
}
