using System.Collections.Generic;
using System.Linq;
using Serilog.Events;

namespace NBB.Tools.Serilog.OpenTracingSink.Internal
{
    internal static class OpenTracingContribFilter
    {
        private static List<string> ExcludedLogSources = new List<string>()
        {
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore.Hosting"
        };


        public static bool ShouldExclude(LogEvent logEvent)
        {
            const string sourceContextPropertyName = global::Serilog.Core.Constants.SourceContextPropertyName;

            if (!logEvent.Properties.TryGetValue(sourceContextPropertyName, out var source) ||
                !(source is ScalarValue scalarSource) ||
                !(scalarSource.Value is string stringValue))
            {
                return false;
            }

            return ExcludedLogSources.Any(x => stringValue.StartsWith(x));
        }
    }
}
