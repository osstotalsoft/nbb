using System;

namespace NBB.ProcessManager.Definition.Builder
{
    public class EventCorrelation<TEvent, TData>
    {
        public Func<TEvent, object> CorrelationFilter { get; set; }

        public EventCorrelation(Func<TEvent, object> correlationFilter)
        {
            CorrelationFilter = correlationFilter;
        }
    }
}