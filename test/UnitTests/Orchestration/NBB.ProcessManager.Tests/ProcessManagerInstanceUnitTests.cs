using FluentAssertions;
using NBB.ProcessManager.Definition.Builder;
using NBB.ProcessManager.Definition.Effects;
using NBB.ProcessManager.Runtime;
using NBB.ProcessManager.Tests.Commands;
using NBB.ProcessManager.Tests.Events;
using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.ProcessManager.Definition;
using Xunit;

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

            var instance = new Instance<OrderProcessManagerData>(definition);
            var identitySelector = ((IDefinition<OrderProcessManagerData>)definition).GetCorrelationFilter<OrderCreated>();
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

            var instance = new Instance<OrderProcessManagerData>(definition);
            Action act = () => instance.ProcessEvent(@event);
            act.Should().Throw<Exception>();
        }

        class OrderProcessManagerNoCorrelation : AbstractDefinition<OrderProcessManagerData>
        {
            public OrderProcessManagerNoCorrelation()
            {
                StartWith<OrderCreated>()
                    .SetState((orderCreated, state) =>
                    {
                        var newState = state.Data;
                        newState.Amount = 100;
                        return newState;
                    })
                    .PublishEvent((orderCreated, state) => new OrderCompleted(100, orderCreated.OrderId, 0, 0));
            }
        }


        [Fact]
        public void setStateHandler_on_first_event()
        {
            var @event = new OrderCreated(Guid.NewGuid(), 100, 0, 0);
            var definition = new OrderProcessManager2();

            var instance = new Instance<OrderProcessManagerData>(definition);
            instance.ProcessEvent(@event);
            instance.InstanceData.Data.Amount.Should().Be(200);
        }

        class OrderProcessManager2 : AbstractDefinition<OrderProcessManagerData>
        {
            public OrderProcessManager2()
            {
                Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => Guid.NewGuid()));

                StartWith<OrderCreated>()
                    .SetState((orderCreated, state) =>
                    {
                        var newState = state.Data;
                        newState.Amount = 100;
                        return newState;
                    })
                    .PublishEvent((orderCreated, state) => new OrderCompleted(100, orderCreated.OrderId, 0, 0))
                    .SetState((orderCreated, state) =>
                    {
                        var newState = state.Data;
                        newState.Amount = state.Data.Amount + 100;
                        return newState;
                    });
            }
        }

        [Fact]
        public void setStateHandler_with_two_events()
        {
            var orderId = Guid.NewGuid();
            var orderCreated = new OrderCreated(orderId, 100, 0, 0);
            var definition = new OrderProcessManager3();

            var instance = new Instance<OrderProcessManagerData>(definition);
            instance.ProcessEvent(orderCreated);
            instance.InstanceData.Data.Amount.Should().Be(200);

            var orderPaymentCreated = new OrderPaymentCreated(100, orderId, 0, 0);
            instance.ProcessEvent(orderPaymentCreated);
            instance.InstanceData.Data.Amount.Should().Be(200);
            instance.InstanceData.Data.IsPaid.Should().BeTrue();
        }

        class OrderProcessManager3 : AbstractDefinition<OrderProcessManagerData>
        {
            public OrderProcessManager3()
            {
                Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));
                Event<OrderPaymentCreated>(configurator => configurator.CorrelateById(paymentReceived => paymentReceived.OrderId));

                StartWith<OrderCreated>()
                    .SetState((orderCreated, state) =>
                    {
                        var newState = state.Data;
                        newState.Amount = 100;
                        newState.OrderId = orderCreated.OrderId;
                        return newState;
                    })
                    .PublishEvent((orderCreated, state) => new OrderCompleted(100, orderCreated.OrderId, 0, 0))
                    .SetState((orderCreated, state) =>
                    {
                        var newState = state.Data;
                        newState.Amount = state.Data.Amount + 100;
                        return newState;
                    });

                When<OrderPaymentCreated>()
                    .SetState((@event, data) =>
                    {
                        var newState = data.Data;
                        newState.IsPaid = true;
                        return newState;
                    })
                    .Then((ev, state) =>
                    {
                        var effect = new SendCommand(new DoPayment());
                        return effect;
                    });
            }
        }

        [Fact]
        public void should_not_start_when_event_with_false_predicate()
        {
            var orderId = Guid.NewGuid();
            var orderCreated = new OrderCreated(orderId, 100, 0, 0);
            var definition = new OrderProcessManager4();

            var instance = new Instance<OrderProcessManagerData>(definition);
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
        public void complete_with_false_predicate()
        {
            var orderId = Guid.NewGuid();
            var orderCreated = new OrderCreated(orderId, 100, 0, 0);
            var orderPaymentCreated = new OrderPaymentCreated(100, orderId, 0, 0);
            var definition = new OrderProcessManager5();

            var instance = new Instance<OrderProcessManagerData>(definition);
            instance.ProcessEvent(orderCreated);
            instance.State.Should().Be(InstanceStates.Started);
            instance.InstanceData.Data.IsPaid.Should().Be(true);
            instance.ProcessEvent(orderPaymentCreated);
            instance.State.Should().Be(InstanceStates.Started);
            instance.InstanceData.Data.IsPaid.Should().Be(false);
            instance.InstanceData.Data.Amount.Should().Be(110);
        }


        class OrderProcessManager5 : AbstractDefinition<OrderProcessManagerData>
        {
            public OrderProcessManager5()
            {
                Event<OrderCreated>(configurator => configurator.CorrelateById(orderCreated => orderCreated.OrderId));
                Event<OrderPaymentCreated>(configurator => configurator.CorrelateById(orderPaymentCreated => orderPaymentCreated.OrderId));

                StartWith<OrderCreated>()
                    .SetState((@event, data) =>
                    {
                        var newState = data.Data;
                        newState.IsPaid = true;
                        return newState;
                    });

                When<OrderPaymentCreated>((@event, data) => true)
                    .SetState((@event, data) =>
                    {
                        var newState = data.Data;
                        newState.Amount = 100;
                        return newState;
                    });

                When<OrderPaymentCreated>((@event, data) => data.Data.Amount < 100)
                    .SetState((@event, data) =>
                    {
                        var newState = data.Data;
                        newState.Amount += 20;
                        newState.IsPaid = false;
                        return newState;
                    })
                    .Complete((@event, data) => data.Data.IsPaid);

                When<OrderPaymentCreated>((@event, data) => data.Data.Amount >= 100)
                    .SetState((@event, data) =>
                    {
                        var newState = data.Data;
                        newState.Amount += 10;
                        newState.IsPaid = false;
                        return newState;
                    })
                    .Complete((@event, data) => data.Data.IsPaid);
            }
        }


        [Fact]
        public void process_event_after_completion()
        {
            var orderId = Guid.NewGuid();
            var orderCreated = new OrderCreated(orderId, 100, 0, 0);
            var orderCompleted = new OrderCompleted(100, orderId, 0, 0);
            var orderPaymentCreated = new OrderPaymentCreated(100, orderId, 0, 0);
            var definition = new OrderProcessManager6();

            var instance = new Instance<OrderProcessManagerData>(definition);
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
                Event<OrderCompleted>(configurator => configurator.CorrelateById(orderCompleted => orderCompleted.OrderId));
                Event<OrderPaymentCreated>(configurator => configurator.CorrelateById(orderPaymentCreated => orderPaymentCreated.OrderId));

                StartWith<OrderCreated>()
                    .SetState((@event, data) =>
                    {
                        var newState = data.Data;
                        newState.OrderId = @event.OrderId;
                        return newState;
                    });

                When<OrderCompleted>()
                    .Complete();

                When<OrderPaymentCreated>()
                    .SetState((@event, data) =>
                    {
                        var newState = data.Data;
                        newState.IsPaid = true;
                        return newState;
                    })
                    .Complete();

            }
        }

        [Fact]
        public void when_predicate_false_should_not_complete()
        {
            var orderId = Guid.NewGuid();
            var orderCreated = new OrderCreated(orderId, 100, 0, 0);
            var orderShipped = new OrderShipped(orderId, DateTime.Parse("2019-09-09"));
            var definition = new OrderProcessManager7();

            var instance = new Instance<OrderProcessManagerData>(definition);
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
    }

    public struct OrderProcessManagerData
    {
        public Guid OrderId { get; set; }
        public int SiteId { get; set; }
        public int DocumentId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public bool IsPaid { get; set; }
        public bool IsShipped { get; set; }
    }
}