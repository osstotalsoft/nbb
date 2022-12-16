// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using System.Linq;
using Serilog.Events;

namespace NBB.Tools.Serilog.OpenTelemetryTracingSink.Internal
{
    internal static class OpenTelemetryTracingContribFilter
    {
        private static List<string> ExcludedLogSources = new()
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore.Hosting"
        };


        public static bool ShouldExclude(LogEvent logEvent)
        {
            const string sourceContextPropertyName = global::Serilog.Core.Constants.SourceContextPropertyName;

            if (!logEvent.Properties.TryGetValue(sourceContextPropertyName, out var source) ||
                source is not ScalarValue scalarSource ||
                scalarSource.Value is not string stringValue)
            {
                return false;
            }

            return ExcludedLogSources.Any(x => stringValue.StartsWith(x));
        }
    }
}
