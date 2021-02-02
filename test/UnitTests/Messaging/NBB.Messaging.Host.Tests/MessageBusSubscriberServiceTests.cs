using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
    public class MessageBusSubscriberServiceTests
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

            var mockedMessageBusSubscriber = Mock.Of<IMessageBus>();
            Mock.Get(mockedMessageBusSubscriber)
                .Setup(x => x.SubscribeAsync(It.IsAny<Func<MessagingEnvelope<TestMessage>, Task>>(), It.IsAny<MessagingSubscriberOptions>(), It.IsAny<CancellationToken>()))
                .Callback((Func<MessagingEnvelope<TestMessage>, Task> handler, MessagingSubscriberOptions _,CancellationToken token) => {
                    messageBusSubscriberCallback = handler;
                    cancellationToken = token;
                })
                .Returns(Task.FromResult(Mock.Of<IDisposable>()));

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
                    Mock.Of<ITopicRegistry>(),
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
