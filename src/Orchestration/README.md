# process manager
we need two packages, one for building the definitions, using a fluent api, and the second one is the runtime, which runs those definitions

## NuGet install
```cmd
dotnet add package NBB.ProcessManager.Definition
dotnet add package NBB.ProcessManager.Runtime
```

## Creating a simple definition 
First we have to define the data struct in this case `OrderState` which is a container that will be persisted, then you have to specify how 
we correlate events that will come through your process manager, then you can buid using the api a flow of sending command and events or 
waiting for other events. 

```csharp
 public record struct OrderState(int OrderNumber);
 
 public class OrderProcessManager : AbstractDefinition<OrderState>
 {
     public OrderProcessManager()
     {
         Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));

         StartWith<OrderCreated>()
             .PublishEvent((orderCreated, data) => new OrderReceivedEvent())
             .SendCommand((orderCreated, data) => new DoPaymentCommand())
             .Complete();
      }
 }
```


## Waiting for an event or timeout  
there are cases where you send a command and want to wait for the pm to complete in a specific amount of time, let's say we want to wait 2 day for 
a payment to be processed, if the instance does not closes in a day, then a specific timeout event (`OrderPaymentExpired`) will be published 

```csharp
   When<OrderPaymentCreated>()
     .SendCommand((orderCreated, data) => ...)
     .Schedule((created, data) => new OrderPaymentExpired(), TimeSpan.FromDays(2));
```

## Upgrading process definitions
You must create a new version of a process definition if you need to perform changes such as:
* Changing the `State` type of the process
* Modifying the steps (add or delete steps)
* Changes to a step (such as state update logic)

 The old versions of the process definition must be preserved because otherwise the already running processes will fail to complete.
 Old versions should be marked with `[ObsoleteProcess]` attribute. This ensures that the definition will not be used for starting new processes.

 ```csharp
public class OrderProcessManager
{
    public class V2 : AbstractDefinition<V2.OrderProcessManagerData>
    {
        public record struct OrderProcessManagerData(Guid OrderId, bool IsPaid);

        public V2()
        {
            Event<OrderCreated>(builder => builder.CorrelateById(orderCreated => orderCreated.OrderId));

            StartWith<OrderCreated>()
                .PublishEvent((orderCreated, data) => new OrderReceivedEvent())
                .SetState((received, state) => state.Data with { OrderId = Guid.NewGuid() })
                .Complete();
        }
    }

    [ObsoleteProcess]
    public class V1 : AbstractDefinition<V1.OrderProcessManagerData>
    {
        public record struct OrderProcessManagerData(string OrderId);

        public V1(IMapper mapper)
        {
            Event<OrderCreated>(builder => builder.CorrelateById(orderCreated => orderCreated.OrderId));

            StartWith<OrderCreated>()
                .SetState((received, state) => state.Data with { OrderId = Guid.NewGuid().ToString() })
                .Complete();
        }
    }
}
 ```
