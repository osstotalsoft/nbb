// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host.Internal;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Messaging.Host.Tests
{
    public class MessagingHostTests
    {
        [Fact]
        public async Task Should_subscribe_on_start_and_unsubscribe_on_stop()
        {
            //Arrange
            var hostOptions = Mock.Of<IOptions<MessagingHostOptions>>(x => x.Value == new MessagingHostOptions());
            var configurator = new DelegateMessagingHostStartup(config =>
                config.AddSubscriberServices(s => s.FromTopic("TestTopic")).WithDefaultOptions().UsePipeline(p => { }));

            var mockedMessageBus = Mock.Of<IMessageBus>();
            var mockedSubscription = Mock.Of<IDisposable>();
            Mock.Get(mockedMessageBus)
                .Setup(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
                    It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(mockedSubscription));

            var mockedServiceProvider = GetMockedServiceProvider(mockedMessageBus);
            var messageHost = new MessagingHost(new MockLogger(), new[] { configurator }, mockedServiceProvider,
                Mock.Of<IServiceCollection>(), Mock.Of<IHostApplicationLifetime>(), Mock.Of<ITransportMonitor>(), hostOptions);

            //Act     
            await messageHost.StartAsync();
            await messageHost.StopAsync();

            //Assert
            Mock.Get(mockedMessageBus).Verify(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
                    It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()), Times.Once);
            Mock.Get(mockedSubscription).Verify(x => x.Dispose(), Times.Once);
        }

        [Fact]
        public async Task Shoud_retry_start_failure()
        {
            //Arrange
            var retryCount = 3;
            var hostOptions = Mock.Of<IOptions<MessagingHostOptions>>(x => x.Value == new MessagingHostOptions { StartRetryCount = retryCount });
            var configurator = new DelegateMessagingHostStartup(config =>
                config.AddSubscriberServices(s => s.FromTopic("TestTopic")).WithDefaultOptions().UsePipeline(p => { }));

            var mockedMessageBus = Mock.Of<IMessageBus>();
            Mock.Get(mockedMessageBus)
                .Setup(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
                    It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()))
                .Throws(new Exception("Retry"));

            var mockedServiceProvider = GetMockedServiceProvider(mockedMessageBus);
            var messageHost = new MessagingHost(new MockLogger(), new[] { configurator }, mockedServiceProvider,
                Mock.Of<IServiceCollection>(), Mock.Of<IHostApplicationLifetime>(), Mock.Of<ITransportMonitor>(), hostOptions);

            //Act     
            await messageHost.StartAsync();

            //Assert
            Mock.Get(mockedMessageBus).Verify(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
                    It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(1 + retryCount));
        }

        [Fact]
        public async Task Shoud_restart_on_transport_error()
        {
            //Arrange
            var hostOptions = Mock.Of<IOptions<MessagingHostOptions>>(x => x.Value ==
                new MessagingHostOptions { TransportErrorStrategy = TransportErrorStrategy.Retry, RestartDelaySeconds = 0 });
            var configurator = new DelegateMessagingHostStartup(config =>
                config.AddSubscriberServices(s => s.FromTopic("TestTopic")).WithDefaultOptions().UsePipeline(p => { }));

            var mockedMessageBus = Mock.Of<IMessageBus>();
            Mock.Get(mockedMessageBus)
                .Setup(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
                    It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()))
                .Returns(Task.FromResult(Mock.Of<IDisposable>()));

            var mockedServiceProvider = GetMockedServiceProvider(mockedMessageBus);
            var mockedTransportMonitor = Mock.Of<ITransportMonitor>();
            var messageHost = new MessagingHost(new MockLogger(), new[] { configurator }, mockedServiceProvider,
                Mock.Of<IServiceCollection>(), Mock.Of<IHostApplicationLifetime>(), mockedTransportMonitor, hostOptions);

            //Act     
            await messageHost.StartAsync();
            Mock.Get(mockedTransportMonitor).Raise(x => x.OnError += _ => { }, new Exception("TransportError"));

            await Task.Delay(100);

            //Assert
            Mock.Get(mockedMessageBus).Verify(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
                    It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
        }

        private IServiceProvider GetMockedServiceProvider(IMessageBus mockedMessageBus)
        {
            return Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(IMessageBus)) == mockedMessageBus &&
                sp.GetService(typeof(IServiceProvider)) == Mock.Of<IServiceProvider>() &&
                sp.GetService(typeof(MessagingContextAccessor)) == Mock.Of<MessagingContextAccessor>() &&
                sp.GetService(typeof(ILogger<MessagingHost>)) == new MockLogger() &&
                sp.GetService(typeof(ITopicRegistry)) == Mock.Of<ITopicRegistry>() &&
                sp.GetService(typeof(ExecutionMonitor)) == new ExecutionMonitor());
        }

        public class MockLogger : ILogger<MessagingHost>
        {
            public IDisposable BeginScope<TState>(TState state) => throw new NotImplementedException();

            public bool IsEnabled(LogLevel logLevel) => false;

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
            }
        }
    }
}
