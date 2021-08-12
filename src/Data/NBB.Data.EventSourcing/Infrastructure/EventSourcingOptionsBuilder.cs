// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Data.EventSourcing.Infrastructure
{
    public class EventSourcingOptionsBuilder
    {
        public EventSourcingOptions Options { get; }

        public EventSourcingOptionsBuilder()
        {
            Options = new EventSourcingOptions();
        }
    }
}
