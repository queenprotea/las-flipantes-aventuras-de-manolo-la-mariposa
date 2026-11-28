using System;

using Game.Services;

using Microsoft.Extensions.Configuration;

namespace Game.Server
{
    public sealed class EmailSettings
    {
        private const string SectionName = "Email";
        private const string SmtpUserVariable = "TORRES_SMTP_USER";
        private const string SmtpPasswordVariable = "TORRES_SMTP_PASSWORD";
        private const string InvalidEmailSettingsMessage = "The Email section of appsettings.json needs a SenderAddress, a SenderName, a positive SendTimeoutSeconds, and a Host and a positive Port for the chosen server.";
        private const string MissingCredentialsMessage = "The real email server needs the environment variables TORRES_SMTP_USER and TORRES_SMTP_PASSWORD.";

        public bool UsesRealServer { get; set; }

        public string SenderAddress { get; set; } = string.Empty;

        public string SenderName { get; set; } = string.Empty;

        public int SendTimeoutSeconds { get; set; }

        public EmailServerSetting Local { get; set; } = new EmailServerSetting();

        public EmailServerSetting Real { get; set; } = new EmailServerSetting();

        public string ChosenHost => FindChosenServer().Host;

        public int ChosenPort => FindChosenServer().Port;

        public static EmailSettings Read(IConfiguration configuration)
        {
            EmailSettings? settings = configuration.GetSection(SectionName).Get<EmailSettings>();
            if ((settings is null) || !settings.IsComplete())
            {
                throw new InvalidOperationException(InvalidEmailSettingsMessage);
            }

            return settings;
        }

        public EmailSender CreateSender()
        {
            MailCredentials? credentials = UsesRealServer ? ReadCredentials() : null;
            EmailServerSetting chosenServer = FindChosenServer();
            var mailServer = new MailServer(chosenServer.Host, chosenServer.Port, chosenServer.UsesStartTls, chosenServer.ChecksCertificateRevocation, credentials);
            var mailSender = new MailSender(SenderAddress, SenderName);
            var emailSender = new EmailSender(mailServer, mailSender, TimeSpan.FromSeconds(SendTimeoutSeconds));

            return emailSender;
        }

        private bool IsComplete()
        {
            bool hasSender = !string.IsNullOrWhiteSpace(SenderAddress) && !string.IsNullOrWhiteSpace(SenderName) && (SendTimeoutSeconds > 0);
            bool hasServer = !string.IsNullOrWhiteSpace(ChosenHost) && (ChosenPort > 0);
            bool isComplete = hasSender && hasServer;

            return isComplete;
        }

        private EmailServerSetting FindChosenServer()
        {
            EmailServerSetting chosenServer = UsesRealServer ? Real : Local;

            return chosenServer;
        }

        private static MailCredentials ReadCredentials()
        {
            string? userName = Environment.GetEnvironmentVariable(SmtpUserVariable);
            string? password = Environment.GetEnvironmentVariable(SmtpPasswordVariable);
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                throw new InvalidOperationException(MissingCredentialsMessage);
            }

            var credentials = new MailCredentials(userName, password);

            return credentials;
        }
    }
}
