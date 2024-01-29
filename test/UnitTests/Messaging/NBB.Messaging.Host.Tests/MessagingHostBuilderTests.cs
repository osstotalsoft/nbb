// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
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
        public void Should_register_subscribers_singleton()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            var provider = Mock.Of<IServiceProvider>();

            //Act
            var builder = new MessagingHostConfigurationBuilder(provider, services);
            builder
                .AddSubscriberServices(cfg =>
                    cfg.FromAssemblyOf<MessageToScanBase>().AddClassesAssignableTo<MessageToScanBase>())
                .WithDefaultOptions()
                .UsePipeline(bld => { });

            var config = builder.Build();

            //Assert
            config.Subscribers.Should().NotBeEmpty();
            config.Subscribers[0].MessageType.Should().Be(typeof(MessageToScan));
            config.Subscribers[0].Options.Should().Be(MessagingSubscriberOptions.Default);
            config.Subscribers[0].Pipeline.Should().NotBeNull();
        }

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
        public void Should_register_handled_topics_singleton()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            var provider = Mock.Of<IServiceProvider>();

            //Act
            var builder = new MessagingHostConfigurationBuilder(provider, services);
            builder
                .AddSubscriberServices(cfg => cfg.FromTopics("TopicName"))
                .WithDefaultOptions()
                .UsePipeline(_ => { });
            var config = builder.Build();

            //Assert
            config.Subscribers.Should().NotBeEmpty();
            config.Subscribers[0].MessageType.Should().Be(typeof(object));
            config.Subscribers[0].Options.Should().Be(MessagingSubscriberOptions.Default with {TopicName = "TopicName" });
            config.Subscribers[0].Pipeline.Should().NotBeNull();
        }

        [Fact]
        public void Should_register_same_pipeline_for_more_subscriber_groups()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            var provider = Mock.Of<IServiceProvider>();

            //Act
            var builder = new MessagingHostConfigurationBuilder(provider, services);
            builder
                .AddSubscriberServices(cfg => cfg.FromTopics("SomeTopicName")).WithDefaultOptions()
                .AddSubscriberServices(cfg => cfg.FromTopics("OtherTopicName")).WithDefaultOptions()
                .UsePipeline(_ => { });
            var config = builder.Build();

            //Assert
            config.Subscribers.Should().HaveCount(2);
            config.Subscribers[0].Options.Should().Be(MessagingSubscriberOptions.Default with { TopicName = "SomeTopicName" });
            config.Subscribers[1].Options.Should().Be(MessagingSubscriberOptions.Default with { TopicName = "OtherTopicName" });
            config.Subscribers[0].Pipeline.Should().Be(config.Subscribers[1].Pipeline);

        }

        [Fact]
        public void Should_register_a_type_dependent_pipeline()
        {
            //Arrange
            var services = Mock.Of<IServiceCollection>();
            var provider = Mock.Of<IServiceProvider>();
            PipelineDelegate<MessagingContext> mockMiddlewareFunc = (ctx, ct) => Task.CompletedTask;
            Func<PipelineDelegate<MessagingContext>, PipelineDelegate<MessagingContext>> mockMiddleware = next => mockMiddlewareFunc;

            //Act
            var builder = new MessagingHostConfigurationBuilder(provider, services);
            builder
                .AddSubscriberServices(cfg => cfg
                    .AddType<CommandMessage>()
                    .AddType<EventMessage>()
                    .FromTopic("OtherTopicName"))
                .WithDefaultOptions()
                .UsePipeline((t, p) => p
                    .When(t == typeof(CommandMessage), p => p.Use(mockMiddleware)));
            var config = builder.Build();

            //Assert
            config.Subscribers.Should().HaveCount(3);
            config.Subscribers[0].MessageType.Should().Be(typeof(CommandMessage));
            config.Subscribers[0].Pipeline.Should().Be(mockMiddlewareFunc);

            config.Subscribers[1].Pipeline.Should().NotBe(mockMiddlewareFunc);
            config.Subscribers[2].Pipeline.Should().NotBe(mockMiddlewareFunc);
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
