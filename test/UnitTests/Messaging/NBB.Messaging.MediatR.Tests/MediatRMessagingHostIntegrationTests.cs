// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host;
using Xunit;

namespace NBB.Messaging.MediatR.Tests
{
    public class MediatRMessagingHostIntegrationTests
    {
        [Fact]
        public async Task Should_dispatch_commands_and_events_from_the_bus_to_mediatr_handlers()
        {
            //Arrange
            using var host = BuildHost(services => services.AddMediatRIntegration());
            await host.StartAsync();
            var probe = host.Services.GetRequiredService<Probe>();
            var publisher = host.Services.GetRequiredService<IMessageBusPublisher>();

            //Act
            await publisher.PublishAsync(new DoSomething("cmd"));
            await publisher.PublishAsync(new SomethingHappened("evt"));

            //Assert
            (await probe.Command.Task.WaitAsync(TimeSpan.FromSeconds(10))).Should().Be("cmd");
            (await probe.Event.Task.WaitAsync(TimeSpan.FromSeconds(10))).Should().Be("evt");
            await host.StopAsync();
        }

        [Fact]
        public async Task Should_resolve_nbb4_topics_with_the_mediatr_classifier()
        {
            //Arrange
            using var host = BuildHost(services => services.AddMediatRIntegration(), nbb4Topics: true);

            //Act
            var topicRegistry = host.Services.GetRequiredService<ITopicRegistry>();

            //Assert
            topicRegistry.GetTopicForMessageType(typeof(DoSomething), false).Should().StartWith("ch.commands.");
            topicRegistry.GetTopicForMessageType(typeof(SomethingHappened), false).Should().StartWith("ch.events.");
            topicRegistry.GetTopicForMessageType(typeof(GetSomething), false).Should().StartWith("ch.messages.");

            await host.StartAsync();
            var probe = host.Services.GetRequiredService<Probe>();
            await host.Services.GetRequiredService<IMessageBusPublisher>().PublishAsync(new DoSomething("nbb4"));
            (await probe.Command.Task.WaitAsync(TimeSpan.FromSeconds(10))).Should().Be("nbb4");
            await host.StopAsync();
        }

        [Fact]
        public async Task Should_fail_host_start_when_nbb4_topics_are_used_without_classification()
        {
            //Arrange
            using var host = BuildHost(_ => { }, nbb4Topics: true);

            //Act
            var act = () => host.StartAsync();

            //Assert
            await act.Should().ThrowAsync<Microsoft.Extensions.Options.OptionsValidationException>()
                .WithMessage("*requires an IContractKindClassifier*");
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
            services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<MediatRMessagingHostIntegrationTests>());
            services.AddMessageBus().AddInProcessTransport();
            services.UseTopicResolutionBackwardCompatibility(builder.Configuration);
            configureServices(services);
            services.AddMessagingHost(builder.Configuration, hostBuilder => hostBuilder.Configure(config => config
                .AddSubscriberServices(selector => selector
                    .FromMediatRHandledCommands().AddClassesWhere(t => t == typeof(DoSomething))
                    .FromMediatRHandledEvents().AddClassesWhere(t => t == typeof(SomethingHappened)))
                .WithDefaultOptions()
                .UsePipeline(pipeline => pipeline.UseMediatRMiddleware())));

            return builder.Build();
        }

        public class Probe
        {
            public TaskCompletionSource<string> Command { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
            public TaskCompletionSource<string> Event { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public record DoSomething(string Value) : IRequest;

        public record SomethingHappened(string Value) : INotification;

        public record GetSomething : IRequest<string>;

        public class DoSomethingHandler(Probe probe) : IRequestHandler<DoSomething>
        {
            public Task Handle(DoSomething request, CancellationToken cancellationToken)
            {
                probe.Command.TrySetResult(request.Value);
                return Task.CompletedTask;
            }
        }

        public class SomethingHappenedHandler(Probe probe) : INotificationHandler<SomethingHappened>
        {
            public Task Handle(SomethingHappened notification, CancellationToken cancellationToken)
            {
                probe.Event.TrySetResult(notification.Value);
                return Task.CompletedTask;
            }
        }
    }
}
