//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using FluentAssertions;
//using MediatR;
//using Microsoft.Extensions.DependencyInjection;
//using NBB.Messaging.Abstractions;
//using Xunit;

//namespace NBB.ProjectR.Tests
//{
//    public class Event<TEvent>
//    {

//    }
//    abstract record Message
//    {
//        record 
//    }

//    public class UnitTest2
//    {
//        record ContractCreated(Guid ContractId, decimal Value) : INotification;

//        record ContractValidated(Guid ContractId, Guid UserId) : INotification;




//        class Contract
//        {
//            record Projection(Guid ContractId, bool IsValidated, Guid? ValidatedByUserId, string ValidatedByUsername) : IHaveIdentityOf<Guid>;

//            record UserLoaded(Guid ContractId, Guid UserId, string Username) : INotification;



//            class Projector :
//                IProject<object, Projection>
//            {
//                public Projection Project(object notification, Projection projection) => notification switch
//                {
//                    ContractCreated ev => new(ev.ContractId, false, null, null),
//                    ContractValidated ev => projection with { IsValidated = true, ValidatedByUserId = ev.UserId },
//                    UserLoaded ev => projection with { ValidatedByUsername = ev.Username },
//                    _ => projection
//                };


//            }

//            class Handler :
//                INotificationHandler<ContractValidated>
//            {
//                private readonly IMessageBus _messageBus;

//                public Handler(IMessageBus messageBus)
//                {
//                    _messageBus = messageBus;
//                }

//                public async Task Handle(ContractValidated ev, CancellationToken cancellationToken)
//                {
//                    Task<string> LoadUserNameBy(Guid userId) => Task.FromResult("rpopovici");

//                    var userName = await LoadUserNameBy(ev.UserId);
//                    await _messageBus.PublishAsync(new UserLoaded(ev.ContractId, ev.UserId, userName), cancellationToken);
//                }
//            }

//            class Correlation
//                : ICorrelate<Projection, Guid>
//            {
//                public Maybe<Guid> Correlate<TEvent>(TEvent ev) => ev switch
//                {
//                    ContractCreated { Value: >= 0 } cc => cc.ContractId,
//                    ContractValidated cv => cv.ContractId,
//                    UserLoaded e => e.ContractId,
//                    _ => Maybe<Guid>.Nothing
//                };
//            }
//        }


//        [Fact]
//        public async Task Test1()
//        {
//            //Arrange
//            var services = new ServiceCollection();
//            services.AddProjectR(GetType().Assembly);
//            await using var container = services.BuildServiceProvider();
//            using var scope = container.CreateScope();
//            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
//            var contractId = Guid.NewGuid();

//            //Act
//            await mediator.Publish(new ContractCreated(contractId, 100));

//            //Assert
//        }
//    }


//}
