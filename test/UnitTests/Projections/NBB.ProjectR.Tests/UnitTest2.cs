using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NBB.Messaging.Abstractions;
using Xunit;

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
                public Projection Project(Message message, Projection projection) => message._ switch
                {
                    ContractCreated ev => new(ev.ContractId, false, null, null),
                    ContractValidated ev => projection with { IsValidated = true, ValidatedByUserId = ev.UserId },
                    UserLoaded ev => projection with { ValidatedByUsername = ev.Username },
                    _ => projection
                };


                public Maybe<Guid> Correlate(Message message) => message._ switch
                {
                    ContractCreated { Value: >= 0 } cc => cc.ContractId,
                    ContractValidated cv => cv.ContractId,
                    UserLoaded e => e.ContractId,
                    _ => Maybe<Guid>.Nothing
                };


            }

            class Handler :
                INotificationHandler<ContractValidated>
            {
                private readonly IMessageBus _messageBus;

                public Handler(IMessageBus messageBus)
                {
                    _messageBus = messageBus;
                }

                public async Task Handle(ContractValidated ev, CancellationToken cancellationToken)
                {
                    Task<string> LoadUserNameBy(Guid userId) => Task.FromResult("rpopovici");

                    var userName = await LoadUserNameBy(ev.UserId);
                    await _messageBus.PublishAsync(new UserLoaded(ev.ContractId, ev.UserId, userName), cancellationToken);
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
