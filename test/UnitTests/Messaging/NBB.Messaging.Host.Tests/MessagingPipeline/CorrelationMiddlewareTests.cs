using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NBB.Correlation;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using NBB.Messaging.Host.MessagingPipeline;
using Xunit;

namespace NBB.Messaging.Host.Tests.MessagingPipeline
{
    public class CorrelationMiddlewareTests
    {
        // IntegrationTest (integrates with CorrelationManager)
        [Fact]
        public async void Should_setNewCorrelationId()
        {
            //Arrange
            var correlationMiddleWare = new CorrelationMiddleware();
            var sentMessage = Mock.Of<IMessage>();
            Guid? correlationId = null;
            var envelope = new MessagingEnvelope<IMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);


            Task Next() { correlationId = CorrelationManager.GetCorrelationId(); return Task.CompletedTask; }

            //Act
            await correlationMiddleWare.Invoke(envelope, default, Next);

            //Assert
            correlationId.Should().NotBeNull();
        }

        // IntegrationTest (integrates with CorrelationManager)
        [Fact]
        public async void Should_takeCorrelationIdFromMessage()
        {
            //Arrange
            var correlationMiddleWare = new CorrelationMiddleware();
            var messageCorrelationId = Guid.NewGuid();
            var sentMessage = "Test"; 
            Guid? correlationId = null;
            var envelope = new MessagingEnvelope<string>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);
            envelope.SetHeader(MessagingHeaders.CorrelationId, messageCorrelationId.ToString());
            Task Next() { correlationId = CorrelationManager.GetCorrelationId(); return Task.CompletedTask; }

            //Act
            await correlationMiddleWare.Invoke(envelope, default, Next);

            //Assert
            correlationId.Should().Be(messageCorrelationId);
        }

        [Fact]
        public async void Should_callNextPipelineMiddleware()
        {
            //Arrange
            var correlationMiddleWare = new CorrelationMiddleware();
            var sentMessage = Mock.Of<IMessage>();
            bool isNextMiddlewareCalled = false;
            var envelope = new MessagingEnvelope<IMessage>(new System.Collections.Generic.Dictionary<string, string>(), sentMessage);


            Task Next() { isNextMiddlewareCalled = true; return Task.CompletedTask; }

            //Act
            await correlationMiddleWare.Invoke(envelope, default, Next);

            //Assert
            isNextMiddlewareCalled.Should().BeTrue();
        }
    }
}
