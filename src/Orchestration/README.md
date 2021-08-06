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
 public struct OrderState
 {
    public int OrderNumber { set; get; }
 }
 
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
