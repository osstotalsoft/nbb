// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Messaging.OpenTelemetry.Publisher;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NBB.Messaging.OpenTelemetry
{
    public static class MessagingActivitySource
    {
        private static readonly AssemblyName assemblyName = typeof(OpenTelemetryPublisherDecorator).Assembly.GetName();
        private static readonly ActivitySource activitySource = new(assemblyName.Name, assemblyName.Version.ToString());

        public static ActivitySource Current => activitySource;
    }
}
