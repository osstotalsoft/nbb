// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Moq;
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
            var sut = new EventStore(eventRepository.Object, eventSerDes.Object, Mock.Of<ILogger<EventStore>>());
            var domainEvent = new GenericEvent<GenericEvent<(int[], string)>>(new GenericEvent<(int[], string)>((new[] { 1, 2 }, "Test")));
            var eventType = "NBB.EventStore.Tests.EventStoreTests+GenericEvent`1[[NBB.EventStore.Tests.EventStoreTests+GenericEvent`1[[System.ValueTuple`2[[System.Int32[], System.Private.CoreLib], [System.String, System.Private.CoreLib]], System.Private.CoreLib]], NBB.EventStore.Tests]], NBB.EventStore.Tests";
            var domainEvents = new List<object> { domainEvent };
            var stream = "stream";

            //Act
            await sut.AppendEventsToStreamAsync(stream, domainEvents, null);

            //Assert
            eventRepository.Verify(er => er.AppendEventsToStreamAsync(stream,
                It.Is<IList<EventDescriptor>>(list => list[0].EventType == eventType),
                null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Should_delete_stream_in_event_repository()
        {
            //Arrange
            var eventRepository = new Mock<IEventRepository>();
            var eventSerDes = new Mock<IEventStoreSerDes>();
            var sut = new EventStore(eventRepository.Object, eventSerDes.Object, Mock.Of<ILogger<EventStore>>());
            var stream = "stream";

            //Act
            await sut.DeleteStreamAsync(stream);

            //Assert
            eventRepository.Verify(er => er.DeleteStreamAsync(stream, It.IsAny<CancellationToken>()), Times.Once);
        }

        public record GenericEvent<T>(T field);

    }
}
