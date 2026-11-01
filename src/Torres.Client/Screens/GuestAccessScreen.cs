using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class GuestAccessScreen : Screen
    {
        private readonly ChannelFactory<IAccountService> _accountChannelFactory;
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _session;
        private readonly LabeledTextBox _aliasField = new LabeledTextBox(TextKeys.GuestAccess.MatchNameLabel, false);
        private readonly LocalizedLabel _errorLabel;
        private readonly LocalizedButton _enterButton;

        private IAccountService? _accountChannel;
        private Task<GuestAccessResult>? _guestAccessRequest;
        private Task<OpenSessionResult>? _sessionOpening;
        private string? _enteringAlias;

        internal GuestAccessScreen(ChannelFactory<IAccountService> accountChannelFactory, GameSessionConnection gameSessionConnection, PlayerSession session)
            : base(TextKeys.GuestAccess.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(accountChannelFactory);
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(session);

            _accountChannelFactory = accountChannelFactory;
            _gameSessionConnection = gameSessionConnection;
            _session = session;
            _errorLabel = CreateErrorMessage(TextKeys.Common.RequiredField);
            _enterButton = CreatePrimaryButton(TextKeys.GuestAccess.EnterButton);
            _aliasField.Box.ValueChanging += AliasBoxOnValueChanging;
        }

        internal bool WasServerUnreachable { get; private set; }

        internal override void Open()
        {
            _errorLabel.Visible = false;
        }

        internal override void Update(GameTime gameTime)
        {
            FollowGuestAccess();
            FollowSessionOpening();
        }

        internal void ClearRequests()
        {
            WasServerUnreachable = false;
        }

        protected override Widget Build()
        {
            HorizontalStackPanel columns = CreateColumns();
            columns.Widgets.Add(BuildCard());
            columns.Widgets.Add(CreateAmbiencePanel());
            StackPanel.SetProportionType(columns.Widgets[1], ProportionType.Fill);

            return columns;
        }

        private VerticalStackPanel BuildCard()
        {
            VerticalStackPanel card = CreateCard(Sizes.GuestCardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(CreateCardHeader(TextKeys.GuestAccess.Title, TextKeys.GuestAccess.Hint));

            var nameField = new VerticalStackPanel { Spacing = Metrics.FieldLabelSpacing };
            nameField.Widgets.Add(_aliasField);
            nameField.Widgets.Add(_errorLabel);
            nameField.Widgets.Add(CreateSuccessMessage(TextKeys.GuestAccess.RepeatableName));
            card.Widgets.Add(nameField);

            card.Widgets.Add(CreateParagraph(TextKeys.GuestAccess.Limitations));

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackButton);
            _enterButton.Click += EnterButtonOnClick;
            backButton.Click += BackButtonOnClick;
            HorizontalStackPanel guestActions = CreateButtonRow();
            guestActions.Widgets.Add(_enterButton);
            guestActions.Widgets.Add(backButton);
            card.Widgets.Add(guestActions);

            return card;
        }

        private void FollowGuestAccess()
        {
            if ((_guestAccessRequest is null) || !_guestAccessRequest.IsCompleted)
            {
                return;
            }

            Task<GuestAccessResult> completedAccess = _guestAccessRequest;
            _guestAccessRequest = null;
            AbortChannel();
            ShowOutcome(completedAccess);
        }

        private void FollowSessionOpening()
        {
            if ((_sessionOpening is null) || !_sessionOpening.IsCompleted)
            {
                return;
            }

            Task<OpenSessionResult> completedOpening = _sessionOpening;
            string? enteringAlias = _enteringAlias;
            _sessionOpening = null;
            _enteringAlias = null;
            _enterButton.Enabled = true;
            bool isSessionOpen = completedOpening.IsCompletedSuccessfully
                && (completedOpening.Result.Status == OpenSessionStatus.SessionOpened);
            if (isSessionOpen && (enteringAlias is not null))
            {
                EnterAs(enteringAlias, completedOpening.Result);
            }
            else
            {
                _gameSessionConnection.Abandon();
                ReportServerUnreachable();
            }
        }

        private void EnterButtonOnClick(object sender, MyraEventArgs e)
        {
            _errorLabel.Visible = false;
            string guestAlias = _aliasField.Value.Trim();
            if (guestAlias.Length == 0)
            {
                ShowError(TextKeys.Common.RequiredField);
                return;
            }

            _enterButton.Enabled = false;
            _enteringAlias = guestAlias;
            IAccountService accountChannel = _accountChannelFactory.CreateChannel();
            _accountChannel = accountChannel;
            _guestAccessRequest = new SessionCall<string, GuestAccessResult>(accountChannel.EnterAsGuest, guestAlias).StartAsync();
        }

        private void ShowOutcome(Task<GuestAccessResult> completedAccess)
        {
            if (!completedAccess.IsCompletedSuccessfully)
            {
                ReportServerUnreachable();
                return;
            }

            GuestAccessResult accessResult = completedAccess.Result;
            switch (accessResult.Status)
            {
                case GuestAccessStatus.Entered:
                    OpenSession(accessResult.SessionToken);
                    break;

                case GuestAccessStatus.AliasRequired:
                case GuestAccessStatus.AliasTooLong:
                    _enterButton.Enabled = true;
                    _enteringAlias = null;
                    ShowError(TextKeys.Common.RequiredField);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(completedAccess), accessResult.Status, null);
            }
        }

        private void OpenSession(string? sessionToken)
        {
            if (string.IsNullOrEmpty(sessionToken))
            {
                ReportServerUnreachable();
                return;
            }

            _sessionOpening = _gameSessionConnection.OpenAsync(sessionToken);
        }

        private void EnterAs(string guestAlias, OpenSessionResult opening)
        {
            _gameSessionConnection.Start(opening);
            _session.StartAsGuest(opening.Identity?.DisplayName ?? guestAlias);
            _aliasField.Box.Text = string.Empty;
            if (opening.CurrentRoom is null)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
            else
            {
                _session.EnterRoom(opening.CurrentRoom);
                RequestedScreen = ScreenId.Room;
            }
        }

        private void ReportServerUnreachable()
        {
            _enterButton.Enabled = true;
            _enteringAlias = null;
            WasServerUnreachable = true;
        }

        private void ShowError(string textKey)
        {
            _errorLabel.TextKey = textKey;
            _errorLabel.Visible = true;
        }

        private void AbortChannel()
        {
            if (_accountChannel is ICommunicationObject openChannel)
            {
                openChannel.Abort();
            }

            _accountChannel = null;
        }

        private void AliasBoxOnValueChanging(object sender, ValueChangingEventArgs<string> e)
        {
            if ((e.NewValue is not null) && (e.NewValue.Length > AccountLimits.LongestGuestAlias))
            {
                e.Cancel = true;
            }
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Login;
        }
    }
}
