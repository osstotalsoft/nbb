// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using NBB.Messaging.Host.Internal;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Messaging.Host.Tests
{
    public class HostedSubscriberTests
    {
        [Fact]
        public async Task Should_execute_pipeline_on_message_received()
        {
            //Arrange
            var message = new TestMessage();
            var envelope = new MessagingEnvelope<TestMessage>(new Dictionary<string, string>(), message);
            var pipeline = Mock.Of<PipelineDelegate<MessagingContext>>();
            Func<MessagingEnvelope<TestMessage>, Task> messageBusSubscriberCallback = null;
            var cancellationToken = new CancellationToken();

            var mockedMessageBusSubscriber = Mock.Of<IMessageBus>();
            Mock.Get(mockedMessageBusSubscriber)
                .Setup(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<TestMessage>, Task>>(),
                    It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()))
                .Callback((Func<MessagingEnvelope<TestMessage>, Task> handler, MessagingSubscriberOptions _,
                    CancellationToken token) =>
                {
                    messageBusSubscriberCallback = handler;
                    cancellationToken = token;
                })
                .Returns(Task.FromResult(Mock.Of<IDisposable>()));

            var mockedServiceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(IServiceScopeFactory)) == Mock.Of<IServiceScopeFactory>(ssf =>
                    ssf.CreateScope() == Mock.Of<IServiceScope>()));

            var messageBusSubscriberService = new HostedSubscriber<TestMessage>(
                mockedMessageBusSubscriber,
                mockedServiceProvider,
                Mock.Of<MessagingContextAccessor>(),
                new MockLogger(),
                Mock.Of<ITopicRegistry>(),
                new ExecutionMonitor()
            );

            //Act     
            using var _ = await messageBusSubscriberService.SubscribeAsync(pipeline, new MessagingSubscriberOptions(),cancellationToken);
            await messageBusSubscriberCallback(envelope);

            //Assert
            Mock.Get(pipeline).Verify(x =>
                x(It.Is<MessagingContext>(ctx => ctx.MessagingEnvelope == envelope), cancellationToken));
        }


        public class TestMessage
        {
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
