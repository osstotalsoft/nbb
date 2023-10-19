// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace NBB.ProcessManager.Definition.Builder
{
    public class EventCorrelation<TEvent, TData>
    {
        public Func<TEvent, IDictionary<string, string>, object> CorrelationFilter { get; set; }

        public EventCorrelation(Func<TEvent, IDictionary<string, string>, object> correlationFilter)
        {
            CorrelationFilter = correlationFilter;
        }
    }
}
