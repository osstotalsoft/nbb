// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace NBB.Domain.Tests
{
    public class EventSourcedAggregateRootTests
    {
        private record TestDomainEvent;
        private class TestEventSourcedAggregateRoot : EventSourcedAggregateRoot<Guid>
        {
            public Guid Id { get; private set; }

            public TestEventSourcedAggregateRoot()
            {
            }

            [JsonConstructor]
            public TestEventSourcedAggregateRoot(Guid id)
            {
                Id = id;
            }

            public bool ApplyWasCalled { get; private set; }

            public override Guid GetIdentityValue() => Id;

            private void Apply(TestDomainEvent e)
            {
                ApplyWasCalled = true;
            }
        }

        [Fact]
        public void Should_invoke_event_specific_apply_methods_when_load_from_history()
        {
            //Arrange
            var domainEvent = new TestDomainEvent();
            var sut = new TestEventSourcedAggregateRoot();

            //Act
            sut.LoadFromHistory(new[] { domainEvent });

            //Assert
            sut.ApplyWasCalled.Should().BeTrue();
        }

        [Fact]
        public void Should_support_serialization()
        {
            //Arrange
            var sut = new TestEventSourcedAggregateRoot(Guid.NewGuid());
            var domainEvent = new TestDomainEvent();
            sut.LoadFromHistory(new[] { domainEvent });

            //Act
            var serializedObject = JsonConvert.SerializeObject(sut);
            var deserializedObject = JsonConvert.DeserializeObject<TestEventSourcedAggregateRoot>(serializedObject);

            //Assert
            deserializedObject.Should().Be(sut);
        }
    }
}
