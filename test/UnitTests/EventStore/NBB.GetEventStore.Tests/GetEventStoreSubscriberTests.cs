using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using NBB.Core.Abstractions;
using NBB.GetEventStore.Internal;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.GetEventStore.Tests
{
    public record TestEvent1(
        int SequenceNumber,
        Guid EventId,
        DateTime CreationDate
    );
    public record TestEvent2(
        int SequenceNumber,
        Guid EventId,
        DateTime CreationDate
    );

    public class GetEventStoreSubscriberTests
    {

        //[Fact]
        public async Task Should_Receive_Events_From_Subscription()
        {
            //Arrange
            var serDes = new SerDes();
            var stream1 = "teststream-1";
            var stream2 = "teststream-2";
            var events1 = new List<object>
            {
                new TestEvent1(
                    CreationDate: DateTime.Now,
                    EventId: Guid.NewGuid(),
                    SequenceNumber: 0
                ),
                new TestEvent1(
                    CreationDate: DateTime.Now,
                    EventId: Guid.NewGuid(),
                    SequenceNumber: 1
                )
            };
            var events2 = new List<object>
            {
                new TestEvent2(
                    CreationDate: DateTime.Now,
                    EventId: Guid.NewGuid(),
                    SequenceNumber: 0
                )
            };

            var totalEventsCount = events1.Count + events2.Count;
            var sut = new GetEventStoreSubscriber(serDes, Mock.Of<ILogger<GetEventStoreSubscriber>>());
            var pub = new GetEventStoreClient(serDes, Mock.Of<ILogger<GetEventStoreClient>>());
            await pub.AppendEventsToStreamAsync(stream1, events1, null, CancellationToken.None);
            await pub.AppendEventsToStreamAsync(stream2, events2, null, CancellationToken.None);
            var messagesReceived = 0;
            var cts = new CancellationTokenSource();
            //Act
            await sut.SubscribeToAllAsync(message =>
            {
                messagesReceived++;
                if (messagesReceived >= totalEventsCount)
                    cts.Cancel();
                return Task.CompletedTask;
            }, cts.Token);

            //Assert
            messagesReceived.Should().Be(totalEventsCount);
        }
    }
}
