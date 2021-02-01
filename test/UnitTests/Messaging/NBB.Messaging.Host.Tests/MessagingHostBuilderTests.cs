using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host.Builder;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Messaging.Host.Tests
{
    public class MessagingHostBuilderTests
    {
        [Fact]
        public void Should_register_pipeline_scoped()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            ServiceDescriptor registeredDescriptor = null;
            Mock.Get(services).Setup(x => x.Add(It.IsAny<ServiceDescriptor>()))
                .Callback((ServiceDescriptor sd) => registeredDescriptor = sd);

            //Act
            new MessagingHostBuilder(services).UsePipeline(_ => { });

            //Assert
            registeredDescriptor.Should().NotBeNull();
            registeredDescriptor.ServiceType.Should().Be(typeof(PipelineDelegate<MessagingEnvelope>));
            registeredDescriptor.Lifetime.Should().Be(ServiceLifetime.Scoped);
        }

        [Fact]
        public void Should_register_subscribers_singleton()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            ServiceDescriptor registeredDescriptor = null;
            Mock.Get(services).Setup(x => x.Add(It.IsAny<ServiceDescriptor>()))
                .Callback((ServiceDescriptor sd) => registeredDescriptor = sd);

            //Act
            new MessagingHostBuilder(services)
                .AddSubscriberServices(cfg => cfg.FromAssemblyOf<MessageToScanBase>().AddClassesAssignableTo<MessageToScanBase>())
                .WithDefaultOptions();

            //Assert
            registeredDescriptor.Should().NotBeNull();
            registeredDescriptor.ServiceType.Should().Be(typeof(IHostedService));
            registeredDescriptor.ImplementationFactory.Should().NotBeNull();
            registeredDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_register_handled_commands_singleton()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            ServiceDescriptor registeredDescriptor = null;
            Mock.Get(services).Setup(x => x.Add(It.IsAny<ServiceDescriptor>()))
                .Callback((ServiceDescriptor sd) => registeredDescriptor = sd);
            Mock.Get(services).Setup(x => x.GetEnumerator())
                .Returns(new List<ServiceDescriptor> {
                    new ServiceDescriptor(typeof(IRequestHandler<CommandMessage, Unit>), new CommandHandler())
                }.GetEnumerator());

            //Act
            new MessagingHostBuilder(services).AddSubscriberServices(cfg => cfg.FromMediatRHandledCommands().AddAllClasses()).WithDefaultOptions();

            //Assert
            registeredDescriptor.Should().NotBeNull();
            registeredDescriptor.ServiceType.Should().Be(typeof(IHostedService));
            registeredDescriptor.ImplementationFactory.Should().NotBeNull();
            registeredDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void Should_register_handled_events_singleton()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            ServiceDescriptor registeredDescriptor = null;
            Mock.Get(services).Setup(x => x.Add(It.IsAny<ServiceDescriptor>()))
                .Callback((ServiceDescriptor sd) => registeredDescriptor = sd);
            Mock.Get(services).Setup(x => x.GetEnumerator())
                .Returns(new List<ServiceDescriptor> {
                    new ServiceDescriptor(typeof(INotificationHandler<EventMessage>), new EventHandler())
                }.GetEnumerator());

            //Act
            new MessagingHostBuilder(services).AddSubscriberServices(cfg => cfg.FromMediatRHandledEvents().AddAllClasses()).WithDefaultOptions();

            //Assert
            registeredDescriptor.Should().NotBeNull();
            registeredDescriptor.ServiceType.Should().Be(typeof(IHostedService));
            registeredDescriptor.ImplementationFactory.Should().NotBeNull();
            registeredDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);        }


        [Fact]
        public void Should_register_handled_queries_singleton()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            ServiceDescriptor registeredDescriptor = null;
            Mock.Get(services).Setup(x => x.Add(It.IsAny<ServiceDescriptor>()))
                .Callback((ServiceDescriptor sd) => registeredDescriptor = sd);
            Mock.Get(services).Setup(x => x.GetEnumerator())
                .Returns(new List<ServiceDescriptor> {
                    new ServiceDescriptor(typeof(IRequestHandler<QueryMessage, string>), new QueryHandler())
                }.GetEnumerator());

            //Act
            new MessagingHostBuilder(services)
                .AddSubscriberServices(cfg => cfg.FromMediatRHandledQueries().AddAllClasses())
                .WithDefaultOptions();

            //Assert
            registeredDescriptor.Should().NotBeNull();
            registeredDescriptor.ServiceType.Should().Be(typeof(IHostedService));
            registeredDescriptor.ImplementationFactory.Should().NotBeNull();
            registeredDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);        }

        [Fact]
        public void Should_register_handled_topics_singleton()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            ServiceDescriptor registeredDescriptor = null;
            Mock.Get(services).Setup(x => x.Add(It.IsAny<ServiceDescriptor>()))
                .Callback((ServiceDescriptor sd) => registeredDescriptor = sd);

            //Act
            new MessagingHostBuilder(services)
                .AddSubscriberServices(cfg => cfg.FromTopics("TopicName"))
                .WithDefaultOptions();

            //Assert
            registeredDescriptor.Should().NotBeNull();
            registeredDescriptor.ServiceType.Should().Be(typeof(IHostedService));
            registeredDescriptor.ImplementationFactory.Should().NotBeNull();
            registeredDescriptor.Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        public record CommandMessage : IRequest;

        public record EventMessage : INotification;

        public record QueryMessage : IRequest<string>;

        public abstract class MessageToScanBase 
        {
            public Guid MessageId => throw new NotImplementedException();
        }

        public class MessageToScan : MessageToScanBase
        {
        }

        public class CommandHandler : IRequestHandler<CommandMessage>
        {
            public Task<Unit> Handle(CommandMessage request, CancellationToken cancellationToken)
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
