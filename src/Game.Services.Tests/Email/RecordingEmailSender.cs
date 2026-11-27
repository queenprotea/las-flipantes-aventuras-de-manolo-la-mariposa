using System.Collections.Generic;
using System.Threading.Tasks;

namespace Game.Services.Tests
{
    internal sealed class RecordingEmailSender : IEmailSender
    {
        internal bool IsDelivering { get; set; } = true;

        internal List<EmailMessage> SentMessages { get; } = new List<EmailMessage>();

        public Task<bool> SendAsync(EmailMessage message)
        {
            if (IsDelivering)
            {
                SentMessages.Add(message);
            }

            Task<bool> sending = Task.FromResult(IsDelivering);

            return sending;
        }
    }
}
