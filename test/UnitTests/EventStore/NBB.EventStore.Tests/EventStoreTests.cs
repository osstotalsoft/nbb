using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
using NBB.Core.Abstractions;
using NBB.EventStore.Internal;
using Xunit;

namespace NBB.EventStore.Tests
{
    public class EventStoreTests
    {
        [Fact]
        public async Task Should_save_events_in_event_repository()
        {
            //Arrange
            var eventRepository = new Mock<IEventRepository>();
            var eventSerDes = new Mock<IEventStoreSerDes>();
            var sut = new NBB.EventStore.EventStore(eventRepository.Object, eventSerDes.Object, Mock.Of<ILogger<NBB.EventStore.EventStore>>());
            var domainEvent = Mock.Of<object>();
            var domainEvents = new List<object> { domainEvent };
            var stream = "stream";

            //Act
            await sut.AppendEventsToStreamAsync(stream, domainEvents, null);

            //Assert
            eventRepository.Verify(er => er.AppendEventsToStreamAsync(stream, It.IsAny<IList<EventDescriptor>>(), null, It.IsAny<CancellationToken>()), Times.Once);
        }

    }
}
