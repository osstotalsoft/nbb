// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Data.EventSourcing.Infrastructure
{
    public class EventSourcingOptions
    {
        public int DefaultSnapshotVersionFrequency { get; set; } = 10;
    }
}
