using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Effects;
using Xunit;
using Moq;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Effects.Tests
{
    public class MessagingEffectsTest
    {
        [Fact]
        public void AddMessagingEffectsShouldRegisterMessagingSideEffectHandler()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IMessageBusPublisher>());

            //Act
            services.AddMessagingEffects();

            //Assert
            using var container = services.BuildServiceProvider();
            var handler = container.GetService<ISideEffectHandler<PublishMessage.SideEffect, Unit>>();
            handler.Should().NotBeNull();
        }


        [Fact]
        public async Task PublishMessageSideEffectHandler_should_call_IMessageBusPublisher_Publish_with_message_runtime_type()
        {
            //Arrange
            object stringMessage = "some msg";
            object intMessage = 1;
            var messageBusPublisher = new Mock<IMessageBusPublisher>();
            var sut = new PublishMessage.Handler(messageBusPublisher.Object);

            //Act
            await sut.Handle(new PublishMessage.SideEffect(stringMessage));
            await sut.Handle(new PublishMessage.SideEffect(intMessage));

            //Assert
            messageBusPublisher.Verify(mock => mock.PublishAsync((string)stringMessage, It.IsAny<CancellationToken>(), null, null), Times.Once);
            messageBusPublisher.Verify(mock => mock.PublishAsync((int)intMessage, It.IsAny<CancellationToken>(), null, null), Times.Once);
        }
    }
}
