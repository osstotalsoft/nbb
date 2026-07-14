// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Riok.Mapperly.Abstractions;
using ProcessManagerSample.Events;

namespace ProcessManagerSample
{
    [Mapper]
    public partial class OrderMapper
    {
        public partial OrderCompleted ToCompleted(OrderCreated src);

        // OrderShipped has no Amount; keep the original behavior of mapping it to 0.
        public OrderCompleted ToCompleted(OrderShipped src)
            => new(src.OrderId, 0, src.DocumentId, src.SiteId);
    }
}
