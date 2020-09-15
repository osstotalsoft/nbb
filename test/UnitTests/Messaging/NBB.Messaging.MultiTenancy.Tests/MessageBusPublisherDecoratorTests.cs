using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Context;
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
            var tenantContext = new TenantContext(new Tenant(new Guid(), string.Empty, false));
            var tenantContextAccessor = Mock.Of<ITenantContextAccessor>(x => x.TenantContext == tenantContext);
            var sut = new MultiTenancyMessageBusPublisherDecorator(publisherMock.Object, tenantContextAccessor);
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
            var tenantContextAccessor = Mock.Of<ITenantContextAccessor>(x => x.TenantContext == new TenantContext(new Tenant(tenantId, string.Empty, false)));
            
            void EnvelopeCustomizer(MessagingEnvelope envelope) => publishedEnvelope = envelope;
            var sut = new MultiTenancyMessageBusPublisherDecorator(new MockMessageBusPublisher(), tenantContextAccessor);

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
