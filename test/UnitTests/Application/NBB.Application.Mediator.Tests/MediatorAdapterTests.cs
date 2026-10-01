// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NBB.Application.Mediator.Effects;
using NBB.Core.Abstractions;
using NBB.Core.Effects;
using NBB.Data.Abstractions;
using Xunit;

namespace NBB.Application.Mediator.Tests
{
    public class MediatorAdapterTests
    {
        [Fact]
        public async Task MediatorEventPublisher_should_publish_only_mediator_notifications_in_order()
        {
            //Arrange
            await using var sp = BuildServiceProvider();
            using var scope = sp.CreateScope();
            var probe = sp.GetRequiredService<Probe>();
            var sut = scope.ServiceProvider.GetRequiredService<IEventPublisher>();

            //Act
            await sut.PublishAsync([new SomethingHappened(1), new PlainEvent(), new SomethingHappened(2)]);

            //Assert
            sut.Should().BeOfType<MediatorEventPublisher>();
            probe.Events.Should().Equal(1, 2);
        }

        [Fact]
        public async Task EventPublishingUowDecorator_should_publish_to_mediator_handlers()
        {
            //Arrange
            await using var sp = BuildServiceProvider();
            using var scope = sp.CreateScope();
            var inner = new Mock<IUow<IEventedEntity>>();
            inner.Setup(x => x.GetChanges()).Returns([Mock.Of<IEventedEntity>(e => e.GetUncommittedChanges() == new object[] { new SomethingHappened(7) })]);
            inner.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
            var sut = new EventPublishingUowDecorator<IEventedEntity>(inner.Object, scope.ServiceProvider.GetRequiredService<IEventPublisher>());

            //Act
            await sut.SaveChangesAsync();

            //Assert
            sp.GetRequiredService<Probe>().Events.Should().Equal(7);
        }

        [Fact]
        public async Task MediatorEffect_should_send_requests_commands_and_queries_and_publish_notifications()
        {
            //Arrange
            await using var sp = BuildServiceProvider();
            using var scope = sp.CreateScope();
            var interpreter = scope.ServiceProvider.GetRequiredService<IInterpreter>();
            var probe = sp.GetRequiredService<Probe>();

            //Act
            var requestResult = await interpreter.Interpret(MediatorEff.Send(new GetValue(1)));
            var commandResult = await interpreter.Interpret(MediatorEff.Send(new CreateValue(2)));
            var queryResult = await interpreter.Interpret(MediatorEff.Send(new QueryValue(3)));
            await interpreter.Interpret(MediatorEff.Send(new DoRequest(4)));
            await interpreter.Interpret(MediatorEff.Send(new DoCommand(5)));
            await interpreter.Interpret(MediatorEff.Publish(new SomethingHappened(6)));

            //Assert
            requestResult.Should().Be("request 1");
            commandResult.Should().Be("command 2");
            queryResult.Should().Be("query 3");
            probe.Voids.Should().Equal(4, 5);
            probe.Events.Should().Equal(6);
        }

        private static ServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddSingleton<Probe>();
            services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
            services.AddMediatorIntegration();
            services.AddEffects();
            services.AddMediatorEffects();
            return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        }
    }

    public class Probe
    {
        public ConcurrentQueue<int> Events { get; } = new();
        public ConcurrentQueue<int> Voids { get; } = new();
    }

    public record SomethingHappened(int Id) : INotification;

    public record PlainEvent;

    public record GetValue(int Id) : IRequest<string>;

    public record CreateValue(int Id) : ICommand<string>;

    public record QueryValue(int Id) : IQuery<string>;

    public record DoRequest(int Id) : IRequest;

    public record DoCommand(int Id) : ICommand;

    public class SomethingHappenedHandler(Probe probe) : INotificationHandler<SomethingHappened>
    {
        public ValueTask Handle(SomethingHappened notification, CancellationToken cancellationToken)
        {
            probe.Events.Enqueue(notification.Id);
            return default;
        }
    }

    public class Handlers(Probe probe) :
        IRequestHandler<GetValue, string>,
        ICommandHandler<CreateValue, string>,
        IQueryHandler<QueryValue, string>,
        IRequestHandler<DoRequest>,
        ICommandHandler<DoCommand>
    {
        public ValueTask<string> Handle(GetValue request, CancellationToken cancellationToken) => new($"request {request.Id}");

        public ValueTask<string> Handle(CreateValue command, CancellationToken cancellationToken) => new($"command {command.Id}");

        public ValueTask<string> Handle(QueryValue query, CancellationToken cancellationToken) => new($"query {query.Id}");

        public ValueTask<global::Mediator.Unit> Handle(DoRequest request, CancellationToken cancellationToken)
        {
            probe.Voids.Enqueue(request.Id);
            return global::Mediator.Unit.ValueTask;
        }

        public ValueTask<global::Mediator.Unit> Handle(DoCommand command, CancellationToken cancellationToken)
        {
            probe.Voids.Enqueue(command.Id);
            return global::Mediator.Unit.ValueTask;
        }
    }
}
