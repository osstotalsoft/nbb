using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NBB.Application.MediatR.Effects;
using NBB.Core.Effects;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Effects;
using NBB.Messaging.InProcessMessaging.Extensions;
using Xunit;
using Unit = NBB.Core.Effects.Unit;
using Mediator = NBB.Application.MediatR.Effects.Mediator;
using MessageBus = NBB.Messaging.Effects.MessageBus;

namespace NBB.ProjectR.Tests
{
    public class UnitTest1 : IClassFixture<TestFixture>
    {
        record ContractCreated(Guid ContractId, decimal Value) : INotification;
        record ContractValidated(Guid ContractId, Guid UserId) : INotification;


        public class ContractProjection
        {
            public record Projection(Guid ContractId, bool IsValidated, Guid? ValidatedByUserId, string ValidatedByUsername) : IHaveIdentityOf<Guid>;

            public record UserLoaded(Guid ContractId, Guid UserId, string Username) : INotification;

            class Projector :
                IProjector<ContractCreated, Projection, Guid>,
                IProjector<ContractValidated, Projection, Guid>,
                IProjector<UserLoaded, Projection, Guid>
            {
                public Maybe<Guid> Correlate(ContractCreated ev) => ev.ContractId;

                public Maybe<Guid> Correlate(ContractValidated ev) => ev.ContractId;

                public Maybe<Guid> Correlate(UserLoaded ev) => ev.ContractId;

                public (Projection, Effect<Unit>) Project(ContractCreated ev, Projection projection)
                    => (new(ev.ContractId, false, null, null), Cmd.None);

                public (Projection, Effect<Unit>) Project(ContractValidated ev, Projection projection)
                    => (projection with { IsValidated = true, ValidatedByUserId = ev.UserId },
                        Mediator.Send(new LoadUserById.Query(ev.UserId)).Then(x =>
                            MessageBus.Publish(new UserLoaded(ev.ContractId, x.UserId, x.UserName))));

                public (Projection, Effect<Unit>) Project(UserLoaded ev, Projection projection)
                    => (projection with { ValidatedByUsername = ev.Username }, Cmd.None);
            }

            class LoadUserById
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

        public UnitTest1(TestFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Test1()
        {
            //Arrange
            await using var container = _fixture.BuildServiceProvider();
            using var scope = container.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var repo = scope.ServiceProvider
                .GetRequiredService<IProjectionStore<ContractProjection.Projection, Guid>>();
            var messageBus = scope.ServiceProvider.GetRequiredService<IMessageBus>();
            using var _ = await messageBus.SubscribeAsync<ContractProjection.UserLoaded>(msg =>
                mediator.Publish(msg.Payload, default)
            );


            var contractId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            

            //Act
            await mediator.Publish(new ContractCreated(contractId, 100));
            await mediator.Publish(new ContractValidated(contractId, userId));
            

            //Assert
            var projection = await repo.LoadById(contractId, CancellationToken.None);
            projection.Should().NotBeNull();
        }
    }


}
