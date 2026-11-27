using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using log4net;

using MailKit;
using MailKit.Net.Smtp;
using MailKit.Security;

using MimeKit;
using MimeKit.Text;

namespace Game.Services
{
    public sealed class EmailSender : IEmailSender
    {
        private const string EmailDeliveredMessage = "An email was handed to the mail server.";
        private const string DeliveryFailedMessage = "An email could not be handed to the mail server.";

        private static readonly ILog _log = LogManager.GetLogger(typeof(EmailSender));

        private readonly MailServer _mailServer;
        private readonly MailSender _mailSender;
        private readonly TimeSpan _sendTimeout;

        public EmailSender(MailServer mailServer, MailSender mailSender, TimeSpan sendTimeout)
        {
            _mailServer = mailServer;
            _mailSender = mailSender;
            _sendTimeout = sendTimeout;
        }

        public Task<bool> SendAsync(EmailMessage message)
        {
            Task<bool> sending = DeliverOrReportAsync(message);

            return sending;
        }

        private async Task<bool> DeliverOrReportAsync(EmailMessage message)
        {
            bool wasDelivered = false;
            try
            {
                await DeliverAsync(message);
                wasDelivered = true;
                _log.Info(EmailDeliveredMessage);
            }
            catch (CommandException exception)
            {
                ReportFailure(exception);
            }
            catch (ProtocolException exception)
            {
                ReportFailure(exception);
            }
            catch (AuthenticationException exception)
            {
                ReportFailure(exception);
            }
            catch (SslHandshakeException exception)
            {
                ReportFailure(exception);
            }
            catch (IOException exception)
            {
                ReportFailure(exception);
            }
            catch (SocketException exception)
            {
                ReportFailure(exception);
            }
            catch (OperationCanceledException exception)
            {
                ReportFailure(exception);
            }
            catch (FormatException exception)
            {
                ReportFailure(exception);
            }

            return wasDelivered;
        }

        private async Task DeliverAsync(EmailMessage message)
        {
            using MimeMessage mimeMessage = ComposeMimeMessage(message);
            using var deliveryDeadline = new CancellationTokenSource(_sendTimeout);
            using var smtpClient = new SmtpClient();
            SecureSocketOptions socketOptions = _mailServer.UsesStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await smtpClient.ConnectAsync(_mailServer.Host, _mailServer.Port, socketOptions, deliveryDeadline.Token);
            if (_mailServer.Credentials is not null)
            {
                await smtpClient.AuthenticateAsync(_mailServer.Credentials.UserName, _mailServer.Credentials.Password, deliveryDeadline.Token);
            }

            await smtpClient.SendAsync(mimeMessage, deliveryDeadline.Token);
            await smtpClient.DisconnectAsync(true, deliveryDeadline.Token);
        }

        private MimeMessage ComposeMimeMessage(EmailMessage message)
        {
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(_mailSender.DisplayName, _mailSender.Address));
            mimeMessage.To.Add(MailboxAddress.Parse(message.RecipientAddress));
            mimeMessage.Subject = message.Subject;
            mimeMessage.Body = new TextPart(TextFormat.Plain) { Text = message.Body };

            return mimeMessage;
        }

        private static void ReportFailure(Exception exception)
        {
            _log.Error(DeliveryFailedMessage, exception);
        }
    }
}
