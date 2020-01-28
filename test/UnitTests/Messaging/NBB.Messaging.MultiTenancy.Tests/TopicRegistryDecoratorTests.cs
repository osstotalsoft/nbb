using System;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using NBB.Messaging.Abstractions;
using NBB.MultiTenancy.Abstractions.Options;
using NBB.MultiTenancy.Abstractions.Services;
using Xunit;

namespace NBB.Messaging.MultiTenancy.Tests
{
    public class TopicRegistryDecoratorTests
    {
        [Fact]
        public void ShouldCallInnerRegistry()
        {
            // Arrange
            var topicRegistryMock = new Mock<ITopicRegistry>();
            var options = Options.Create(new TenancyOptions
                {TenancyContextType = TenancyContextType.MultiTenant});
            var sut = new MultiTenancyTopicRegistryDecorator(topicRegistryMock.Object, options);
            const string topicName = "test";

            // Act
            sut.GetTopicForMessageType(typeof(string));
            sut.GetTopicForName(topicName);
            sut.GetTopicPrefix();

            // Assert
            topicRegistryMock.Verify(
                topicRegistry => topicRegistry.GetTopicForMessageType(typeof(string), It.IsAny<bool>()), Times.Once);

            topicRegistryMock.Verify(
                topicRegistry => topicRegistry.GetTopicForName(topicName, It.IsAny<bool>()), Times.Once);

            topicRegistryMock.Verify(
                topicRegistry => topicRegistry.GetTopicPrefix(), Times.Once);
        }

        [Fact]
        public void ShouldAddTenantIdInTopicName()
        {
            // Arrange
            var topicRegistryMock = new Mock<ITopicRegistry>();
            topicRegistryMock.Setup(x => x.GetTopicForMessageType(typeof(string), It.IsAny<bool>())).Returns("topic");
            var tenantId = Guid.NewGuid();
            var options = Options.Create(new TenancyOptions
                {TenancyContextType = TenancyContextType.MonoTenant, MonoTenantId = tenantId});

            var sut = new MultiTenancyTopicRegistryDecorator(topicRegistryMock.Object,  options);

            // Act
            var topic = sut.GetTopicForMessageType(typeof(string));

            // Assert
            topic.Should().Contain(tenantId.ToString());
        }
    }
}
