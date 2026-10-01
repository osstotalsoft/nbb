// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using NBB.Application.Mediator.Effects;
using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Definition.Builder;
using NBB.ProcessManager.Runtime.Persistence;
using Xunit;

namespace NBB.ProcessManager.Mediator.Tests
{
    public class ProcessManagerMediatorTests
    {
        [Fact]
        public async Task Should_run_the_process_manager_exactly_once_per_published_event()
        {
            //Arrange
            using var host = BuildHost(services => services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped));
            await host.StartAsync();
            var orderId = Guid.NewGuid();

            //Act
            using (var scope = host.Services.CreateScope())
            {
                var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
                await publisher.Publish(new OrderCreated(orderId));
                await publisher.Publish((object)new OrderPaid(orderId));
            }

            //Assert
            using (var scope = host.Services.CreateScope())
            {
                var definition = scope.ServiceProvider.GetRequiredService<IDefinition<OrderProcessData>>();
                var instance = await scope.ServiceProvider.GetRequiredService<IInstanceDataRepository>().Get(definition, orderId);
                instance.Data.CreatedCount.Should().Be(1);
                instance.Data.PaidCount.Should().Be(1);
            }

            var probe = host.Services.GetRequiredService<Probe>();
            probe.ShippedOrders.Should().Equal(orderId);
            await host.StopAsync();
        }

        [Fact]
        public async Task Should_fail_host_start_when_mediator_is_singleton()
        {
            //Arrange
            using var host = BuildHost(services => services.AddSingleton(Mock.Of<IMediator>()));

            //Act
            var act = () => host.StartAsync();

            //Assert
            await act.Should().ThrowAsync<OptionsValidationException>().WithMessage("*NBB.ProcessManager.Mediator requires Mediator.IMediator with the Scoped or Transient lifetime*");
        }

        [Fact]
        public async Task Should_fail_host_start_when_mediator_is_not_registered()
        {
            //Arrange
            using var host = BuildHost(_ => { });

            //Act
            var act = () => host.StartAsync();

            //Assert
            await act.Should().ThrowAsync<OptionsValidationException>().WithMessage("*NBB.ProcessManager.Mediator requires Mediator.IMediator to be registered*AddMediator*");
        }

        [Fact]
        public void Should_require_AddProcessManager()
        {
            var act = () => new ServiceCollection().AddProcessManagerMediatorHandlers();

            act.Should().Throw<InvalidOperationException>();
        }

        private static IHost BuildHost(Action<IServiceCollection> addMediator)
        {
            var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
            var services = builder.Services;
            services.AddSingleton<Probe>();
            addMediator(services);
            services
                .AddProcessManager(typeof(OrderProcess).Assembly)
                .AddProcessManagerMediatorHandlers();
            services.AddEventStore(es =>
            {
                es.UseNewtownsoftJson();
                es.UseInMemoryEventRepository();
            });

            return builder.Build();
        }
    }

    public class Probe
    {
        public System.Collections.Concurrent.ConcurrentQueue<Guid> ShippedOrders { get; } = new();
    }

    public record OrderCreated(Guid OrderId) : INotification;

    public record OrderPaid(Guid OrderId) : INotification;

    public record ShipOrder(Guid OrderId) : ICommand;

    public class ShipOrderHandler(Probe probe) : ICommandHandler<ShipOrder>
    {
        public ValueTask<Unit> Handle(ShipOrder command, CancellationToken cancellationToken)
        {
            probe.ShippedOrders.Enqueue(command.OrderId);
            return Unit.ValueTask;
        }
    }

    public record struct OrderProcessData
    {
        public Guid OrderId { get; init; }
        public int CreatedCount { get; init; }
        public int PaidCount { get; init; }
    }

    public class OrderProcess : AbstractDefinition<OrderProcessData>
    {
        public OrderProcess()
        {
            Event<OrderCreated>(configurator => configurator.CorrelateById(e => e.OrderId));
            Event<OrderPaid>(configurator => configurator.CorrelateById(e => e.OrderId));

            StartWith<OrderCreated>();

            When<OrderCreated>()
                .SetState((e, state) => state.Data with { OrderId = e.OrderId, CreatedCount = state.Data.CreatedCount + 1 });

            When<OrderPaid>()
                .SetState((e, state) => state.Data with { PaidCount = state.Data.PaidCount + 1 })
                .Then((e, state) => MediatorEff.Send(new ShipOrder(e.OrderId)));
        }
    }
}
