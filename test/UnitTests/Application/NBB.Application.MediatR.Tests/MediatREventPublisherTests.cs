// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NBB.Core.Abstractions;
using Xunit;

namespace NBB.Application.MediatR.Tests
{
    public class MediatREventPublisherTests
    {
        [Fact]
        public async Task Should_publish_only_mediatr_notifications_in_order()
        {
            //Arrange
            var published = new List<object>();
            var publisher = new Mock<IPublisher>();
            publisher.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Callback((object n, CancellationToken _) => published.Add(n))
                .Returns(Task.CompletedTask);
            var sut = new MediatREventPublisher(publisher.Object);
            var e1 = new TestEvent(1);
            var e2 = new TestEvent(2);

            //Act
            await sut.PublishAsync([e1, new PlainEvent(), e2]);

            //Assert
            published.Should().Equal(e1, e2);
        }

        [Fact]
        public void AddMediatRIntegration_should_register_the_event_publisher()
        {
            var services = new ServiceCollection();
            services.AddSingleton(Mock.Of<IPublisher>());

            services.AddMediatRIntegration();

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            scope.ServiceProvider.GetRequiredService<IEventPublisher>().Should().BeOfType<MediatREventPublisher>();
        }

#pragma warning disable CS0618 // MediatorUowDecorator is obsolete, kept for NBB 10 compatibility
        [Fact]
        public async Task Obsolete_MediatorUowDecorator_should_publish_notifications_after_save()
        {
            //Arrange
            var calls = new List<string>();
            var evt = new TestEvent(1);
            var inner = new Mock<IUow<IEventedEntity>>();
            inner.Setup(x => x.GetChanges()).Returns([Mock.Of<IEventedEntity>(e => e.GetUncommittedChanges() == new object[] { evt, new PlainEvent() })]);
            inner.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Callback(() => calls.Add("save")).Returns(Task.CompletedTask);
            var mediator = new Mock<IMediator>();
            mediator.Setup(x => x.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()))
                .Callback(() => calls.Add("publish"))
                .Returns(Task.CompletedTask);
            var sut = new MediatorUowDecorator<IEventedEntity>(inner.Object, mediator.Object);

            //Act
            await sut.SaveChangesAsync();

            //Assert
            calls.Should().Equal("save", "publish");
            mediator.Verify(x => x.Publish<INotification>(evt, It.IsAny<CancellationToken>()), Times.Once);
        }
#pragma warning restore CS0618

        public record TestEvent(int Id) : INotification;

        public record PlainEvent;
    }
}
