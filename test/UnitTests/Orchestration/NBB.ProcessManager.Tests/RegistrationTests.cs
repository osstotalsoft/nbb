// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBB.Messaging.Abstractions;
using NBB.ProcessManager.Definition.Builder;
using NBB.ProcessManager.Runtime;
using NBB.ProcessManager.Tests.Commands;
using NBB.ProcessManager.Tests.Events;
using System;
using System.Linq;
using Xunit;
using static NBB.ProcessManager.Tests.RegistrationTests.RegistrationProcessManager;

namespace NBB.ProcessManager.Tests
{
    public class RegistrationTests
    {

        [Fact]
        public void HandlersShouldBeRegisteredOnce()
        {
            var sp = BuildServiceProvider();
            var orderCreatedHandlers = sp.GetServices<INotificationHandler<OrderCreated>>().OfType<ProcessManagerNotificationHandler<RegistrationProcessManager, RegistrationProcessManagerData, OrderCreated>>();
            var orderPayemntCreatedHandlers = sp.GetServices<INotificationHandler<OrderPaymentCreated>>().OfType<ProcessManagerNotificationHandler<RegistrationProcessManager, RegistrationProcessManagerData, OrderPaymentCreated>>();

            orderCreatedHandlers.Count().Should().Be(1);
            orderPayemntCreatedHandlers.Count().Should().Be(1);
        }

        public IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton<MessagingContextAccessor>();
            services.AddProcessManager(typeof(RegistrationProcessManager).Assembly);
            services.AddEventStore(es =>
            {
                es.UseNewtownsoftJson();
                es.UseInMemoryEventRepository();
            });
            services.AddLogging(builder => builder.AddConsole());

            return services.BuildServiceProvider();
        }

        public class RegistrationProcessManager : AbstractDefinition<RegistrationProcessManagerData>
        {
            public record struct RegistrationProcessManagerData
            {
                public Guid OrderId { get; init; }
                public int CreateCount { get; init; }
                public int PaidCount { get; set;}
               
            }
            public RegistrationProcessManager()
            {
                Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));
                Event<OrderPaymentCreated>(configurator =>
                    configurator.CorrelateById(paymentReceived => paymentReceived.OrderId));

                StartWith<OrderCreated>();

                When<OrderCreated>()
                    .SetState((orderCreated, state) =>
                        state.Data with
                        {
                            OrderId = orderCreated.OrderId,
                            CreateCount = state.Data.CreateCount + 1
                        })
                    .PublishEvent((orderCreated, state) => new OrderCompleted(orderCreated.OrderId, 100, 0, 0));

                When<OrderPaymentCreated>()
                    .SetState((@event, state) => state.Data with { PaidCount = state.Data.PaidCount + 1 })
                    .Then((ev, state) =>
                    {
                        var effect = Messaging.Effects.MessageBus.Publish(new DoPayment());
                        return effect;
                    });
            }
        }
    }
}
