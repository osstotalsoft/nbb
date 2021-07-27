using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Abstractions;
using NBB.Core.Effects;
using NBB.EventStore.Abstractions;
using NBB.Messaging.Abstractions;
using Xunit;
using Mediator = NBB.Application.MediatR.Effects.Mediator;
using MessageBus = NBB.Messaging.Effects.MessageBus;
using Unit = NBB.Core.Effects.Unit;

namespace NBB.ProjectR.Tests
{
    public class ProjectRTests : IClassFixture<TestFixture>
    {
        record ContractCreated(Guid ContractId, decimal Value) : INotification;

        record ContractValidated(Guid ContractId, Guid UserId) : INotification;



        public class ContractProjection
        {
            public record Projection(Guid ContractId, bool IsValidated, Guid? ValidatedByUserId, string ValidatedByUsername);

            public record UserLoaded(Guid ContractId, Guid UserId, string Username) : INotification;


            [SnapshotFrequency(2)]
            class Projector
                : IProjector<Projection>,
                  ISubscribeTo<ContractCreated, ContractValidated, UserLoaded>
            {
                public (Projection, Effect<Unit>) Project(object message, Projection projection) => message switch
                {
                    ContractCreated ev => (new(ev.ContractId, false, null, null), Cmd.None),
                    ContractValidated ev => (projection with { IsValidated = true, ValidatedByUserId = ev.UserId }, LoadUserName(ev)),
                    UserLoaded ev => (projection with { ValidatedByUsername = ev.Username }, Cmd.None),
                    _ => (projection, Cmd.None)
                };

                public object Correlate(object message) => (message as dynamic).ContractId;

                private Effect<Unit> LoadUserName(ContractValidated ev) =>
                    from x in Mediator.Send(new LoadUserById.Query(ev.UserId))
                    from _ in MessageBus.Publish(new UserLoaded(ev.ContractId, x.UserId, x.UserName))
                    select _;
            }

            public class LoadUserById
            {
                public record Query(Guid UserId) : IRequest<Model>;

                public record Model(Guid UserId, string UserName);

                class Handler : IRequestHandler<Query, Model>
                {
                    public Task<Model> Handle(Query request, CancellationToken cancellationToken)
                    {
                        return Task.FromResult(new Model(request.UserId, "rpopovici"));
                    }
                }
            }

        }

        private readonly TestFixture _fixture;

        public ProjectRTests(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task SomeIntegrationTest()
        {
            //Arrange
            await using var container = _fixture.BuildServiceProvider();
            using var scope = container.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var projectionStore = scope.ServiceProvider
                .GetRequiredService<IProjectionStore<ContractProjection.Projection>>();
            var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
            var tcs = new TaskCompletionSource();
            using var _ = await messageBus.SubscribeAsync<ContractProjection.UserLoaded>(
                async msg =>
                {
                    await mediator.Publish(msg.Payload, default);
                    tcs.SetResult();
                }
            );


            var contractId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            
            var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
            var snapshotStore = scope.ServiceProvider.GetRequiredService<ISnapshotStore>();



            //Act
            await mediator.Publish(new ContractCreated(contractId, 100));
            await mediator.Publish(new ContractValidated(contractId, userId));
            await tcs.Task;
            var (projection, loadedAtVersion) = await projectionStore.Load(contractId, CancellationToken.None);



            //Assert
            projection.Should().NotBeNull();
            projection.ContractId.Should().Be(contractId);
            projection.IsValidated.Should().BeTrue();
            projection.ValidatedByUserId.Should().Be(userId);
            projection.ValidatedByUsername.Should().Be("rpopovici");

            var stream = $"PROJ::{typeof(ContractProjection.Projection).GetLongPrettyName()}::{contractId}";
            var events = (await eventStore.GetEventsFromStreamAsync(stream, null));
            events.Count.Should().Be(3);
            var snapshot = await snapshotStore.LoadSnapshotAsync(stream);
            snapshot?.Snapshot.Should().NotBeNull();


        }
    }


}
