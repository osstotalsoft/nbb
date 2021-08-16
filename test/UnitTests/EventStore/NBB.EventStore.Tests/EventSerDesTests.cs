// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using NBB.EventStore.Internal;
using Xunit;

namespace NBB.EventStore.Tests
{
    public class EventSerDesTests
    {
        public record TestEvent(
            long ContractId,
            long PartnerId,
            string Details
        );
        

        [Fact]
        public void Should_deserialize_events_using_the_attributed_private_constructor()
        {
            //Arrange
            var sut = new NewtonsoftJsonEventStoreSerDes();
            var @event = new TestEvent(11232, 1122312, "ceva");
            var json = sut.Serialize(@event);

            //Act
            var deserialized = sut.Deserialize(json, typeof(TestEvent)) as TestEvent;


            //Assert
            deserialized.Should().NotBeNull();
        }
    }
}
