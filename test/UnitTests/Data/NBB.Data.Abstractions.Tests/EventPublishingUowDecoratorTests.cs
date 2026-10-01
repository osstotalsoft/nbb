// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NBB.Core.Abstractions;
using Xunit;

namespace NBB.Data.Abstractions.Tests
{
    public class EventPublishingUowDecoratorTests
    {
        [Fact]
        public async Task Should_publish_the_uncommitted_changes_of_all_entities_after_saving()
        {
            //Arrange
            var calls = new List<string>();
            var event1 = new object();
            var event2 = new object();
            var event3 = new object();
            var inner = new Mock<IUow<IEventedEntity>>();
            inner.Setup(x => x.GetChanges()).Returns(
            [
                Mock.Of<IEventedEntity>(e => e.GetUncommittedChanges() == new[] { event1, event2 }),
                Mock.Of<IEventedEntity>(e => e.GetUncommittedChanges() == new[] { event3 })
            ]);
            inner.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).Callback(() => calls.Add("save")).Returns(Task.CompletedTask);
            IEnumerable<object> published = null;
            var publisher = new Mock<IEventPublisher>();
            publisher.Setup(x => x.PublishAsync(It.IsAny<IEnumerable<object>>(), It.IsAny<CancellationToken>()))
                .Callback((IEnumerable<object> events, CancellationToken _) => { calls.Add("publish"); published = events; })
                .Returns(Task.CompletedTask);
            var sut = new EventPublishingUowDecorator<IEventedEntity>(inner.Object, publisher.Object);

            //Act
            await sut.SaveChangesAsync();

            //Assert
            published.Should().Equal(event1, event2, event3);
            calls.Should().Equal("save", "publish");
        }

        [Fact]
        public async Task Should_not_publish_when_saving_fails()
        {
            //Arrange
            var inner = new Mock<IUow<IEventedEntity>>();
            inner.Setup(x => x.GetChanges()).Returns([Mock.Of<IEventedEntity>(e => e.GetUncommittedChanges() == new[] { new object() })]);
            inner.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException());
            var publisher = new Mock<IEventPublisher>();
            var sut = new EventPublishingUowDecorator<IEventedEntity>(inner.Object, publisher.Object);

            //Act
            var act = () => sut.SaveChangesAsync();

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            publisher.Verify(x => x.PublishAsync(It.IsAny<IEnumerable<object>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void Should_delegate_GetChanges_to_the_inner_unit_of_work()
        {
            var entities = new[] { Mock.Of<IEventedEntity>() };
            var inner = Mock.Of<IUow<IEventedEntity>>(x => x.GetChanges() == entities);
            var sut = new EventPublishingUowDecorator<IEventedEntity>(inner, Mock.Of<IEventPublisher>());

            sut.GetChanges().Should().BeSameAs(entities);
        }
    }
}
