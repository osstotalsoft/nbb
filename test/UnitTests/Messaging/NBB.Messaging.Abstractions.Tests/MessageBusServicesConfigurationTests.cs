// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace NBB.Messaging.Abstractions.Tests
{
    public class MessageBusServicesConfigurationTests
    {
        private class Marker
        {
        }

        private class PublisherDecorator : IMessageBusPublisher
        {
            public Task PublishAsync<T>(T message, MessagingPublisherOptions publisherOptions = null,
                CancellationToken cancellationToken = default) => Task.CompletedTask;
        }

        private static int MarkerCount(IServiceCollection services) =>
            services.Count(d => d.ServiceType == typeof(Marker));

        [Fact]
        public void Should_apply_configuration_when_the_message_bus_is_added_later()
        {
            //Arrange
            var services = new ServiceCollection();

            //Act
            services.ConfigureMessageBusServices(s => s.AddSingleton<Marker>());
            var countBeforeMessageBus = MarkerCount(services);
            services.AddMessageBus();

            //Assert
            countBeforeMessageBus.Should().Be(0);
            MarkerCount(services).Should().Be(1);
        }

        [Fact]
        public void Should_apply_configuration_immediately_when_the_message_bus_is_already_added()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddMessageBus();

            //Act
            services.ConfigureMessageBusServices(s => s.AddSingleton<Marker>());

            //Assert
            MarkerCount(services).Should().Be(1);
        }

        [Fact]
        public void Should_not_apply_configuration_without_a_message_bus()
        {
            //Arrange
            var services = new ServiceCollection();

            //Act
            services.ConfigureMessageBusServices(s => s.AddSingleton<Marker>());

            //Assert
            MarkerCount(services).Should().Be(0);
        }

        [Fact]
        public void Should_apply_deferred_configuration_only_once()
        {
            //Arrange
            var services = new ServiceCollection();
            services.ConfigureMessageBusServices(s => s.AddSingleton<Marker>());

            //Act
            services.AddMessageBus();
            services.AddMessageBus();

            //Assert
            MarkerCount(services).Should().Be(1);
        }

        [Fact]
        public void Should_keep_the_deferred_decoration_when_the_message_bus_is_added_again()
        {
            //Arrange
            var services = new ServiceCollection();
            services.ConfigureMessageBusServices(s =>
                s.Replace(ServiceDescriptor.Singleton<IMessageBusPublisher, PublisherDecorator>()));
            services.AddMessageBus();

            //Act
            services.AddMessageBus();

            //Assert
            services.BuildServiceProvider().GetRequiredService<IMessageBusPublisher>()
                .Should().BeOfType<PublisherDecorator>();
        }
    }
}
