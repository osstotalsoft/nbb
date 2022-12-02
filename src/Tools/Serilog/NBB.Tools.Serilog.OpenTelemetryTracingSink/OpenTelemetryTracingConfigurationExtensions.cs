// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Tools.Serilog.OpenTelemetryTracingSink.Internal;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;

namespace NBB.Tools.Serilog.OpenTelemetryTracingSink
{
    public static class OpenTelemetryTracingConfigurationExtensions
    {
        public static LoggerConfiguration OpenTelemetryTracing(
            this LoggerSinkConfiguration sinkConfiguration,
            LogEventLevel restrictedToMinimumLevel = LogEventLevel.Warning,
            LoggingLevelSwitch levelSwitch = null,
            bool exludeOpenTelemetryContribEvents = true
        )
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));

            var sink = exludeOpenTelemetryContribEvents ?
                new Internal.OpenTelemetryTracingSink(OpenTelemetryTracingContribFilter.ShouldExclude) :
                new Internal.OpenTelemetryTracingSink();

            return sinkConfiguration.Sink(sink, restrictedToMinimumLevel, levelSwitch);
        }
    }
}
