using System;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class NoticeBox : VerticalStackPanel
    {
        private readonly LocalizedLabel _titleLabel = new LocalizedLabel(TextKeys.Common.ServerErrorTitle)
        {
            Font = Fonts.Body,
            TextColor = Theme.BlushInk,
            Wrap = true,
        };

        private readonly LocalizedLabel _bodyLabel = new LocalizedLabel(TextKeys.Common.ServerErrorDetail)
        {
            Font = Fonts.Small,
            TextColor = Theme.SoftInk,
            Wrap = true,
        };

        internal NoticeBox()
        {
            Spacing = Metrics.CardHeaderSpacing;
            Padding = Metrics.CardPadding;
            Background = Theme.BlushTintBrush;
            Border = Theme.BlushBrush;
            BorderThickness = Sizes.Border;
            HorizontalAlignment = HorizontalAlignment.Stretch;
            Visible = false;
            Widgets.Add(_titleLabel);
            Widgets.Add(_bodyLabel);
        }

        internal void ShowTitle(string titleKey)
        {
            _titleLabel.TextArguments = null;
            _titleLabel.TextKey = titleKey;
            _bodyLabel.Visible = false;
            Visible = true;
        }

        internal void ShowMessage(string titleKey, string bodyKey, object[]? bodyArguments)
        {
            _titleLabel.TextArguments = null;
            _titleLabel.TextKey = titleKey;
            _bodyLabel.TextArguments = bodyArguments;
            _bodyLabel.TextKey = bodyKey;
            _bodyLabel.Visible = true;
            Visible = true;
        }

        internal void SetTitleArguments(object[]? titleArguments)
        {
            _titleLabel.TextArguments = titleArguments;
        }

        internal void ShowBan(DateTime? banEndsAtUtc)
        {
            if (banEndsAtUtc is DateTime endsAtUtc)
            {
                string remainingTime = DisplayFormats.FormatRemainingTime(endsAtUtc - DateTime.UtcNow);
                ShowMessage(TextKeys.Alerts.SuspensionTitle, TextKeys.Alerts.SuspensionBody, new object[] { remainingTime });
            }
            else
            {
                ShowMessage(TextKeys.Alerts.SuspensionTitle, TextKeys.Alerts.PermanentBanBody, null);
            }
        }

        internal void Hide()
        {
            Visible = false;
        }
    }
}
