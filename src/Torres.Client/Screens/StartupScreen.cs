using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class StartupScreen : Screen
    {
        private const double ReadyDisplaySeconds = 0.8;
        private const int StatusSpacing = 14;
        private const int NoticeWidth = 420;

        private readonly ChannelFactory<IServerStatusService> _statusChannelFactory;

        private LocalizedLabel? _statusLabel;
        private VerticalStackPanel? _failureNotice;
        private IServerStatusService? _statusChannel;
        private Task<ServerStatus>? _statusCheck;
        private double _readyElapsedSeconds;

        internal StartupScreen(ChannelFactory<IServerStatusService> statusChannelFactory)
            : base(TextKeys.Startup.ConnectingStatus, false)
        {
            ArgumentNullException.ThrowIfNull(statusChannelFactory);

            _statusChannelFactory = statusChannelFactory;
        }

        internal bool WasExitRequested { get; private set; }

        internal override void Update(GameTime gameTime)
        {
            _statusCheck ??= StartStatusCheckAsync();
            if ((_statusLabel is null) || (_failureNotice is null) || !_statusCheck.IsCompleted)
            {
                return;
            }

            bool isServerReady = _statusCheck.IsCompletedSuccessfully && (_statusCheck.Result == ServerStatus.Ready);
            if (isServerReady)
            {
                ShowReady(_statusLabel, gameTime);
            }
            else
            {
                _statusLabel.Visible = false;
                _failureNotice.Visible = true;
            }
        }

        protected override Widget Build()
        {
            _statusLabel = new LocalizedLabel(TextKeys.Startup.ConnectingStatus)
            {
                Font = Fonts.Body,
                TextColor = Theme.MutedInk,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, StatusSpacing, 0, 0),
            };
            _failureNotice = BuildFailureNotice();

            var startupLayout = new VerticalStackPanel
            {
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Widgets =
                {
                    new LocalizedLabel(TextKeys.Common.GameName)
                    {
                        Font = Fonts.Display,
                        TextColor = Theme.Ink,
                        HorizontalAlignment = HorizontalAlignment.Center,
                    },
                    _statusLabel,
                    _failureNotice,
                },
            };

            return startupLayout;
        }

        private VerticalStackPanel BuildFailureNotice()
        {
            var failureMessage = new VerticalStackPanel
            {
                Padding = Metrics.CardPadding,
                Background = Theme.BlushTintBrush,
                Border = Theme.BlushBrush,
                BorderThickness = Sizes.Border,
            };
            failureMessage.Widgets.Add(new LocalizedLabel(TextKeys.Common.ServerErrorTitle)
            {
                Font = Fonts.Body,
                TextColor = Theme.BlushInk,
                Wrap = true,
            });
            failureMessage.Widgets.Add(CreateParagraph(TextKeys.Common.ServerErrorDetail));

            LocalizedButton retryButton = CreatePrimaryButton(TextKeys.Common.RetryButton);
            LocalizedButton exitButton = CreateSecondaryButton(TextKeys.Common.ExitButton);
            retryButton.Click += RetryButtonOnClick;
            exitButton.Click += ExitButtonOnClick;
            HorizontalStackPanel failureActions = CreateButtonRow();
            failureActions.HorizontalAlignment = HorizontalAlignment.Center;
            failureActions.Widgets.Add(retryButton);
            failureActions.Widgets.Add(exitButton);

            var failureNotice = new VerticalStackPanel
            {
                Width = NoticeWidth,
                Spacing = Metrics.FieldSpacing,
                Margin = new Thickness(0, StatusSpacing, 0, 0),
                Visible = false,
            };
            failureNotice.Widgets.Add(failureMessage);
            failureNotice.Widgets.Add(failureActions);

            return failureNotice;
        }

        private Task<ServerStatus> StartStatusCheckAsync()
        {
            _statusChannel = _statusChannelFactory.CreateChannel();

            Task<ServerStatus> statusCheck = _statusChannel.GetStatusAsync();

            return statusCheck;
        }

        private void ShowReady(LocalizedLabel statusLabel, GameTime gameTime)
        {
            statusLabel.TextKey = TextKeys.Startup.ReadyStatus;
            _readyElapsedSeconds += gameTime.ElapsedGameTime.TotalSeconds;
            if (_readyElapsedSeconds >= ReadyDisplaySeconds)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
        }

        private void RetryButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_statusChannel is ICommunicationObject failedChannel)
            {
                failedChannel.Abort();
            }

            _statusCheck = null;
            if ((_statusLabel is null) || (_failureNotice is null))
            {
                return;
            }

            _failureNotice.Visible = false;
            _statusLabel.Visible = true;
            _statusLabel.TextKey = TextKeys.Startup.ConnectingStatus;
        }

        private void ExitButtonOnClick(object sender, MyraEventArgs e)
        {
            WasExitRequested = true;
        }
    }
}
