using AutoMapper;
using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Definition.Builder;
using NBB.ProcessManager.Definition.Effects;
using ProcessManagerSample.Commands;
using ProcessManagerSample.Events;
using ProcessManagerSample.Queries;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using MediatR;

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
                .SetState((received, state) =>
                {
                    var newState = state.Data;
                    newState.OrderId = Guid.NewGuid();
                    return newState;
                })
                .Then((orderCreated, data) =>
                {
                    var a = EffectsFactory.Http<Partner>(new HttpRequestMessage());
                    var q1 = EffectsFactory.Query(new GetPartnerQuery());
                    var q2 = EffectsFactory.Query(new GetPartnerQuery());

                    var q3 = EffectsFactory.WhenAll(q1, q2);

                    var ab = 
                        from x in q1
                        from y in q2
                        select x.PartnerCode + x.PartnerName;



                    return q3.ContinueWith(partners => EffectsFactory.PublishMessage(new DoPayment()))
                        .ContinueWith(partner => EffectsFactory.PublishMessage(new DoPayment()));
                })
                .RequestTimeout(TimeSpan.FromSeconds(10), (created, data) => new OrderPaymentExpired(Guid.Empty, 0, 0));

            When<OrderPaymentExpired>()
                .Complete();

            When<OrderShipped>((@event, data) => !data.Data.IsPaid)
                .SendCommand(OrderShippedHandler)
                .PublishEvent((orderShipped, data) => _mapper.Map<OrderCompleted>(orderShipped))
                .Complete();
        }

        private static IEffect<Unit> OrderCreatedHandler(OrderCreated orderCreated, InstanceData<OrderProcessManagerData> state)
        {
            return EffectsFactory.PublishMessage(new DoPayment());
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