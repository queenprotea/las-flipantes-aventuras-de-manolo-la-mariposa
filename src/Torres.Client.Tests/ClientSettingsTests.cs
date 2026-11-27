using System;
using System.ServiceModel;
using System.Threading.Tasks;

using Xunit;

namespace Torres.Client.Tests
{
    public sealed class ClientSettingsTests
    {
        private const string ServerAddress = "net.tcp://localhost:8000";
        private const string AccountPath = "/account";
        private const string AccountAddress = "net.tcp://localhost:8000/account";
        private const double FiveSeconds = 5;
        private const double TwoHours = 120;

        private static readonly TimeSpan _fiveSeconds = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan _twoHours = TimeSpan.FromHours(2);

        private readonly ClientSettings _settings = new ClientSettings
        {
            ServerAddress = ServerAddress,
            FriendshipListRefreshSeconds = FiveSeconds,
            InvitationStateRefreshSeconds = FiveSeconds,
            SessionCloseTimeoutSeconds = FiveSeconds,
            ReconnectionRetrySeconds = FiveSeconds,
            ReceiveTimeoutMinutes = TwoHours,
            SendTimeoutSeconds = FiveSeconds,
            StepSilenceMarginSeconds = FiveSeconds,
        };

        [Fact]
        public async Task Test_ReadAsync_WithTheSettingsOfTheProject_ReadsThem()
        {
            ClientSettings? settings = await ClientSettings.ReadAsync();

            Assert.NotNull(settings);
        }

        [Fact]
        public void Test_CreateEndpointAddress_WithAServicePath_JoinsItToTheServerAddress()
        {
            EndpointAddress endpointAddress = _settings.CreateEndpointAddress(AccountPath);

            Assert.Equal(AccountAddress, endpointAddress.Uri.AbsoluteUri);
        }

        [Fact]
        public void Test_FriendshipListRefreshPeriod_InSeconds_ConvertsToATimeSpan()
        {
            TimeSpan refreshPeriod = _settings.FriendshipListRefreshPeriod;

            Assert.Equal(_fiveSeconds, refreshPeriod);
        }

        [Fact]
        public void Test_InvitationStateRefreshPeriod_InSeconds_ConvertsToATimeSpan()
        {
            TimeSpan refreshPeriod = _settings.InvitationStateRefreshPeriod;

            Assert.Equal(_fiveSeconds, refreshPeriod);
        }

        [Fact]
        public void Test_SessionCloseTimeout_InSeconds_ConvertsToATimeSpan()
        {
            TimeSpan closeTimeout = _settings.SessionCloseTimeout;

            Assert.Equal(_fiveSeconds, closeTimeout);
        }

        [Fact]
        public void Test_ReconnectionRetryInterval_InSeconds_ConvertsToATimeSpan()
        {
            TimeSpan retryInterval = _settings.ReconnectionRetryInterval;

            Assert.Equal(_fiveSeconds, retryInterval);
        }

        [Fact]
        public void Test_ReceiveTimeout_InMinutes_ConvertsToATimeSpan()
        {
            TimeSpan receiveTimeout = _settings.ReceiveTimeout;

            Assert.Equal(_twoHours, receiveTimeout);
        }

        [Fact]
        public void Test_SendTimeout_InSeconds_ConvertsToATimeSpan()
        {
            TimeSpan sendTimeout = _settings.SendTimeout;

            Assert.Equal(_fiveSeconds, sendTimeout);
        }

        [Fact]
        public void Test_StepSilenceMargin_InSeconds_ConvertsToATimeSpan()
        {
            TimeSpan silenceMargin = _settings.StepSilenceMargin;

            Assert.Equal(_fiveSeconds, silenceMargin);
        }
    }
}
