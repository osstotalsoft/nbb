// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.ProcessManager.Definition.Builder
{
    public class EventCorrelationBuilder<TEvent, TData>
    {
        private Func<TEvent, object> _correlationFilter;

        public EventCorrelationBuilder<TEvent, TData> CorrelateById(Func<TEvent, Guid> selector)
        {
            _correlationFilter = @event => selector(@event);
            return this;
        }

        public EventCorrelationBuilder<TEvent, TData> CorrelateById<T>(Func<TEvent, T> selector) where T : struct
        {
            _correlationFilter = @event => selector(@event);
            return this;
        }

        public EventCorrelationBuilder<TEvent, TData> CorrelateBy<T>(Func<TEvent, T> selector) where T : class
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