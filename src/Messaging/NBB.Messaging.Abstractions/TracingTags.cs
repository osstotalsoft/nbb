// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBB.Messaging.Abstractions
{
    public static class TracingTags
    {
        public const string ComponentMessaging = "NBB.Messaging";
        public const string CorrelationId = "nbb.correlation_id";
        public const string TenantId = "nbb.tenant_id";
        public const string MessagingEnvelopeHeaderSpanTagPrefix = "messaging_header.";
    }
}
