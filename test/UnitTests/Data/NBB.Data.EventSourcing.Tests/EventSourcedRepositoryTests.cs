using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging;
using Moq;
using NBB.Core.Abstractions;
using NBB.Data.EventSourcing.Infrastructure;
using NBB.Domain.Abstractions;
using NBB.EventStore.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace NBB.Data.EventSourcing.Tests
{
    public class EventSourcedRepositoryTests
    {
        public class TestEventSourcedAggregateRoot : IEventSourcedAggregateRoot<Guid>, ISnapshotableEntity
        {
            public TestEventSourcedAggregateRoot()
            {

            }

            public TestEventSourcedAggregateRoot(Guid id, int version, List<IDomainEvent> domainEvents)
            {
                Id = id;
                Version = version;
                _domainEvents = domainEvents;

            }

            public int Version { get; set; }

            private readonly List<IDomainEvent> _domainEvents;

            public Guid Id { get; }

            public int SnapshotVersion { get; protected set; }

            public virtual int? SnapshotVersionFrequency => null;

            public virtual IEnumerable<IEvent> GetUncommittedChanges() => _domainEvents;

            public void LoadFromHistory(IEnumerable<IDomainEvent> history)
            {
                //throw new NotImplementedException();
            }

            public virtual void MarkChangesAsCommitted()
            {
            }

            public Guid GetIdentityValue() => Id;
            object IIdentifiedEntity.GetIdentityValue() => this.GetIdentityValue();

            public string GetTypeId()
            {
                return "asa";
            }

            public (object snapshot, int snapshotVersion) TakeSnapshot()
            {
                return (null, Version);
            }

            public void ApplySnapshot(object snapshot, int snapshotVersion)
            {
                
            }
        }

        public class TestSnapshotAggregateRoot : TestEventSourcedAggregateRoot, IMementoProvider
        {
            public TestSnapshotAggregateRoot()
            {
            }

            public TestSnapshotAggregateRoot(Guid id, int version, int snapshotVersion, int? snapshotVersionFrequency, List<IDomainEvent> domainEvents)
                : base(id, version, domainEvents)
            {
                SnapshotVersion = snapshotVersion;
                SnapshotVersionFrequency = snapshotVersionFrequency;
            }

            

            public void SetMemento(object snapshot)
            {
            }

            public object CreateMemento()
            {
                return null;
            }

            public override int? SnapshotVersionFrequency { get; } 
        }

        [Fact]
        public async Task Should_save_events_in_event_store_when_aggregate_is_saved()
        {
            //Arrange
            //var eventStoreMock = new Mock<IEventStore>();
            var eventStoreMock = new TestEventStore();
            var sut = new EventSourcedRepository<TestEventSourcedAggregateRoot>(eventStoreMock, Mock.Of<ISnapshotStore>(), Mock.Of<IMediator>(), new EventSourcingOptions(), Mock.Of<ILogger<EventSourcedRepository<TestEventSourcedAggregateRoot>>>());
            var domainEvent = Mock.Of<IDomainEvent>();
            var domainEvents = new List<IDomainEvent> { domainEvent };
            var testAggregate = new TestEventSourcedAggregateRoot(Guid.NewGuid(), 5, domainEvents);

            //Act
            await sut.SaveAsync(testAggregate, CancellationToken.None);

            //Assert
            //eventStoreMock.Verify(es => es.AppendEventsToStreamAsync(It.IsAny<string>(), It.Is<IEnumerable<IDomainEvent>>(de=> de.Single() == domainEvent), null, It.IsAny<CancellationToken>()));
            eventStoreMock.AppendEventsToStreamAsyncCallsCount.Should().Be(1);
        }

        [Fact]
        public async Task Should_save_snapshot_in_snapshot_store_when_aggregate_is_saved()
        {
            //Arrange
            var snapshotStore = Mock.Of<ISnapshotStore>();
            var sut = new EventSourcedRepository<TestSnapshotAggregateRoot>(Mock.Of<IEventStore>(), snapshotStore, Mock.Of<IMediator>(), new EventSourcingOptions { DefaultSnapshotVersionFrequency = 1 }, Mock.Of<ILogger<EventSourcedRepository<TestSnapshotAggregateRoot>>>());

            var domainEvent = Mock.Of<IDomainEvent>();
            var domainEvents = new List<IDomainEvent> { domainEvent };
            var testAggregate = new TestSnapshotAggregateRoot(Guid.NewGuid(), 1000, 1, 10, domainEvents);

            //Act
            await sut.SaveAsync(testAggregate, CancellationToken.None);

            //Assert
            Mock.Get(snapshotStore)
                .Verify(
                    x => x.StoreSnapshotAsync(It.IsAny<SnapshotEnvelope>(), It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task Should_not_take_snapshot_below_default_frequency()
        {
            //Arrange
            var snapshotStore = Mock.Of<ISnapshotStore>();
            var options = new EventSourcingOptions {DefaultSnapshotVersionFrequency = 2};
            var sut = new EventSourcedRepository<TestSnapshotAggregateRoot>(Mock.Of<IEventStore>(), snapshotStore, Mock.Of<IMediator>(), options, Mock.Of<ILogger<EventSourcedRepository<TestSnapshotAggregateRoot>>>());

            var domainEvent = Mock.Of<IDomainEvent>();
            var domainEvents = new List<IDomainEvent> { domainEvent };
            var testAggregate = new TestSnapshotAggregateRoot(Guid.NewGuid(), 2, 1, null, domainEvents);

            //Act
            await sut.SaveAsync(testAggregate, CancellationToken.None);

            //Assert
            Mock.Get(snapshotStore)
                .Verify(
                    x => x.StoreSnapshotAsync(It.IsAny<SnapshotEnvelope>(), It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Fact]
        public async Task Should_take_snapshot_at_default_frequency()
        {
            //Arrange
            var snapshotStore = Mock.Of<ISnapshotStore>();
            var options = new EventSourcingOptions {DefaultSnapshotVersionFrequency = 2};
            var sut = new EventSourcedRepository<TestSnapshotAggregateRoot>(Mock.Of<IEventStore>(), snapshotStore, Mock.Of<IMediator>(), options, Mock.Of<ILogger<EventSourcedRepository<TestSnapshotAggregateRoot>>>());

            var domainEvent = Mock.Of<IDomainEvent>();
            var domainEvents = new List<IDomainEvent> { domainEvent };
            var testAggregate = new TestSnapshotAggregateRoot(Guid.NewGuid(), 3, 1, null, domainEvents);

            //Act
            await sut.SaveAsync(testAggregate, CancellationToken.None);

            //Assert
            Mock.Get(snapshotStore)
                .Verify(
                    x => x.StoreSnapshotAsync(It.IsAny<SnapshotEnvelope>(), It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task Should_not_take_snapshot_below_custom_frequency()
        {
            //Arrange
            var snapshotStore = Mock.Of<ISnapshotStore>();
            var options = new EventSourcingOptions {DefaultSnapshotVersionFrequency = 10};
            var sut = new EventSourcedRepository<TestSnapshotAggregateRoot>(Mock.Of<IEventStore>(), snapshotStore, Mock.Of<IMediator>(), options, Mock.Of<ILogger<EventSourcedRepository<TestSnapshotAggregateRoot>>>());

            var domainEvent = Mock.Of<IDomainEvent>();
            var domainEvents = new List<IDomainEvent> { domainEvent };
            var testAggregate = new TestSnapshotAggregateRoot(Guid.NewGuid(), 2, 1, 2, domainEvents);

            //Act
            await sut.SaveAsync(testAggregate, CancellationToken.None);

            //Assert
            Mock.Get(snapshotStore)
                .Verify(
                    x => x.StoreSnapshotAsync(It.IsAny<SnapshotEnvelope>(), It.IsAny<CancellationToken>()),
                    Times.Never);
        }

        [Fact]
        public async Task Should_take_snapshot_at_custom_frequency()
        {
            //Arrange
            var snapshotStore = Mock.Of<ISnapshotStore>();
            var options = new EventSourcingOptions {DefaultSnapshotVersionFrequency = 10};
            var sut = new EventSourcedRepository<TestSnapshotAggregateRoot>(Mock.Of<IEventStore>(), snapshotStore, Mock.Of<IMediator>(), options, Mock.Of<ILogger<EventSourcedRepository<TestSnapshotAggregateRoot>>>());

            var domainEvent = Mock.Of<IDomainEvent>();
            var domainEvents = new List<IDomainEvent> { domainEvent };
            var testAggregate = new TestSnapshotAggregateRoot(Guid.NewGuid(), 3, 1, 2, domainEvents);

            //Act
            await sut.SaveAsync(testAggregate, CancellationToken.None);

            //Assert
            Mock.Get(snapshotStore)
                .Verify(
                    x => x.StoreSnapshotAsync(It.IsAny<SnapshotEnvelope>(), It.IsAny<CancellationToken>()),
                    Times.Once);
        }

        [Fact]
        public async Task Should_mark_changes_as_committed_for_aggregate_when_aggregate_is_saved()
        {
            //Arrange
            var eventStoreMock = new Mock<IEventStore>();
            var sut = new EventSourcedRepository<TestEventSourcedAggregateRoot>(eventStoreMock.Object, Mock.Of<ISnapshotStore>(), Mock.Of<IMediator>(), new EventSourcingOptions(), Mock.Of<ILogger<EventSourcedRepository<TestEventSourcedAggregateRoot>>>());
            var domainEvent = Mock.Of<IDomainEvent>();
            var domainEvents = new List<IDomainEvent> { domainEvent };
            var testAggregate = new Mock<TestEventSourcedAggregateRoot>();
            testAggregate.Setup(a => a.GetUncommittedChanges()).Returns(domainEvents);

            //Act
            await sut.SaveAsync(testAggregate.Object, CancellationToken.None);

            //Assert
            testAggregate.Verify(a => a.MarkChangesAsCommitted(), Times.Once);
        }

        [Fact]
        public async Task Should_dispatch_events()
        {
            //Arrange
            var eventStoreMock = new Mock<IEventStore>();
            //var wasCalled = false;
            //var mediatorMock = new Mock<IMediator>();
            //mediatorMock
            //    .Setup(m => m.Publish(It.IsAny<TestDomainEvent>(), It.IsAny<CancellationToken>()))
            //    .Callback(() =>
            //    {
            //        wasCalled = true;
            //    });
            //    .Returns(Task.CompletedTask);

            //.ReturnsAsync(Task.CompletedTask); //<-- return Task to allow await to continue
            //mediatorMock.Setup(x=> x.Publish())
            var mediatorMock = new TestMediator();

            var sut = new EventSourcedRepository<TestEventSourcedAggregateRoot>(eventStoreMock.Object, Mock.Of<ISnapshotStore>(), mediatorMock, new EventSourcingOptions(), Mock.Of<ILogger<EventSourcedRepository<TestEventSourcedAggregateRoot>>>());
            var testAggregate = new Mock<TestEventSourcedAggregateRoot>();
            var domainEvent = new TestDomainEvent();
            var domainEvents = new List<IDomainEvent> { domainEvent };
            testAggregate.Setup(a => a.GetUncommittedChanges()).Returns(domainEvents);
            //Act
            await sut.SaveAsync(testAggregate.Object, CancellationToken.None);

            //Assert
            //mediatorMock.Verify(m => m.Publish(domainEvent, It.IsAny<CancellationToken>()), Times.Once());

            mediatorMock.PublishCallsCount.Should().Be(1);

        }
    }

    public class TestDomainEvent : IDomainEvent, INotification
    {
        public DateTime CreationDate => DateTime.Now;
        
        public Guid EventId => Guid.Empty;

        public int SequenceNumber { get; set; }
    }

    public class TestMediator : IMediator
    {
        public int PublishCallsCount { get; private set; }

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task Send(IRequest request, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = new CancellationToken()) where TNotification : INotification
        {
            this.PublishCallsCount++;
            return Task.CompletedTask;
        }

    }

    public class TestEventStore : IEventStore
    {
        public int AppendEventsToStreamAsyncCallsCount { get; private set; }

        public Task AppendEventsToStreamAsync(string stream, IEnumerable<IEvent> events, int? expectedVersion, CancellationToken cancellationToken = default)
        {
            AppendEventsToStreamAsyncCallsCount++;
            return Task.CompletedTask;
        }

        public Task<List<IEvent>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
