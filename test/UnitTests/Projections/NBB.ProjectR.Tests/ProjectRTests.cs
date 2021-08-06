using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Abstractions;
using NBB.Core.Effects;
using NBB.EventStore.Abstractions;
using Xunit;
using Mediator = NBB.Application.MediatR.Effects.Mediator;
using MessageBus = NBB.Messaging.Effects.MessageBus;

namespace NBB.ProjectR.Tests
{

    public class ProjectRTests : IClassFixture<TestFixture>
    {
        record ContractCreated(Guid ContractId, decimal Value) : INotification;

        record ContractValidated(Guid ContractId, Guid UserId) : INotification;

        record ContractSigned(Guid ContractId, Guid SignerId) : INotification;



        public class ContractProjection
        {
            public record Model(Guid ContractId, decimal Value, bool IsValidated = false,
                Guid? ValidatedByUserId = null, string ValidatedByUsername = null, bool IsSigned = false);


            //Messages
            public record Message
            {
                public record CreateContract(Guid ContractId, decimal Value) : Message;

                public record ValidateContract(Guid ContractId, Guid UserId) : Message;

                public record SignContract(Guid ContractId, Guid SignerId) : Message;

                public record SetUserName(Guid ContractId, string Username) : Message;
            }

            


            //Events
            public record ContractProjectionCreated(Guid ContractId, decimal Value);

            public record ContractProjectionValidated(Guid ContractId, Guid? UserId, string Username);


            [SnapshotFrequency(2)]
            class Projector: 
                IProjector<Model, Message, Guid>,
                ISubscribeTo<ContractCreated, ContractValidated, ContractSigned>
            {
                public (Model Model, Effect<Message> Effect) Project(Message message, Model model)
                    => (message, model) switch
                    {
                        (Message.CreateContract msg, null) => (
                            new(msg.ContractId, msg.Value),
                            MessageBus.Publish(new ContractProjectionCreated(msg.ContractId, msg.Value))
                                .Then(Eff.None<Message>())),

                        (Message.ValidateContract msg, { IsValidated: false }) => (
                            model with { IsValidated = true, ValidatedByUserId = msg.UserId },
                            Mediator.Send(new LoadUserById.Query(msg.UserId)).Then(x =>
                                Eff.OfMsg<Message>(new Message.SetUserName(msg.ContractId, x.UserName)))),

                        (Message.SetUserName msg, not null) => (
                            model with { ValidatedByUsername = msg.Username },
                            MessageBus.Publish(new ContractProjectionValidated(model.ContractId, model.ValidatedByUserId, msg.Username))
                                .Then(Eff.None<Message>())),

                        (Message.SignContract, not null) => (model with { IsSigned = true }, Eff.None<Message>()),

                        _ => (model, Eff.None<Message>())
                    };

                public (Guid Identity, Message Message) Subscribe(INotification @event) => @event switch
                {
                    ContractCreated ev => (ev.ContractId, new Message.CreateContract(ev.ContractId, ev.Value)),
                    ContractValidated ev => (ev.ContractId, new Message.ValidateContract(ev.ContractId, ev.UserId)),
                    ContractSigned ev => (ev.ContractId, new Message.SignContract(ev.ContractId, ev.SignerId)),
                    _ => (default, default)
                };
            }
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
                .GetRequiredService<IReadModelStore<ContractProjection.Model>>();

            var contractId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();
            var snapshotStore = scope.ServiceProvider.GetRequiredService<ISnapshotStore>();



            //Act
            await mediator.Publish(new ContractCreated(contractId, 100));
            await mediator.Publish(new ContractValidated(contractId, userId));
            var projection = await projectionStore.Load(contractId, CancellationToken.None);



            //Assert
            projection.Should().NotBeNull();
            projection.ContractId.Should().Be(contractId);
            projection.IsValidated.Should().BeTrue();
            projection.ValidatedByUserId.Should().Be(userId);
            projection.ValidatedByUsername.Should().Be("rpopovici");

            var stream = $"PROJ::{typeof(ContractProjection.Model).GetLongPrettyName()}::{contractId}";
            var events = await eventStore.GetEventsFromStreamAsync(stream, null);
            events.Count.Should().Be(3);
            var snapshot = await snapshotStore.LoadSnapshotAsync(stream);
            snapshot?.Snapshot.Should().NotBeNull();


        }
    }


}
