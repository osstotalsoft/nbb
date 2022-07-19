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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Messaging.Host.Tests;

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
        var messageHost = new MessagingHost(Mock.Of<ILogger<MessagingHost>>(), new[] { configurator }, mockedServiceProvider,
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
        var retryCount = 1;
        var hostOptions = Mock.Of<IOptions<MessagingHostOptions>>(x => x.Value == new MessagingHostOptions { StartRetryCount = retryCount });
        var configurator = new DelegateMessagingHostStartup(config =>
            config.AddSubscriberServices(s => s.FromTopic("TestTopic")).WithDefaultOptions().UsePipeline(p => { }));

        var mockedMessageBus = Mock.Of<IMessageBus>();
        Mock.Get(mockedMessageBus)
            .Setup(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
                It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()))
            .Throws(new Exception("Retry"));

        var mockedServiceProvider = GetMockedServiceProvider(mockedMessageBus);
        var messageHost = new MessagingHost(Mock.Of<ILogger<MessagingHost>>(), new[] { configurator }, mockedServiceProvider,
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
            .ReturnsAsync(Mock.Of<IDisposable>());

        var mockedServiceProvider = GetMockedServiceProvider(mockedMessageBus);
        var mockedTransportMonitor = Mock.Of<ITransportMonitor>();
        var messageHost = new MessagingHost(Mock.Of<ILogger<MessagingHost>>(), new[] { configurator }, mockedServiceProvider,
            Mock.Of<IServiceCollection>(), Mock.Of<IHostApplicationLifetime>(), mockedTransportMonitor, hostOptions);

        //Act     
        await messageHost.StartAsync();
        Mock.Get(mockedTransportMonitor).Raise(x => x.OnError += _ => { }, new Exception("TransportError"));

        await Task.Delay(100);

        //Assert
        Mock.Get(mockedMessageBus).Verify(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
                It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Shoud_reject_subsequent_starts_When_starting()
    {
        //Arrange
        var hostOptions = Mock.Of<IOptions<MessagingHostOptions>>(x => x.Value == new MessagingHostOptions());
        var configurator = new DelegateMessagingHostStartup(config =>
            config.AddSubscriberServices(s => s.FromTopic("TestTopic")).WithDefaultOptions().UsePipeline(p => { }));

        var mockedMessageBus = Mock.Of<IMessageBus>();
        Mock.Get(mockedMessageBus)
            .Setup(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
                It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(10); // Subscribe takes some time
                return Mock.Of<IDisposable>();
            });

        var mockedServiceProvider = GetMockedServiceProvider(mockedMessageBus);
        var mockLogger = Mock.Of<ILogger<MessagingHost>>();
        var messageHost = new MessagingHost(mockLogger, new[] { configurator }, mockedServiceProvider,
            Mock.Of<IServiceCollection>(), Mock.Of<IHostApplicationLifetime>(), Mock.Of<ITransportMonitor>(), hostOptions);

        var startAttempts = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() => messageHost.StartAsync()));

        //Act     
        await Task.WhenAll(startAttempts.ToArray());

        //Assert
        Mock.Get(mockLogger).VerifyLogInformationWasCalled("Messaging host is starting", Times.Once(), "Expected messaging host to start once");
    }

    [Fact]
    public async Task Shoud_reject_subsequent_stops_When_stopping()
    {
        //Arrange
        var hostOptions = Mock.Of<IOptions<MessagingHostOptions>>(x => x.Value == new MessagingHostOptions());
        var configurator = new DelegateMessagingHostStartup(config =>
            config.AddSubscriberServices(s => s.FromTopic("TestTopic")).WithDefaultOptions().UsePipeline(p => { }));

        var mockedMessageBus = Mock.Of<IMessageBus>();
        Mock.Get(mockedMessageBus)
            .Setup(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
                It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
            {
                var mockedSubscription = Mock.Of<IDisposable>();
                Mock.Get(mockedSubscription)
                    .Setup(x => x.Dispose()).Callback(() => { Thread.Sleep(10); }); //UnSubScribe takes some time
                return mockedSubscription;
            });

        var mockedServiceProvider = GetMockedServiceProvider(mockedMessageBus);
        var mockLogger = Mock.Of<ILogger<MessagingHost>>();
        var messageHost = new MessagingHost(mockLogger, new[] { configurator }, mockedServiceProvider,
            Mock.Of<IServiceCollection>(), Mock.Of<IHostApplicationLifetime>(), Mock.Of<ITransportMonitor>(), hostOptions);

        //Act     
        await messageHost.StartAsync();

        var rand = new Random();
        var stops = Enumerable.Range(0, 100).Select(async _ =>
        {
            await Task.Delay(rand.Next(0, 5));
            await messageHost.StopAsync();
        });
        await Task.WhenAll(stops.ToArray());

        //Assert
        Mock.Get(mockLogger).VerifyLogInformationWasCalled("Messaging host is stopping", Times.Once(), "Expected messaging host to stop once");
    }

    [Fact]
    public async Task Shoud_reject_subsequent_restarts_When_restart_scheduled()
    {
        //Arrange
        var hostOptions = Mock.Of<IOptions<MessagingHostOptions>>(x => x.Value == new MessagingHostOptions { RestartDelaySeconds = 0});
        var configurator = new DelegateMessagingHostStartup(config =>
            config.AddSubscriberServices(s => s.FromTopic("TestTopic")).WithDefaultOptions().UsePipeline(p => { }));

        var mockedMessageBus = Mock.Of<IMessageBus>();
       

        var mockedServiceProvider = GetMockedServiceProvider(mockedMessageBus);
        var mockLogger = Mock.Of<ILogger<MessagingHost>>();
        var messageHost = new MessagingHost(mockLogger, new[] { configurator }, mockedServiceProvider,
            Mock.Of<IServiceCollection>(), Mock.Of<IHostApplicationLifetime>(), Mock.Of<ITransportMonitor>(), hostOptions);

        Mock.Get(mockedMessageBus)
           .Setup(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
               It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()))
           .Returns(async () =>
           {
               //subscribe takes some time
               await Task.Delay(10);

               return Mock.Of<IDisposable>();
           });

        //Act
        var restartAttempts = Enumerable.Range(0, 100)
               .Select(_ => Task.Run(() => messageHost.ScheduleRestart()));
        await Task.WhenAll(restartAttempts);


        await Task.Delay(10);

        //Assert
        Mock.Get(mockLogger).VerifyLogInformationWasCalled("Messaging host is scheduled for restart in 0 seconds", Times.Exactly(1), "Messaging host expected to re-start once");
    }

    [Fact]
    public async Task Shoud_restart_even_if_stop_fails()
    {
        //Arrange
        var hostOptions = Mock.Of<IOptions<MessagingHostOptions>>(x => x.Value == new MessagingHostOptions());
        var configurator = new DelegateMessagingHostStartup(config =>
            config.AddSubscriberServices(s => s.FromTopic("TestTopic")).WithDefaultOptions().UsePipeline(p => { }));

        var mockedMessageBus = Mock.Of<IMessageBus>();
        Mock.Get(mockedMessageBus)
            .Setup(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
                It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                var subscription = Mock.Of<IDisposable>();
                Mock.Get(subscription).Setup(x => x.Dispose()).Throws(new Exception("Unsubscribe error"));
                return Task.FromResult(subscription);
            });
        var mockLogger = Mock.Of<ILogger<MessagingHost>>();
        var mockedServiceProvider = GetMockedServiceProvider(mockedMessageBus);
        var messageHost = new MessagingHost(mockLogger, new[] { configurator }, mockedServiceProvider,
            Mock.Of<IServiceCollection>(), Mock.Of<IHostApplicationLifetime>(), Mock.Of<ITransportMonitor>(), hostOptions);

        //Act     
        await messageHost.StartAsync();
        messageHost.ScheduleRestart();

        await Task.Delay(100);

        //Assert
        Mock.Get(mockLogger).VerifyLogInformationWasCalled("Messaging host is starting", Times.Exactly(2), "Messaging host expected to re-start");
    }

    [Fact]
    public async Task Start_Stop_Start_should_work()
    {
        //Arrange
        var hostOptions = Mock.Of<IOptions<MessagingHostOptions>>(x => x.Value == new MessagingHostOptions
        {
            RestartDelaySeconds = 0,
            StartRetryCount = 0,
        });
        var configurator = new DelegateMessagingHostStartup(config =>
            config.AddSubscriberServices(s => s.FromTopic("TestTopic")).WithDefaultOptions().UsePipeline(p => { }));

        var mockedMessageBus = Mock.Of<IMessageBus>();
        Mock.Get(mockedMessageBus)
            .Setup(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<object>, Task>>(),
                It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<IDisposable>());

        var mockLogger = Mock.Of<ILogger<MessagingHost>>();
        var mockedServiceProvider = GetMockedServiceProvider(mockedMessageBus);
        var messageHost = new MessagingHost(mockLogger, new[] { configurator }, mockedServiceProvider,
            Mock.Of<IServiceCollection>(), Mock.Of<IHostApplicationLifetime>(), Mock.Of<ITransportMonitor>(), hostOptions);

        //Act     
        await messageHost.StartAsync();
        await messageHost.StopAsync();
        await messageHost.StartAsync();

        //Assert
        Mock.Get(mockLogger).VerifyLogInformationWasCalled("Messaging host has started", Times.Exactly(2), "Messaging host expected to start twice");
    }


    private IServiceProvider GetMockedServiceProvider(IMessageBus mockedMessageBus)
    {
        return Mock.Of<IServiceProvider>(sp =>
            sp.GetService(typeof(IMessageBus)) == mockedMessageBus &&
            sp.GetService(typeof(IServiceProvider)) == Mock.Of<IServiceProvider>() &&
            sp.GetService(typeof(MessagingContextAccessor)) == Mock.Of<MessagingContextAccessor>() &&
            sp.GetService(typeof(ILogger<MessagingHost>)) == Mock.Of<ILogger<MessagingHost>>() &&
            sp.GetService(typeof(ITopicRegistry)) == Mock.Of<ITopicRegistry>() &&
            sp.GetService(typeof(ExecutionMonitor)) == new ExecutionMonitor());
    }
}

public static class LoggerExtensions
{
    public static Mock<ILogger<T>> VerifyLogInformationWasCalled<T>(this Mock<ILogger<T>> logger, string expectedMessage, Times times, string failMessage = null)
    {
        Func<object, Type, bool> state = (v, t) => v.ToString().CompareTo(expectedMessage) == 0;

        logger.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => state(v, t)),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)), times, failMessage);

        return logger;
    }
}
