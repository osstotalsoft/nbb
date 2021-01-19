using System;
using System.Collections.Generic;

namespace NBB.Messaging.Abstractions
{
    public static class MessagingEnvelopeExtensions
    {
        public static string GetMessageTypeId(this MessagingEnvelope envelope)
        {
            return envelope.Headers.TryGetValue(MessagingHeaders.MessageType, out var value) 
                ? value 
                : null;
        }

        public static Guid? GetCorrelationId(this MessagingEnvelope envelope)
        {
            return envelope.Headers.TryGetValue(MessagingHeaders.CorrelationId, out var value)
                ? Guid.TryParse(value, out var guidValue)
                    ? guidValue
                    : default(Guid?)
                : default(Guid?);
        }

        public static void TransferHeaderTo(this MessagingEnvelope envelope, MessagingEnvelope destinationEnvelope, string header, bool overwrite = false)
        {
            if (!envelope.Headers.ContainsKey(header))
                return;

            destinationEnvelope.SetHeader(header, envelope.Headers[header], overwrite);
        }

        public static void TransferCustomHeadersTo(this MessagingEnvelope envelope, MessagingEnvelope destinationEnvelope, bool overwrite = false)
        {
            foreach (KeyValuePair<string, string> header in envelope.Headers)
            {
                if (header.Key.StartsWith("nbb-"))
                    continue;

                envelope.TransferHeaderTo(destinationEnvelope, header.Key, overwrite);
            }
        }

        public static void SetHeader(this MessagingEnvelope envelope, string header, string value, bool overwrite = false)
        {
            if (!overwrite && envelope.Headers.ContainsKey(header) && !string.IsNullOrEmpty(envelope.Headers[header]))
                return;

            envelope.Headers[header] = value;

        }
    }

    public static class MessagingHeaders
    {
        public const string MessageType = "nbb-messageType";
        public const string Source = "nbb-source";
        public const string CorrelationId = "nbb-correlationId";
        public const string MessageId = "nbb-messageId";
        public const string PublishTime = "nbb-publishTime";
        public const string StreamId = "nbb-streamId";
    }
}
