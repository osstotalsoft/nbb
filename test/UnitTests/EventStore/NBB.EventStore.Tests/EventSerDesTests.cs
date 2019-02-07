using FluentAssertions;
using NBB.Core.Abstractions;
using NBB.EventStore.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using Xunit;

namespace NBB.EventStore.Tests
{
    public class EventSerDesTests
    {
        public class TestEvent : IEvent
        {
            public Guid EventId { get; private set; }
            public Guid? CorrelationId { get; set; }
            public DateTime CreationDate { get; private set; }
            public long ContractId { get; private set; }
            public long PartnerId { get; private set; }
            public string Details { get; private set; }
            public int SequenceNumber { get; set; }

            public Dictionary<string, object> Metadata { get; set; }

            private readonly bool _constructedWithPrivateConstructor;
            public bool ConstructedWithPrivateConstructor() => _constructedWithPrivateConstructor;

            public void SetCorrelationId(Guid correlationId)
            {
                throw new NotImplementedException();
            }

            [JsonConstructor]
            private TestEvent(Guid eventId, DateTime creationDate, long contractId, long partnerId, string details)
            {
                EventId = eventId;
                CreationDate = creationDate;
                ContractId = contractId;
                PartnerId = partnerId;
                Details = details;

                _constructedWithPrivateConstructor = true;
            }

            public TestEvent(long contractId, long partnerId, string details)
            {
            }


        }

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
            deserialized?.ConstructedWithPrivateConstructor().Should().BeTrue();
        }
    }
}
