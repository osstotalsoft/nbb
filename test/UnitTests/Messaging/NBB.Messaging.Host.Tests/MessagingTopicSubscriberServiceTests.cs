﻿using Microsoft.Extensions.DependencyInjection;
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
    public class MessagingTopicSubscriberServiceTests
    {
        [Fact]
        public async Task Should_execute_pipeline_on_message_received()
        {
            //Arrange
            var message = new TestMessage();
            var envelope = new MessagingEnvelope<TestMessage>(new Dictionary<string, string>(), message);
            var pipeline = Mock.Of<PipelineDelegate<MessagingEnvelope>>();
            Func<string, Task> messageBusSubscriberCallback = null;
            var cancellationToken = new CancellationToken();

            var mockedMessageBusSubscriber = Mock.Of<IMessagingTopicSubscriber>();
            Mock.Get(mockedMessageBusSubscriber)
                .Setup(x => x.SubscribeAsync(It.IsAny<string>(), It.IsAny<Func<string, Task>>(), It.IsAny<CancellationToken>(), It.IsAny<MessagingSubscriberOptions>()))
                .Callback((string topic, Func<string, Task> handler, CancellationToken token,  MessagingSubscriberOptions options) => {
                    messageBusSubscriberCallback = handler;
                    cancellationToken = token;
                })
                .Returns(Task.CompletedTask);

            var mockedServiceProvider = Mock.Of<IServiceProvider>(sp =>
                    sp.GetService(typeof(IServiceScopeFactory)) == Mock.Of<IServiceScopeFactory>(ssf =>
                        ssf.CreateScope() == Mock.Of<IServiceScope>(ss =>
                            ss.ServiceProvider == Mock.Of<IServiceProvider>(ssp =>
                                (PipelineDelegate<MessagingEnvelope>) ssp.GetService(typeof(PipelineDelegate<MessagingEnvelope>)) == pipeline))));

            var mockedSerDes = Mock.Of<IMessageSerDes>(x =>
                x.DeserializeMessageEnvelope(It.IsAny<string>(), It.IsAny<MessageSerDesOptions>()) == envelope);

            var messageBusSubscriberService = new MessagingTopicSubscriberService(
                    "topicName",
                    mockedSerDes,
                    mockedMessageBusSubscriber,
                    mockedServiceProvider,
                    Mock.Of<MessagingContextAccessor>(),
                    Mock.Of<ITopicRegistry>(),
                    Mock.Of<ILogger<MessagingTopicSubscriberService>>(),
                    new MessagingSubscriberOptions()
                );

            //Act     
            await messageBusSubscriberService.StartAsync(cancellationToken);
            await messageBusSubscriberCallback("serializedEnvelope");
            await messageBusSubscriberService.StopAsync(cancellationToken);

            //Assert
            Mock.Get(pipeline).Verify(x => x(envelope, cancellationToken));
        }


        public class TestMessage
        {
           
        }
    }
}
