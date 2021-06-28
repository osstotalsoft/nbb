using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Effects;
using Xunit;
using Mediator = NBB.Application.MediatR.Effects.Mediator;
using MessageBus = NBB.Messaging.Effects.MessageBus;
using Unit = NBB.Core.Effects.Unit;

namespace NBB.ProjectR.Tests
{



    public class UnitTest2
    {
        record ContractCreated(Guid ContractId, decimal Value) : INotification;

        record ContractValidated(Guid ContractId, Guid UserId) : INotification;



        class ContractProjection
        {
            record Message(INotification _) : OneOf<ContractCreated, ContractValidated, UserLoaded>;

            record Projection(Guid ContractId, bool IsValidated, Guid? ValidatedByUserId, string ValidatedByUsername) : IHaveIdentityOf<Guid>;

            record LoadUsed(Guid UserId) : IRequest<UserLoaded>;

            public record UserLoaded(Guid ContractId, Guid UserId, string Username) : INotification;


            class Projector : IProjector<Message, Projection, Guid>
            {
                public (Projection, Effect<Unit>) Project(Message message, Projection projection) => message._ switch
                {
                    ContractCreated ev => (new(ev.ContractId, false, null, null), Cmd.None),
                    ContractValidated ev => (projection with { IsValidated = true, ValidatedByUserId = ev.UserId }, LoadUserName(ev)),
                    UserLoaded ev => (projection with { ValidatedByUsername = ev.Username }, Cmd.None),
                    _ => (projection, Cmd.None)
                };


                public Maybe<Guid> Correlate(Message message) => message._ switch
                {
                    ContractCreated { Value: >= 0 } cc => cc.ContractId,
                    ContractValidated cv => cv.ContractId,
                    UserLoaded e => e.ContractId,
                    _ => Maybe<Guid>.Nothing
                };

                private Effect<Unit> LoadUserName(ContractValidated ev) =>
                    from x in Mediator.Send(new LoadUserById.Query(ev.UserId))
                    from _ in MessageBus.Publish(new UserLoaded(ev.ContractId, x.UserId, x.UserName))
                    select _;
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
        public async Task Test2()
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
