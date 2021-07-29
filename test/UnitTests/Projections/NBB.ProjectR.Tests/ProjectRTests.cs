using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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
        record ContractSigned(Guid ContractId, Guid SignerId) : INotification;



        public class ContractProjection
        {
            public record Projection(Guid ContractId, decimal Value, bool IsValidated = false, Guid? ValidatedByUserId = null, string ValidatedByUsername = null, bool IsSigned = false);

            //Messages
            record CreateContract(Guid ContractId, decimal Value) : IMessage<Projection>;

            record ValidateContract(Guid ContractId, Guid UserId) : IMessage<Projection>;

            record SignContract(Guid ContractId, Guid SignerId) : IMessage<Projection>;

            record SetUserName(Guid ContractId, string Username) : IMessage<Projection>;

            

            //Events
            public record ContractProjectionCreated(Guid ContractId, decimal Value) : INotification;
            public record ContractProjectionValidated(Guid ContractId, Guid? UserId, string Username) : INotification;

            
            //private static readonly Effect<IMessage<Projection>> Nothing = Effect.Pure<IMessage<Projection>>(null);
            

            [SnapshotFrequency(2)]
            class Projector
                : IProjector<Projection, ContractCreated, ContractValidated, ContractSigned>
            {
                public (Projection, Effect<Unit>) Project(IMessage<Projection> message, Projection projection) =>
                    (message, projection) switch
                    {
                        (CreateContract msg, null) => (
                            new(msg.ContractId, msg.Value),
                            MessageBus.Publish(new ContractProjectionCreated(msg.ContractId, msg.Value))),

                        (ValidateContract msg, { IsValidated: false }) => (
                            projection with { IsValidated = true, ValidatedByUserId = msg.UserId },
                            LoadUserName(msg.ContractId, msg.UserId)),

                        (SetUserName msg, not null) => (
                            projection with { ValidatedByUsername = msg.Username },
                            MessageBus.Publish(new ContractProjectionValidated(projection.ContractId, projection.ValidatedByUserId, msg.Username))),

                        (SignContract, not null) => (projection with { IsSigned = true }, Cmd.None),

                        _ => (projection, Cmd.None)
                    };

                public IMessage<Projection> Subscribe(object @event) => @event switch
                {
                    ContractCreated ev => new CreateContract(ev.ContractId, ev.Value),
                    ContractValidated ev => new ValidateContract(ev.ContractId, ev.UserId),
                    ContractSigned ev => new SignContract(ev.ContractId, ev.SignerId),
                    _ => null
                };
                
                public object Identify(IMessage<Projection> message)
                    => (message as dynamic).ContractId;


                private Effect<Unit> LoadUserName(Guid contractId, Guid userId) =>
                    Mediator.Send(new LoadUserById.Query(userId)).Then(x => Cmd.Project(new SetUserName(contractId, x.UserName)));
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

            var contractId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
            var snapshotStore = scope.ServiceProvider.GetRequiredService<ISnapshotStore>();



            //Act
            await mediator.Publish(new ContractCreated(contractId, 100));
            await mediator.Publish(new ContractValidated(contractId, userId));
            var (projection, _) = await projectionStore.Load(contractId, CancellationToken.None);



            //Assert
            projection.Should().NotBeNull();
            projection.ContractId.Should().Be(contractId);
            projection.IsValidated.Should().BeTrue();
            projection.ValidatedByUserId.Should().Be(userId);
            projection.ValidatedByUsername.Should().Be("rpopovici");

            var stream = $"PROJ::{typeof(ContractProjection.Projection).GetLongPrettyName()}::{contractId}";
            var events = await eventStore.GetEventsFromStreamAsync(stream, null);
            events.Count.Should().Be(3);
            var snapshot = await snapshotStore.LoadSnapshotAsync(stream);
            snapshot?.Snapshot.Should().NotBeNull();


        }
    }


}
