// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host;
using Xunit;

namespace NBB.Messaging.Mediator.Tests
{
    public class MediatorMessagingHostTests
    {
        [Fact]
        public void Should_discover_handled_message_types_from_mediator_registrations()
        {
            var services = new ServiceCollection();
            services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);

            SubscribedTypes(services, s => s.FromMediatorHandledEvents())
                .Should().BeEquivalentTo([typeof(SomethingHappened)]);
            SubscribedTypes(services, s => s.FromMediatorHandledCommands())
                .Should().BeEquivalentTo([typeof(DoSomething), typeof(DoSomethingElse)]);
            SubscribedTypes(services, s => s.FromMediatorHandledQueries())
                .Should().BeEquivalentTo([typeof(GetSomething), typeof(CreateSomething), typeof(QuerySomething)]);
            SubscribedTypes(services, s => s.FromMediatorHandledMessages())
                .Should().HaveCount(6);
        }

        [Fact]
        public async Task Should_dispatch_messages_from_the_bus_to_mediator_handlers()
        {
            //Arrange
            using var host = BuildHost(services => services.AddMediatorIntegration());
            await host.StartAsync();
            var probe = host.Services.GetRequiredService<Probe>();
            var publisher = host.Services.GetRequiredService<IMessageBusPublisher>();

            //Act
            await publisher.PublishAsync(new DoSomething("request"));
            await publisher.PublishAsync(new DoSomethingElse("command"));
            await publisher.PublishAsync(new SomethingHappened("event"));

            //Assert
            (await probe.Request.Task.WaitAsync(TimeSpan.FromSeconds(10))).Should().Be("request");
            (await probe.Command.Task.WaitAsync(TimeSpan.FromSeconds(10))).Should().Be("command");
            (await probe.Event.Task.WaitAsync(TimeSpan.FromSeconds(10))).Should().Be("event");
            await host.StopAsync();
        }

        [Fact]
        public void Should_resolve_nbb4_topics_with_the_mediator_classifier()
        {
            //Arrange
            using var host = BuildHost(services => services.AddMediatorIntegration(), nbb4Topics: true);

            //Act
            var topicRegistry = host.Services.GetRequiredService<ITopicRegistry>();

            //Assert
            topicRegistry.GetTopicForMessageType(typeof(DoSomething), false).Should().StartWith("ch.commands.");
            topicRegistry.GetTopicForMessageType(typeof(DoSomethingElse), false).Should().StartWith("ch.commands.");
            topicRegistry.GetTopicForMessageType(typeof(SomethingHappened), false).Should().StartWith("ch.events.");
            topicRegistry.GetTopicForMessageType(typeof(GetSomething), false).Should().StartWith("ch.messages.");
        }

        [Fact]
        public async Task Should_throw_for_messages_that_are_not_mediator_messages()
        {
            var middleware = new MediatorMiddleware(Mock.Of<global::Mediator.IMediator>());
            var envelope = new MessagingEnvelope(new Dictionary<string, string>(), new PlainMessage("x"));

            var act = () => middleware.Invoke(new MessagingContext(envelope, string.Empty, null), default, () => Task.CompletedTask);

            await act.Should().ThrowAsync<ApplicationException>();
        }

        private static List<Type> SubscribedTypes(IServiceCollection services, Func<ITypeSourceSelector, IImplementationTypeSelector> select)
        {
            var builder = new MessagingHostConfigurationBuilder(Mock.Of<IServiceProvider>(), services);
            builder.AddSubscriberServices(s => select(s).AddAllClasses()).WithDefaultOptions().UsePipeline(_ => { });
            return builder.Build().Subscribers.Select(s => s.MessageType).ToList();
        }

        private static IHost BuildHost(Action<IServiceCollection> configureServices, bool nbb4Topics = false)
        {
            var builder = Microsoft.Extensions.Hosting.Host.CreateApplicationBuilder();
            builder.Configuration.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["Messaging:TopicResolutionCompatibility"] = nbb4Topics ? "NBB_4" : "None"
            });

            var services = builder.Services;
            services.AddSingleton<Probe>();
            services.AddMediator(options => options.ServiceLifetime = ServiceLifetime.Scoped);
            services.AddMessageBus().AddInProcessTransport();
            services.UseTopicResolutionBackwardCompatibility(builder.Configuration);
            configureServices(services);
            services.AddMessagingHost(builder.Configuration, hostBuilder => hostBuilder.Configure(config => config
                .AddSubscriberServices(selector => selector
                    .FromMediatorHandledCommands().AddAllClasses()
                    .FromMediatorHandledEvents().AddAllClasses())
                .WithDefaultOptions()
                .UsePipeline(pipeline => pipeline.UseMediatorMiddleware())));

            return builder.Build();
        }
    }
}
