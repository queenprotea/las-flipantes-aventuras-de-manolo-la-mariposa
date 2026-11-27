using System;
using System.Globalization;

using CoreWCF;
using CoreWCF.Channels;
using CoreWCF.Configuration;
using CoreWCF.Description;

using Game.Contracts;
using Game.Services;

using log4net;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Game.Server
{
    public sealed class ServerEndpoints
    {
        private const string MetadataHostName = "localhost";
        private const string MetadataPublishedMessage = "Service metadata published on {0}";

        private readonly ServerSettings _serverSettings;
        private readonly SessionSettings _sessionSettings;
        private readonly Uri _metadataBaseAddress;
        private readonly ILog _hostLog;

        public ServerEndpoints(ServerSettings serverSettings, SessionSettings sessionSettings, ILog hostLog)
        {
            _serverSettings = serverSettings;
            _sessionSettings = sessionSettings;
            _metadataBaseAddress = new UriBuilder(Uri.UriSchemeHttp, MetadataHostName, serverSettings.MetadataHttpPort).Uri;
            _hostLog = hostLog;
        }

        public void ConfigureHost(WebApplicationBuilder builder)
        {
            builder.WebHost.UseNetTcp(_serverSettings.NetTcpPort);
            builder.Services.AddServiceModelServices();
            if (_serverSettings.PublishesServiceMetadata)
            {
                builder.WebHost.ConfigureKestrel(kestrelOptions => kestrelOptions.ListenLocalhost(_serverSettings.MetadataHttpPort));
                builder.Services.AddServiceModelMetadata();
            }
        }

        public void Publish(WebApplication app)
        {
            if (_serverSettings.PublishesServiceMetadata)
            {
                ServiceMetadataBehavior metadataBehavior = app.Services.GetRequiredService<ServiceMetadataBehavior>();
                metadataBehavior.HttpGetEnabled = true;
                _hostLog.InfoFormat(CultureInfo.InvariantCulture, MetadataPublishedMessage, _metadataBaseAddress);
            }

            app.UseServiceModel(AddEndpoints);
        }

        private static NetTcpBinding CreateBinding()
        {
            var binding = new NetTcpBinding(SecurityMode.None);

            return binding;
        }

        private NetTcpBinding CreateGameSessionBinding()
        {
            NetTcpBinding gameSessionBinding = CreateBinding();
            gameSessionBinding.ReaderQuotas.MaxArrayLength = ProfileLimits.LargestArrayBytes;
            gameSessionBinding.ReceiveTimeout = _sessionSettings.ReceiveTimeout;
            gameSessionBinding.SendTimeout = _sessionSettings.SendTimeout;

            return gameSessionBinding;
        }

        private void AddEndpoints(IServiceBuilder serviceBuilder)
        {
            AddEndpoint<ServerStatusService, IServerStatusService>(serviceBuilder, CreateBinding(), ServiceEndpointPaths.ServerStatus);
            AddEndpoint<AccountService, IAccountService>(serviceBuilder, CreateBinding(), ServiceEndpointPaths.Account);
            AddEndpoint<RankingService, IRankingService>(serviceBuilder, CreateBinding(), ServiceEndpointPaths.Ranking);
            AddEndpoint<GameSessionService, IGameSessionService>(serviceBuilder, CreateGameSessionBinding(), ServiceEndpointPaths.GameSession);
        }

        private void AddEndpoint<TService, TContract>(IServiceBuilder serviceBuilder, Binding binding, string servicePath)
            where TService : class
        {
            serviceBuilder.AddService<TService>(serviceOptions => AddMetadataAddress(serviceOptions, servicePath));
            serviceBuilder.AddServiceEndpoint<TService, TContract>(binding, servicePath);
        }

        private void AddMetadataAddress(ServiceOptions serviceOptions, string servicePath)
        {
            if (_serverSettings.PublishesServiceMetadata)
            {
                var metadataAddress = new Uri(_metadataBaseAddress, servicePath);
                serviceOptions.BaseAddresses.Add(metadataAddress);
            }
        }
    }
}
