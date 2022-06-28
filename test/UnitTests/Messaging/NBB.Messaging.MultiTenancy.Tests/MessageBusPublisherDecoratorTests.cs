// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NBB.Messaging.Abstractions;
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
            var tenantContext = new TenantContext(new Tenant(new Guid(), string.Empty));
            var tenantContextAccessor = Mock.Of<ITenantContextAccessor>(x => x.TenantContext == tenantContext);
            var sut = new MultiTenancyMessageBusPublisherDecorator(publisherMock.Object, tenantContextAccessor);
            const string message = "test";

            // Act
            await sut.PublishAsync(message);

            // Assert
            publisherMock.Verify(
                publisher => publisher.PublishAsync(message,  It.IsAny<MessagingPublisherOptions>(), default),
                Times.Once);
        }

        [Fact]
        public async void ShouldSetTenantIdInEnvelopeHeader()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var publishedEnvelope = default(MessagingEnvelope);
            var tenantContextAccessor = Mock.Of<ITenantContextAccessor>(x => x.TenantContext == new TenantContext(new Tenant(tenantId, string.Empty, true)));
            
            void EnvelopeCustomizer(MessagingEnvelope envelope) => publishedEnvelope = envelope;
            var sut = new MultiTenancyMessageBusPublisherDecorator(new MockMessageBusPublisher(), tenantContextAccessor);

            // Act
            await sut.PublishAsync("test", new MessagingPublisherOptions {EnvelopeCustomizer = EnvelopeCustomizer});

            // Assert
            publishedEnvelope.Headers[MessagingHeaders.TenantId].Should().Be(tenantId.ToString());
        }

        private class MockMessageBusPublisher : IMessageBusPublisher
        {
            public Task PublishAsync<T>(T message, MessagingPublisherOptions options = null,
                CancellationToken cancellationToken = default)
            {
                options?.EnvelopeCustomizer?.Invoke(new MessagingEnvelope(new Dictionary<string, string>(), message));
                return Task.CompletedTask;
            }
        }
    }
}
