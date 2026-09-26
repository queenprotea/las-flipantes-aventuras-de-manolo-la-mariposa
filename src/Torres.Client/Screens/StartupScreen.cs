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
        private const double ReadySeconds = 0.8;
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
            _statusCheck ??= StartStatusCheck();
            if ((_statusLabel is null) || (_failureNotice is null) || !_statusCheck.IsCompleted)
            {
                return;
            }

            bool isServerReady = _statusCheck.IsCompletedSuccessfully && (_statusCheck.Result == ServerStatus.Ready);
            if (isServerReady)
            {
                ShowReady(_statusLabel, gameTime);
                return;
            }

            _statusLabel.Visible = false;
            _failureNotice.Visible = true;
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

            return new VerticalStackPanel
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
        }

        private VerticalStackPanel BuildFailureNotice()
        {
            var message = new VerticalStackPanel
            {
                Padding = Theme.CardPadding,
                Background = Theme.BlushTintBrush,
                Border = Theme.BlushBrush,
                BorderThickness = Theme.Border,
            };
            message.Widgets.Add(new LocalizedLabel(TextKeys.Common.ServerErrorTitle)
            {
                Font = Fonts.Body,
                TextColor = Theme.BlushInk,
                Wrap = true,
            });
            message.Widgets.Add(Paragraph(TextKeys.Common.ServerErrorDetail));

            LocalizedButton retryButton = PrimaryButton(TextKeys.Common.RetryButton);
            LocalizedButton exitButton = SecondaryButton(TextKeys.Common.ExitButton);
            retryButton.Click += OnRetryClick;
            exitButton.Click += OnExitClick;
            HorizontalStackPanel actions = Row();
            actions.HorizontalAlignment = HorizontalAlignment.Center;
            actions.Widgets.Add(retryButton);
            actions.Widgets.Add(exitButton);

            var notice = new VerticalStackPanel
            {
                Width = NoticeWidth,
                Spacing = Theme.FieldSpacing,
                Margin = new Thickness(0, StatusSpacing, 0, 0),
                Visible = false,
            };
            notice.Widgets.Add(message);
            notice.Widgets.Add(actions);
            return notice;
        }

        private Task<ServerStatus> StartStatusCheck()
        {
            _statusChannel = _statusChannelFactory.CreateChannel();

            return _statusChannel.GetStatusAsync();
        }

        private void ShowReady(LocalizedLabel statusLabel, GameTime gameTime)
        {
            statusLabel.TextKey = TextKeys.Startup.ReadyStatus;
            _readyElapsedSeconds += gameTime.ElapsedGameTime.TotalSeconds;
            if (_readyElapsedSeconds >= ReadySeconds)
            {
                RequestedScreen = ScreenId.MainMenu;
            }
        }

        private void OnRetryClick(object sender, MyraEventArgs arguments)
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

        private void OnExitClick(object sender, MyraEventArgs arguments)
        {
            WasExitRequested = true;
        }
    }
}
