// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Messaging.MediatR.Tests
{
    public class MediatRMessagingHostBuilderTests
    {
        [Fact]
        public void Should_register_handled_commands_singleton()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            var provider = Mock.Of<IServiceProvider>();
            Mock.Get(services).Setup(x => x.GetEnumerator())
                .Returns(new List<ServiceDescriptor>
                {
                    new ServiceDescriptor(typeof(IRequestHandler<CommandMessage>), new CommandHandler())
                }.GetEnumerator());

            //Act
            var builder = new MessagingHostConfigurationBuilder(provider, services);
            builder
                .AddSubscriberServices(cfg => cfg.FromMediatRHandledCommands().AddAllClasses())
                .WithDefaultOptions()
                .UsePipeline(_ => { });

            var config = builder.Build();

            //Assert
            config.Subscribers.Should().NotBeEmpty();
            config.Subscribers[0].MessageType.Should().Be(typeof(CommandMessage));
            config.Subscribers[0].Options.Should().Be(MessagingSubscriberOptions.Default);
            config.Subscribers[0].Pipeline.Should().NotBeNull();
        }

        [Fact]
        public void Should_register_handled_events_singleton()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            var provider = Mock.Of<IServiceProvider>();
            Mock.Get(services).Setup(x => x.GetEnumerator())
                .Returns(new List<ServiceDescriptor>
                {
                    new ServiceDescriptor(typeof(INotificationHandler<EventMessage>), new EventHandler())
                }.GetEnumerator());

            //Act
            var builder = new MessagingHostConfigurationBuilder(provider, services);
            builder
                .AddSubscriberServices(cfg => cfg.FromMediatRHandledEvents().AddAllClasses())
                .WithDefaultOptions()
                .UsePipeline(_ => { });

            var config = builder.Build();

            //Assert
            config.Subscribers.Should().NotBeEmpty();
            config.Subscribers[0].MessageType.Should().Be(typeof(EventMessage));
            config.Subscribers[0].Options.Should().Be(MessagingSubscriberOptions.Default);
            config.Subscribers[0].Pipeline.Should().NotBeNull();
        }


        [Fact]
        public void Should_register_handled_queries_singleton()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            var provider = Mock.Of<IServiceProvider>();
            Mock.Get(services).Setup(x => x.GetEnumerator())
                .Returns(new List<ServiceDescriptor>
                {
                    new ServiceDescriptor(typeof(IRequestHandler<QueryMessage, string>), new QueryHandler())
                }.GetEnumerator());

            //Act
            var builder = new MessagingHostConfigurationBuilder(provider, services);
            builder
                .AddSubscriberServices(cfg => cfg.FromMediatRHandledQueries().AddAllClasses())
                .WithDefaultOptions()
                .UsePipeline(_ => { });

            var config = builder.Build();

            //Assert
            config.Subscribers.Should().NotBeEmpty();
            config.Subscribers[0].MessageType.Should().Be(typeof(QueryMessage));
            config.Subscribers[0].Options.Should().Be(MessagingSubscriberOptions.Default);
            config.Subscribers[0].Pipeline.Should().NotBeNull();
        }

        [Fact]
        public void Should_discover_handled_messages_from_the_selected_service_collection()
        {
            //Arrange
            var hostServices = new ServiceCollection();
            hostServices.AddSingleton<INotificationHandler<EventMessage>>(new EventHandler());
            var moduleServices = new ServiceCollection();
            moduleServices.AddSingleton<IRequestHandler<CommandMessage>>(new CommandHandler());

            //Act
            var builder = new MessagingHostConfigurationBuilder(Mock.Of<IServiceProvider>(), hostServices);
            builder
                // events from the host collection, commands from the module collection, in the same subscriber group
                .AddSubscriberServices(cfg => cfg
                    .FromMediatRHandledEvents().AddAllClasses()
                    .FromServiceCollection(moduleServices)
                    .FromMediatRHandledCommands().AddAllClasses())
                .WithDefaultOptions()
                .UsePipeline(_ => { });

            var config = builder.Build();

            //Assert
            config.Subscribers.Select(s => s.MessageType).Should().BeEquivalentTo([typeof(EventMessage), typeof(CommandMessage)]);
        }

        public record CommandMessage : IRequest;

        public record EventMessage : INotification;

        public record QueryMessage : IRequest<string>;

        public class CommandHandler : IRequestHandler<CommandMessage>
        {
            public Task Handle(CommandMessage request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public class EventHandler : INotificationHandler<EventMessage>
        {
            public Task Handle(EventMessage notification, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }

        public class QueryHandler : IRequestHandler<QueryMessage, string>
        {
            public Task<string> Handle(QueryMessage request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
            }
        }
    }
}
