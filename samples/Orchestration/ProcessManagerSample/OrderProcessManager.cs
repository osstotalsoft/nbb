using AutoMapper;
using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Definition.Builder;
using ProcessManagerSample.Commands;
using ProcessManagerSample.Events;
using ProcessManagerSample.Queries;
using System;
using System.Collections.Generic;
using NBB.Core.Effects;

using NBB.Application.MediatR.Effects;
using NBB.Messaging.Effects;

namespace ProcessManagerSample
{
    public class OrderProcessManager : AbstractDefinition<OrderProcessManagerData>
    {
        private readonly IMapper _mapper;

        public OrderProcessManager(IMapper mapper)
        {
            _mapper = mapper;

            Event<OrderCreated>(builder => builder.CorrelateById(orderCreated => orderCreated.OrderId));
            Event<OrderPaymentCreated>(builder => builder.CorrelateById(paymentReceived => paymentReceived.OrderId));
            Event<OrderShipped>(builder => builder.CorrelateById(orderShipped => orderShipped.OrderId));
            Event<OrderPaymentExpired>(builder => builder.CorrelateById(orderShipped => orderShipped.OrderId));

            StartWith<OrderCreated>()
                .PublishEvent((orderCreated, data) => _mapper.Map<OrderCompleted>(orderCreated))
                .Then(OrderCreatedHandler);

            When<OrderPaymentCreated>()
                .SetState((received, state) => state.Data with {OrderId = Guid.NewGuid()})
                .Then((orderCreated, data) =>
                {
                    var q1 = Mediator.SendQuery(new GetClientQuery());
                    var q2 = Effect.Parallel(Mediator.SendQuery(new GetPartnerQuery()), Mediator.SendQuery(new GetClientQuery()));

                    var queries =
                        from x in q1
                        from y in q2
                        select new List<string> { x.ClientCode, y.Item1.PartnerCode, y.Item2.ClientCode };

                    return queries
                        .Then(partners => MessageBus.Publish(new DoPayment()))
                        .Then(partner => MessageBus.Publish(new DoPayment()));
                })
                .RequestTimeout(TimeSpan.FromSeconds(10), (created, data) => new OrderPaymentExpired(Guid.Empty, 0, 0));

            When<OrderPaymentExpired>()
                .Complete();

            When<OrderShipped>((@event, data) => !data.Data.IsPaid)
                .SendCommand(OrderShippedHandler)
                .PublishEvent((orderShipped, data) => _mapper.Map<OrderCompleted>(orderShipped))
                .Complete();
        }

        private static Effect<Unit> OrderCreatedHandler(OrderCreated orderCreated, InstanceData<OrderProcessManagerData> state)
        {
            return MessageBus.Publish(new DoPayment());
        }

        private static DoPayment OrderPaymentCreatedHandler(OrderPaymentCreated orderPaymentReceived, InstanceData<OrderProcessManagerData> state)
        {
            return new DoPayment();
        }

        private static DoPayment OrderShippedHandler(OrderShipped orderShipped, InstanceData<OrderProcessManagerData> state)
        {
            return new DoPayment();
        }
    }
}