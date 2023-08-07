// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using NBB.ProcessManager.Definition.Builder;
using NBB.ProcessManager.Runtime;
using NBB.ProcessManager.Tests.Commands;
using NBB.ProcessManager.Tests.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Messaging.Effects;
using NBB.ProcessManager.Definition;
using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using NBB.ProcessManager.Runtime.Events;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Effects;
using NBB.EventStore.InMemory;
using NBB.EventStore.Internal;
using NBB.ProcessManager.Runtime.Persistence;
using static NBB.ProcessManager.Tests.RegistrationTests.RegistrationProcessManager;
using System.Collections.Generic;
using System.Linq;
using MediatR;
using FluentAssertions.Common;


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
                        var effect = MessageBus.Publish(new DoPayment());
                        return effect;
                    });
            }
        }
    }
}
