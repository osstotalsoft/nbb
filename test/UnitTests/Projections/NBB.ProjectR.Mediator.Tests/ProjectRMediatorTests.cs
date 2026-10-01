// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Moq;
using NBB.Application.Mediator.Effects;
using NBB.Core.Effects;
using Xunit;
using MessageBus = NBB.Messaging.Effects.MessageBus;

namespace NBB.ProjectR.Mediator.Tests
{
    public record ContractCreated(Guid ContractId, decimal Value) : INotification;

    public record ContractValidated(Guid ContractId, Guid UserId) : INotification;

    public class ContractProjection
    {
        public record Model(Guid ContractId, decimal Value, bool IsValidated = false, Guid? ValidatedByUserId = null, string ValidatedByUsername = null);

        public record Message
        {
            public record CreateContract(Guid ContractId, decimal Value) : Message;

            public record ValidateContract(Guid ContractId, Guid UserId) : Message;

            public record SetUserName(Guid ContractId, string Username) : Message;
        }

        public record ContractProjectionCreated(Guid ContractId, decimal Value);

        [SnapshotFrequency(2)]
        class Projector :
            IProjector<Model, Message, Guid>,
            ISubscribeTo<ContractCreated, ContractValidated>
        {
            public (Model Model, Effect<Message> Effect) Project(Message message, Model model)
                => (message, model) switch
                {
                    (Message.CreateContract msg, null) => (
                        new(msg.ContractId, msg.Value),
                        MessageBus.Publish(new ContractProjectionCreated(msg.ContractId, msg.Value)).Then(Eff.None<Message>())),

                    (Message.ValidateContract msg, { IsValidated: false }) => (
                        model with { IsValidated = true, ValidatedByUserId = msg.UserId },
                        MediatorEff.Send(new LoadUserById.Query(msg.UserId)).Then(x =>
                            Eff.OfMsg<Message>(new Message.SetUserName(msg.ContractId, x.UserName)))),

                    (Message.SetUserName msg, not null) => (model with { ValidatedByUsername = msg.Username }, Eff.None<Message>()),

                    _ => (model, Eff.None<Message>())
                };

            public (Guid Identity, Message Message) Subscribe(object @event) => @event switch
            {
                ContractCreated ev => (ev.ContractId, new Message.CreateContract(ev.ContractId, ev.Value)),
                ContractValidated ev => (ev.ContractId, new Message.ValidateContract(ev.ContractId, ev.UserId)),
                _ => (default, default)
            };
        }
    }

    public class LoadUserById
    {
        public record Query(Guid UserId) : IQuery<Model>;

        public record Model(Guid UserId, string UserName);

        public class Handler : IQueryHandler<Query, Model>
        {
            public ValueTask<Model> Handle(Query query, CancellationToken cancellationToken)
                => new(new Model(query.UserId, "rpopovici"));
        }
    }

    public class ProjectRMediatorTests
    {
        [Fact]
        public async Task Should_project_mediator_notifications()
        {
            //Arrange
            using var host = BuildHost(services => services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped));
            await host.StartAsync();
            var contractId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            //Act
            using var scope = host.Services.CreateScope();
            var publisher = scope.ServiceProvider.GetRequiredService<IPublisher>();
            await publisher.Publish(new ContractCreated(contractId, 100));
            await publisher.Publish((object)new ContractValidated(contractId, userId));
            var projection = await scope.ServiceProvider.GetRequiredService<IReadModelStore<ContractProjection.Model>>()
                .Load(contractId, CancellationToken.None);

            //Assert
            projection.Should().NotBeNull();
            projection.Value.Should().Be(100);
            projection.IsValidated.Should().BeTrue();
            projection.ValidatedByUserId.Should().Be(userId);
            projection.ValidatedByUsername.Should().Be("rpopovici");
            await host.StopAsync();
        }

        [Fact]
        public async Task Should_fail_host_start_when_mediator_is_singleton()
        {
            //Arrange
            using var host = BuildHost(services => services.AddSingleton(Mock.Of<IMediator>()));

            //Act
            var act = () => host.StartAsync();

            //Assert
            await act.Should().ThrowAsync<OptionsValidationException>().WithMessage("*NBB.ProjectR.Mediator requires Mediator.IMediator with the Scoped or Transient lifetime*");
        }

        [Fact]
        public void Should_require_AddProjectR()
        {
            var act = () => new ServiceCollection().AddProjectRMediatorHandlers();

            act.Should().Throw<InvalidOperationException>();
        }

        private static IHost BuildHost(Action<IServiceCollection> addMediator)
        {
            var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
            var services = builder.Services;
            addMediator(services);
            services
                .AddProjectR(typeof(ContractProjection).Assembly)
                .AddProjectRMediatorHandlers();
            services
                .AddEffects()
                .AddMessagingEffects()
                .AddMediatorEffects();
            services.AddMessageBus().AddInProcessTransport();
            services.AddEventStore(b => b.UseNewtownsoftJson().UseInMemoryEventRepository());

            return builder.Build();
        }
    }
}
