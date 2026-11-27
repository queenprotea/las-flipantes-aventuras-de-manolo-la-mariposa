using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Flows;
using Torres.Client.Localization;
using Torres.Client.UI;
using Torres.Client.Widgets;

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

        internal virtual bool KeepsEscapeKey => false;

        protected VerticalStackPanel ConfirmationPlaceholder => _confirmationPlaceholder ??= new VerticalStackPanel
        {
            Spacing = Metrics.FieldSpacing,
            Visible = false,
        };

        internal virtual void Update(GameTime gameTime)
        {
        }

        internal virtual void Open()
        {
        }

        internal virtual void Close()
        {
        }

        internal virtual void RenderScene()
        {
        }

        protected abstract Widget Build();

        protected NoticeBox ShowConfirmation(string titleKey, string bodyKey)
        {
            ConfirmationPlaceholder.Widgets.Clear();
            var confirmationNotice = new NoticeBox();
            confirmationNotice.ShowMessage(titleKey, bodyKey, null);
            ConfirmationPlaceholder.Widgets.Add(confirmationNotice);
            ConfirmationPlaceholder.Visible = true;

            return confirmationNotice;
        }

        protected void AddConfirmationButtons(LocalizedButton confirmButton, LocalizedButton cancelButton)
        {
            HorizontalStackPanel answerRow = PanelFactory.CreateButtonRow();
            answerRow.Widgets.Add(confirmButton);
            answerRow.Widgets.Add(cancelButton);
            ConfirmationPlaceholder.Widgets.Add(answerRow);
        }

        protected void HideConfirmation()
        {
            ConfirmationPlaceholder.Widgets.Clear();
            ConfirmationPlaceholder.Visible = false;
        }

        protected static void DisplayNotice(NoticeBox noticeBox, Notice notice)
        {
            if (notice.IsBan)
            {
                noticeBox.ShowBan(notice.BanEndsAt);
            }
            else if ((notice.TitleKey is string titleKey) && (notice.BodyKey is string bodyKey))
            {
                noticeBox.ShowMessage(titleKey, bodyKey, notice.BodyArguments);
            }
            else if (notice.TitleKey is string onlyTitleKey)
            {
                noticeBox.ShowTitle(onlyTitleKey);
            }
        }
    }
}
