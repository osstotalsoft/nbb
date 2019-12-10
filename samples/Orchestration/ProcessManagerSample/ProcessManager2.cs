using NBB.Core.Effects;
using NBB.ProcessManager.Definition.Builder;
using ProcessManagerSample.Events;

namespace ProcessManagerSample
{
    public class ProcessManager2 : AbstractDefinition<ProcessManager2Data>
    {
        public ProcessManager2()
        {

        }

    }

    public struct ProcessManager2Data
    {
    }


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