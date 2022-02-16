// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Effects;
using NBB.ProcessManager.Definition.Builder;
using ProcessManagerSample.Events;
using System;

namespace ProcessManagerSample
{
    public record struct OrderProcessManagerData(Guid OrderId, bool IsPaid);

    public class OrderProcessManager2 : AbstractDefinition<OrderProcessManagerData>
    {
        public OrderProcessManager2()
        {
            Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));

            StartWith<OrderCreated>()
                .Then((created, data) => Effect.Pure())
                .Complete();
        }
    }
}
