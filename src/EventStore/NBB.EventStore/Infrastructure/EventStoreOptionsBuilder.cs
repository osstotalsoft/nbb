// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.EventStore.Infrastructure
{
    public class EventStoreOptionsBuilder
    {
        public EventStoreOptions Options { get; }

        public EventStoreOptionsBuilder()
        {
            Options = new EventStoreOptions();
        }

    }
}
