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

namespace NBB.ProcessManager.Tests
{
    public class ProcessManagerInstanceUnitTests : IClassFixture<InstanceDataRepositoryFixture>
    {
        private readonly InstanceDataRepositoryFixture _fixture;

        public ProcessManagerInstanceUnitTests(InstanceDataRepositoryFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task TestRepo()
        {
            var @event = new OrderCreated(Guid.NewGuid(), 100, 0, 0);
            var definition = new OrderProcessManager3();
            var logger = Mock.Of<ILogger<Instance<OrderProcessManagerData>>>();

            var instance = new Instance<OrderProcessManagerData>(definition, logger);
            var identitySelector =
                ((IDefinition<OrderProcessManagerData>) definition).GetCorrelationFilter<OrderCreated>();
            if (identitySelector != null)
                instance = await _fixture.Repository.Get(definition, identitySelector(@event), CancellationToken.None);

            instance.ProcessEvent(@event);
            await _fixture.Repository.Save(instance, CancellationToken.None);
        }

        [Fact]
        public void Should_throw_for_missing_correlation()
        {
            var @event = new OrderCreated(Guid.NewGuid(), 100, 0, 0);
            var definition = new OrderProcessManagerNoCorrelation();
            var logger = Mock.Of<ILogger<Instance<OrderProcessManagerData>>>();

            var instance = new Instance<OrderProcessManagerData>(definition, logger);
            Action act = () => instance.ProcessEvent(@event);
            act.Should().Throw<Exception>();
        }

        class OrderProcessManagerNoCorrelation : AbstractDefinition<OrderProcessManagerData>
        {
            public OrderProcessManagerNoCorrelation()
            {
                StartWith<OrderCreated>()
                    .SetState((orderCreated, state) =>
                        state.Data with
                        {
                            Amount = 100
                        })
                    .PublishEvent((orderCreated, state) => new OrderCompleted(orderCreated.OrderId, 100, 0, 0));
            }
        }


        [Fact]
        public void SetStateHandler_on_first_event()
        {
            var @event = new OrderCreated(Guid.NewGuid(), 100, 0, 0);
            var definition = new OrderProcessManager2();
            var logger = Mock.Of<ILogger<Instance<OrderProcessManagerData>>>();

            var instance = new Instance<OrderProcessManagerData>(definition, logger);
            instance.ProcessEvent(@event);
            instance.Data.Amount.Should().Be(100);
        }

        class OrderProcessManager2 : AbstractDefinition<OrderProcessManagerData>
        {
            public OrderProcessManager2()
            {
                Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => Guid.NewGuid()));
                StartWith<OrderCreated>()
                    .SetState((orderCreated, state) => state.Data with { Amount = state.Data.Amount + 100 })
                    .PublishEvent((orderCreated, state) => new OrderCompleted(orderCreated.OrderId, 100, 0, 0));
            }
        }

        [Fact]
        public void SetStateHandler_with_two_events()
        {
            var orderId = Guid.NewGuid();
            var orderCreated = new OrderCreated(orderId, 100, 0, 0);
            var definition = new OrderProcessManager3();
            var logger = Mock.Of<ILogger<Instance<OrderProcessManagerData>>>();

            var instance = new Instance<OrderProcessManagerData>(definition, logger);
            instance.ProcessEvent(orderCreated);
            instance.Data.Amount.Should().Be(100);

            var orderPaymentCreated = new OrderPaymentCreated(orderId, 100, 0, 0);
            instance.ProcessEvent(orderPaymentCreated);
            instance.Data.Amount.Should().Be(100);
            instance.Data.IsPaid.Should().BeTrue();
        }

        class OrderProcessManager3 : AbstractDefinition<OrderProcessManagerData>
        {
            public OrderProcessManager3()
            {
                Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));
                Event<OrderPaymentCreated>(configurator =>
                    configurator.CorrelateById(paymentReceived => paymentReceived.OrderId));

                StartWith<OrderCreated>()
                    .SetState((orderCreated, state) =>
                        state.Data with
                        {
                            Amount = 100,
                            OrderId = orderCreated.OrderId
                        })
                    .PublishEvent((orderCreated, state) => new OrderCompleted(orderCreated.OrderId, 100, 0, 0));

                When<OrderPaymentCreated>()
                    .SetState((@event, state) => state.Data with {IsPaid = true})
                    .Then((ev, state) =>
                    {
                        var effect = MessageBus.Publish(new DoPayment());
                        return effect;
                    });
            }
        }

        [Fact]
        public void Should_not_start_when_event_with_false_predicate()
        {
            var orderId = Guid.NewGuid();
            var orderCreated = new OrderCreated(orderId, 100, 0, 0);
            var definition = new OrderProcessManager4();
            var logger = Mock.Of<ILogger<Instance<OrderProcessManagerData>>>();

            var instance = new Instance<OrderProcessManagerData>(definition, logger);
            instance.ProcessEvent(orderCreated);
            instance.State.Should().Be(InstanceStates.NotStarted);
        }


        class OrderProcessManager4 : AbstractDefinition<OrderProcessManagerData>
        {
            public OrderProcessManager4()
            {
                Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));
                StartWith<OrderCreated>((@event, data) => false);
            }
        }

        [Fact]
        public void Complete_with_false_predicate()
        {
            var orderId = Guid.NewGuid();
            var orderCreated = new OrderCreated(orderId, 100, 0, 0);
            var orderPaymentCreated = new OrderPaymentCreated(orderId, 100, 0, 0);
            var definition = new OrderProcessManager5();
            var logger = Mock.Of<ILogger<Instance<OrderProcessManagerData>>>();

            var instance = new Instance<OrderProcessManagerData>(definition, logger);
            instance.ProcessEvent(orderCreated);
            instance.State.Should().Be(InstanceStates.Started);
            instance.Data.IsPaid.Should().Be(true);
            instance.ProcessEvent(orderPaymentCreated);
            instance.State.Should().Be(InstanceStates.Started);
            instance.Data.IsPaid.Should().Be(false);
            instance.Data.Amount.Should().Be(110);
        }


        class OrderProcessManager5 : AbstractDefinition<OrderProcessManagerData>
        {
            public OrderProcessManager5()
            {
                Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));
                Event<OrderPaymentCreated>(configurator =>
                    configurator.CorrelateById(orderPaymentCreated => orderPaymentCreated.OrderId));

                StartWith<OrderCreated>()
                    .SetState((@event, data) => data.Data with {IsPaid = true});

                When<OrderPaymentCreated>((@event, data) => true)
                    .SetState((@event, data) => data.Data with {Amount = 100});

                When<OrderPaymentCreated>((@event, data) => data.Data.Amount < 100)
                    .SetState((@event, data) =>
                        data.Data with
                        {
                            Amount = data.Data.Amount + 20,
                            IsPaid = false
                        })
                    .Complete((@event, data) => data.Data.IsPaid);

                When<OrderPaymentCreated>((@event, data) => data.Data.Amount >= 100)
                    .SetState((@event, data) =>
                        data.Data with
                        {
                            Amount = data.Data.Amount + 10,
                            IsPaid = false
                        })
                    .Complete((@event, data) => data.Data.IsPaid);
            }
        }


        [Fact]
        public void Process_event_after_completion()
        {
            var orderId = Guid.NewGuid();
            var orderCreated = new OrderCreated(orderId, 100, 0, 0);
            var orderCompleted = new OrderCompleted(orderId, 100, 0, 0);
            var orderPaymentCreated = new OrderPaymentCreated(orderId, 100, 0, 0);
            var definition = new OrderProcessManager6();
            var logger = Mock.Of<ILogger<Instance<OrderProcessManagerData>>>();

            var instance = new Instance<OrderProcessManagerData>(definition, logger);
            instance.ProcessEvent(orderCreated);
            instance.ProcessEvent(orderCompleted);
            instance.State.Should().Be(InstanceStates.Completed);

            Action act = () => instance.ProcessEvent(orderPaymentCreated);
            act.Should().Throw<Exception>();
        }


        class OrderProcessManager6 : AbstractDefinition<OrderProcessManagerData>
        {
            public OrderProcessManager6()
            {
                Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));
                Event<OrderCompleted>(configurator =>
                    configurator.CorrelateById(orderCompleted => orderCompleted.OrderId));
                Event<OrderPaymentCreated>(configurator =>
                    configurator.CorrelateById(orderPaymentCreated => orderPaymentCreated.OrderId));

                StartWith<OrderCreated>()
                    .SetState((@event, data) => data.Data with {OrderId = @event.OrderId});

                When<OrderCompleted>()
                    .Complete();

                When<OrderPaymentCreated>()
                    .SetState((@event, data) => data.Data with {IsPaid = true})
                    .Complete();
            }
        }

        [Fact]
        public void When_predicate_false_should_not_complete()
        {
            var orderId = Guid.NewGuid();
            var orderCreated = new OrderCreated(orderId, 100, 0, 0);
            var orderShipped = new OrderShipped(orderId, DateTime.Parse("2019-09-09"));
            var definition = new OrderProcessManager7();
            var logger = Mock.Of<ILogger<Instance<OrderProcessManagerData>>>();

            var instance = new Instance<OrderProcessManagerData>(definition, logger);
            instance.ProcessEvent(orderCreated);
            instance.ProcessEvent(orderShipped);
            instance.State.Should().Be(InstanceStates.Started);
        }


        class OrderProcessManager7 : AbstractDefinition<OrderProcessManagerData>
        {
            public OrderProcessManager7()
            {
                Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));
                Event<OrderShipped>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));

                StartWith<OrderCreated>()
                    .SendCommand((created, data) => new ShipOrder(created.OrderId, created.Amount, "bucuresti"));

                When<OrderShipped>((@event, data) => @event.ShippingDate < DateTime.Parse("2019-09-08"))
                    .Complete();

                When<OrderShipped>()
                    .Complete((@event, data) => @event.ShippingDate < DateTime.Parse("2019-09-08"));
            }
        }

        [Fact]
        public void Should_throw_when_starting_obsolete_processes()
        {
            var @event = new OrderCreated(Guid.NewGuid(), 100, 0, 0);
            var definition = new ObsoleteProcessManager();
            var logger = Mock.Of<ILogger<Instance<OrderProcessManagerData>>>();

            var instance = new Instance<OrderProcessManagerData>(definition, logger);
            Action act = () => instance.ProcessEvent(@event);
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Should_handle_non_start_events_for_obsolete_processes()
        {
            var startEvent = new OrderCreated(Guid.NewGuid(), 100, 0, 0);
            var @event = new OrderShipped(startEvent.OrderId, DateTime.Now);
            var definition = new ObsoleteProcessManager();
            var logger = Mock.Of<ILogger<Instance<OrderProcessManagerData>>>();

            var instance = new Instance<OrderProcessManagerData>(definition, logger);
            instance.LoadFromHistory(new[] { new ProcessStarted(@event.OrderId) });
            instance.ProcessEvent(@event);
            instance.MarkChangesAsCommitted();
            instance.Version.Should().Be(2);
        }

        [ObsoleteProcess]
        class ObsoleteProcessManager : AbstractDefinition<OrderProcessManagerData>
        {
            public ObsoleteProcessManager()
            {
                Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));
                Event<OrderShipped>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));

                StartWith<OrderCreated>()
                    .SendCommand((created, data) => new ShipOrder(created.OrderId, created.Amount, "bucuresti"));

                When<OrderShipped>((@event, data) => @event.ShippingDate < DateTime.Parse("2019-09-08"))
                    .Complete();

                When<OrderShipped>()
                    .Complete((@event, data) => @event.ShippingDate < DateTime.Parse("2019-09-08"));
            }
        }
    }

    public record struct OrderProcessManagerData
    {
        public Guid OrderId { get; init; }
        public int SiteId { get; init; }
        public int DocumentId { get; init; }
        public int UserId { get; init; }
        public decimal Amount { get; init; }
        public bool IsPaid { get; init; }
        public bool IsShipped { get; init; }
    }
}
