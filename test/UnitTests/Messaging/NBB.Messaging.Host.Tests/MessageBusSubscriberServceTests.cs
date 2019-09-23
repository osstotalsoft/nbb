using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Messaging.Host.Tests
{
    public class MessageBusSubscriberServceTests
    {
        [Fact]
        public async Task Should_execute_pipeline_on_message_received()
        {
            //Arrange
            var message = new TestMessage();
            var envelope = new MessagingEnvelope<TestMessage>(new Dictionary<string, string>(), message);
            var pipeline = Mock.Of<PipelineDelegate<MessagingEnvelope>>();
            Func<MessagingEnvelope<TestMessage>, Task> messageBusSubscriberCallback = null;
            var cancellationToken = new CancellationToken();
            IDisposable subscription = new Subscription(new InvocationHandler(), new List<InvocationHandler>());

            var mockedMessageBusSubscriber = Mock.Of<IMessageBusSubscriber>();
            Mock.Get(mockedMessageBusSubscriber)
                .Setup(x => x.SubscribeAsync(typeof(TestMessage), It.IsAny<Func<MessagingEnvelope, Task>>(), It.IsAny<CancellationToken>(), 
                    It.IsAny<string>(), It.IsAny<MessagingSubscriberOptions>())
                )
                .Callback((Type messageType, Func<MessagingEnvelope, Task> handler, CancellationToken token, string topicName, MessagingSubscriberOptions options) =>
                {
                    messageBusSubscriberCallback = handler;
                    cancellationToken = token;
                })
                .Returns(Task.FromResult(subscription));

            var mockedServiceProvider = Mock.Of<IServiceProvider>(sp =>
                sp.GetService(typeof(IServiceScopeFactory)) == Mock.Of<IServiceScopeFactory>(ssf =>
                    ssf.CreateScope() == Mock.Of<IServiceScope>(ss =>
                        ss.ServiceProvider == Mock.Of<IServiceProvider>(ssp =>
                            (PipelineDelegate<MessagingEnvelope>) ssp.GetService(typeof(PipelineDelegate<MessagingEnvelope>)) == pipeline))));

            var messageBusSubscriberService = new MessageBusSubscriberService<TestMessage>(
                mockedMessageBusSubscriber,
                mockedServiceProvider,
                Mock.Of<MessagingContextAccessor>(),
                Mock.Of<ILogger<MessageBusSubscriberService<TestMessage>>>(),
                new MessagingSubscriberOptions()
            );

            //Act     
            await messageBusSubscriberService.StartAsync(cancellationToken);
            await messageBusSubscriberCallback(envelope);
            await messageBusSubscriberService.StopAsync(cancellationToken);

            //Assert
            Mock.Get(pipeline).Verify(x => x(envelope, cancellationToken));
        }


        public class TestMessage
        {

        }
    }
}