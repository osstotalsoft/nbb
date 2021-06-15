using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Messaging.Abstractions;

namespace NBB.ProjectR.Tests
{
    record ContractCreated(Guid ContractId, decimal Value) : INotification;
    record ContractValidated(Guid ContractId, Guid UserId) : INotification;


    class Contract
    {
        record Projection(Guid ContractId, bool IsValidated, Guid? ValidatedByUserId, string ValidatedByUsername) : IHaveIdentityOf<Guid>;
        
        record UserLoaded(Guid ContractId, Guid UserId, string Username) : INotification;
        
        class Projector:
            IProject<ContractCreated, Projection>,
            IProject<ContractValidated, Projection>,
            IProject<UserLoaded, Projection>
        {
            public Projection Project(ContractCreated ev, Projection projection)
                => new(ev.ContractId, false, null, null);

            public Projection Project(ContractValidated ev, Projection projection)
                => projection with { IsValidated = true, ValidatedByUserId = ev.UserId};

            public Projection Project(UserLoaded ev, Projection projection)
                => projection with { ValidatedByUsername = ev.Username};
        }

        class Handler:
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
        
        class Correlation
            : ICorrelate<Projection, Guid>
        {
            public Maybe<Guid> Correlate<TEvent>(TEvent ev) => ev switch
            {
                ContractCreated { Value: >= 0 } cc => cc.ContractId,
                ContractValidated cv => cv.ContractId,
                UserLoaded e => e.ContractId,
                _ => Maybe<Guid>.Nothing
            };
        }
    }

}
