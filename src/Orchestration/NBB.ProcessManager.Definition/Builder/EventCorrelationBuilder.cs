// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace NBB.ProcessManager.Definition.Builder
{
    public class EventCorrelationBuilder<TEvent, TData>
    {
        private Func<TEvent, IDictionary<string, string>, object> _correlationFilter;

        public EventCorrelationBuilder<TEvent, TData> CorrelateById(Func<TEvent, Guid> selector)
        {
            _correlationFilter = (@event, headers) => selector(@event);
            return this;
        }

        public EventCorrelationBuilder<TEvent, TData> CorrelateById(Func<TEvent, IDictionary<string, string>, Guid> selector)
        {
            _correlationFilter = (@event, headers) => selector(@event, headers);
            return this;
        }

        public EventCorrelationBuilder<TEvent, TData> CorrelateById<T>(Func<TEvent, T> selector) where T : struct
        {
            _correlationFilter = (@event, headers) => selector(@event);
            return this;
        }

        public EventCorrelationBuilder<TEvent, TData> CorrelateBy<T>(Func<TEvent, IDictionary<string, string>, T> selector) where T : class
        {
            _correlationFilter = selector;
            return this;
        }

        public EventCorrelation<TEvent, TData> Build()
        {
            return new EventCorrelation<TEvent, TData>(_correlationFilter);
        }
    }
}
