using System;
using FluentAssertions;
using NBB.Core.Abstractions;
using NBB.Domain.Abstractions;
using Newtonsoft.Json;
using Xunit;

namespace NBB.Domain.Tests
{
    public class SnapshotAggregateRootTests
    {
        private class TestDomainEvent : DomainEvent
        {
            public TestDomainEvent()
                : base(Guid.NewGuid(), null)
            {
            }
        }
        private class TestSnapshotAggregateRoot : EventSourcedAggregateRoot<Guid, TestSnapshot>
        {
            public Guid Id { get; private set; }

            public string Prop { get; private set; }

            public TestSnapshotAggregateRoot()
            {
            }

            [JsonConstructor]
            public TestSnapshotAggregateRoot(Guid id)
            {
                Id = id;
            }


            public override Guid GetIdentityValue() => Id;

           

            private void Apply(TestDomainEvent e)
            {
            }
          
            protected override void SetMemento(TestSnapshot snapshot)
            {
                Prop = snapshot.Prop;
            }

            protected override TestSnapshot CreateMemento()
            {
                return new TestSnapshot {Prop = Prop};
            }
        }

        private class TestSnapshot
        {
            public string Prop { get; set; }
        }

        [Fact]
        public void Should_support_serialization()
        {
            //Arrange
            var sut = new TestSnapshotAggregateRoot(Guid.NewGuid());
            var snapshot = new TestSnapshot();
            ((ISnapshotableEntity)sut).ApplySnapshot(snapshot, 5);

            //Act
            var serializedObject = JsonConvert.SerializeObject(sut);
            var deserializedObject = JsonConvert.DeserializeObject<TestSnapshotAggregateRoot>(serializedObject);

            //Assert
            deserializedObject.Should().Be(sut);
        }

        [Fact]
        public void Should_set_aggregate_version_on_load()
        {
            //Arrange
            var sut = new TestSnapshotAggregateRoot(Guid.NewGuid());
            var snapshot = new TestSnapshot {Prop = "EEE"};
            var snapshotVersion = 5;

            //Act
            ((ISnapshotableEntity)sut).ApplySnapshot(snapshot, snapshotVersion);

            //Assert
            sut.Version.Should().Be(snapshotVersion);
        }

        [Fact]
        public void Should_set_snapshot_version_on_save()
        {
            //Arrange
            var domainEvent = new TestDomainEvent();
            var sut = new TestSnapshotAggregateRoot(Guid.NewGuid());
            sut.LoadFromHistory(new [] {domainEvent, domainEvent});

            //Act
            var (_, snapshotVersion) = ((ISnapshotableEntity)sut).TakeSnapshot();

            //Assert
            sut.Version.Should().Be(snapshotVersion);
        }
    }
}
