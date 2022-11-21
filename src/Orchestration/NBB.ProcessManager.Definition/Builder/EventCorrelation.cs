// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

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
