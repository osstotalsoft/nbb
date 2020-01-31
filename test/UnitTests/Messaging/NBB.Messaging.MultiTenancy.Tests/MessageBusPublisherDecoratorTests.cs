using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using NBB.MultiTenancy.Abstractions.Services;
using Xunit;

namespace NBB.Messaging.MultiTenancy.Tests
{
    public class MessageBusPublisherDecoratorTests
    {
        [Fact]
        public async void ShouldCallInnerPublisher()
        {
            // Arrange
            var publisherMock = new Mock<IMessageBusPublisher>();
            var sut = new MultiTenancyMessageBusPublisherDecorator(publisherMock.Object, Mock.Of<ITenantService>());
            const string message = "test";

            // Act
            await sut.PublishAsync(message);

            // Assert
            publisherMock.Verify(
                publisher => publisher.PublishAsync(message, default, It.IsAny<Action<MessagingEnvelope>>(), null),
                Times.Once);
        }

        [Fact]
        public async void ShouldSetTenantIdInEnvelopeHeader()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var publishedEnvelope = default(MessagingEnvelope);
            var tenantService = Mock.Of<ITenantService>();
            Mock.Get(tenantService).Setup(x => x.GetTenantIdAsync()).ReturnsAsync(tenantId);
            void EnvelopeCustomizer(MessagingEnvelope envelope) => publishedEnvelope = envelope;
            var sut = new MultiTenancyMessageBusPublisherDecorator(new MockMessageBusPublisher(), tenantService);

            // Act
            await sut.PublishAsync("test", default, EnvelopeCustomizer);

            // Assert
            publishedEnvelope.Headers[MessagingHeaders.TenantId].Should().Be(tenantId.ToString());
        }

        private class MockMessageBusPublisher : IMessageBusPublisher
        {
            public Task PublishAsync<T>(T message, CancellationToken cancellationToken = default,
                Action<MessagingEnvelope> envelopeCustomizer = null,
                string topicName = null)
            {
                envelopeCustomizer?.Invoke(new MessagingEnvelope(new Dictionary<string, string>(), message));
                return Task.CompletedTask;
            }
        }
    }
}
