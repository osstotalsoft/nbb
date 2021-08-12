// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Serilog.Core;
using Serilog.Events;

namespace NBB.Correlation.Serilog
{
    public class CorrelationLogEventEnricher : ILogEventEnricher
    {

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddOrUpdateProperty(propertyFactory.CreateProperty("CorrelationId", CorrelationManager.GetCorrelationId()));
        }
    }
}
