using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Effects;
using Xunit;
using Unit = NBB.Core.Effects.Unit;
using NBB.Messaging.Effects;
using Mediator = NBB.Application.MediatR.Effects.Mediator;

namespace NBB.ProjectR.Tests
{
    public class UnitTest1
    {
        record ContractCreated(Guid ContractId, decimal Value) : INotification;
        record ContractValidated(Guid ContractId, Guid UserId) : INotification;


        class ContractProjection
        {
            record Projection(Guid ContractId, bool IsValidated, Guid? ValidatedByUserId, string ValidatedByUsername) : IHaveIdentityOf<Guid>;

            record UserLoaded(Guid ContractId, Guid UserId, string Username) : INotification;

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


        [Fact]
        public async Task Test1()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddProjectR(GetType().Assembly);
            services.AddMediatR(GetType().Assembly);
            await using var container = services.BuildServiceProvider();
            using var scope = container.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var contractId = Guid.NewGuid();

            //Act
            await mediator.Publish(new ContractCreated(contractId, 100));

            //Assert
        }
    }


}
