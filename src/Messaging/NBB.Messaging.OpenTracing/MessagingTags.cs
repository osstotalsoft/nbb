// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Messaging.OpenTracing
{
    public static class MessagingTags
    {
        public const string ComponentMessaging = "NBB.Messaging";
        public const string CorrelationId = "nbb.correlation_id";
        public const string MessagingEnvelopeHeaderSpanTagPrefix = "messaging_header.";
    }
}
