using NBB.Tools.Serilog.OpenTracingSink.Internal;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;
using System;

namespace NBB.Tools.Serilog.OpenTracingSink
{
    public static class OpenTracingConfigurationExtensions
    {
        public static LoggerConfiguration OpenTracing(
            this LoggerSinkConfiguration sinkConfiguration,
            LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
            LoggingLevelSwitch levelSwitch = null,
            bool exludeOpenTracingContribEvents = true
        )
        {
            if (sinkConfiguration == null) throw new ArgumentNullException(nameof(sinkConfiguration));

            var sink = exludeOpenTracingContribEvents ? 
                new Internal.OpenTracingSink(OpenTracingContribFilter.ShouldExclude) : 
                new Internal.OpenTracingSink();

            return sinkConfiguration.Sink(sink, restrictedToMinimumLevel, levelSwitch);
        }
    }
}
