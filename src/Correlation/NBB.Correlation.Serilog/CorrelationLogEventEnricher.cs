using Serilog.Core;
using Serilog.Events;

namespace NBB.Correlation.Serilog
{
    public class CorrelationLogEventEnricher : ILogEventEnricher
    {

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", CorrelationManager.GetCorrelationId()));
        }
    }
}
