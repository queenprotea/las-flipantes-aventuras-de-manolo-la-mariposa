using System;

namespace Game.Services
{
    public sealed class EmailMessage
    {
        public EmailMessage(string recipientAddress, string subject, string body)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(recipientAddress);
            ArgumentException.ThrowIfNullOrWhiteSpace(subject);
            ArgumentException.ThrowIfNullOrWhiteSpace(body);

            RecipientAddress = recipientAddress;
            Subject = subject;
            Body = body;
        }

        public string RecipientAddress { get; }

        public string Subject { get; }

        public string Body { get; }
    }
}
