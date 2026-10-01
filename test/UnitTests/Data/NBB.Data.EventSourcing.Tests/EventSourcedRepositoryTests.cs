// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NBB.Core.Abstractions;
using NBB.Data.Abstractions;
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

            public TestEventSourcedAggregateRoot(Guid id, int version, List<object> domainEvents)
            {
                Id = id;
                Version = version;
                _domainEvents = domainEvents;

            }

            public int Version { get; set; }

            private readonly List<object> _domainEvents;

            public Guid Id { get; }

            public int SnapshotVersion { get; protected set; }

            public virtual int? SnapshotVersionFrequency => null;

            public virtual IEnumerable<object> GetUncommittedChanges() => _domainEvents;

            public void LoadFromHistory(IEnumerable<object> history)
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

            public TestSnapshotAggregateRoot(Guid id, int version, int snapshotVersion, int? snapshotVersionFrequency, List<object> domainEvents)
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
            var sut = new EventSourcedRepository<TestEventSourcedAggregateRoot>(eventStoreMock, Mock.Of<ISnapshotStore>(), Mock.Of<IEventPublisher>(), new EventSourcingOptions(), Mock.Of<ILogger<EventSourcedRepository<TestEventSourcedAggregateRoot>>>());
            var domainEvent = Mock.Of<object>();
            var domainEvents = new List<object> { domainEvent };
            var testAggregate = new TestEventSourcedAggregateRoot(Guid.NewGuid(), 5, domainEvents);

            //Act
            await sut.SaveAsync(testAggregate, CancellationToken.None);

            //Assert
            //eventStoreMock.Verify(es => es.AppendEventsToStreamAsync(It.IsAny<string>(), It.Is<IEnumerable<object>>(de=> de.Single() == domainEvent), null, It.IsAny<CancellationToken>()));
            eventStoreMock.AppendEventsToStreamAsyncCallsCount.Should().Be(1);
        }

        [Fact]
        public async Task Should_save_snapshot_in_snapshot_store_when_aggregate_is_saved()
        {
            //Arrange
            var snapshotStore = Mock.Of<ISnapshotStore>();
            var sut = new EventSourcedRepository<TestSnapshotAggregateRoot>(Mock.Of<IEventStore>(), snapshotStore, Mock.Of<IEventPublisher>(), new EventSourcingOptions { DefaultSnapshotVersionFrequency = 1 }, Mock.Of<ILogger<EventSourcedRepository<TestSnapshotAggregateRoot>>>());

            var domainEvent = Mock.Of<object>();
            var domainEvents = new List<object> { domainEvent };
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
            var sut = new EventSourcedRepository<TestSnapshotAggregateRoot>(Mock.Of<IEventStore>(), snapshotStore, Mock.Of<IEventPublisher>(), options, Mock.Of<ILogger<EventSourcedRepository<TestSnapshotAggregateRoot>>>());

            var domainEvent = Mock.Of<object>();
            var domainEvents = new List<object> { domainEvent };
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
            var sut = new EventSourcedRepository<TestSnapshotAggregateRoot>(Mock.Of<IEventStore>(), snapshotStore, Mock.Of<IEventPublisher>(), options, Mock.Of<ILogger<EventSourcedRepository<TestSnapshotAggregateRoot>>>());

            var domainEvent = Mock.Of<object>();
            var domainEvents = new List<object> { domainEvent };
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
            var sut = new EventSourcedRepository<TestSnapshotAggregateRoot>(Mock.Of<IEventStore>(), snapshotStore, Mock.Of<IEventPublisher>(), options, Mock.Of<ILogger<EventSourcedRepository<TestSnapshotAggregateRoot>>>());

            var domainEvent = Mock.Of<object>();
            var domainEvents = new List<object> { domainEvent };
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
            var sut = new EventSourcedRepository<TestSnapshotAggregateRoot>(Mock.Of<IEventStore>(), snapshotStore, Mock.Of<IEventPublisher>(), options, Mock.Of<ILogger<EventSourcedRepository<TestSnapshotAggregateRoot>>>());

            var domainEvent = Mock.Of<object>();
            var domainEvents = new List<object> { domainEvent };
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
            var sut = new EventSourcedRepository<TestEventSourcedAggregateRoot>(eventStoreMock.Object, Mock.Of<ISnapshotStore>(), Mock.Of<IEventPublisher>(), new EventSourcingOptions(), Mock.Of<ILogger<EventSourcedRepository<TestEventSourcedAggregateRoot>>>());
            var domainEvent = Mock.Of<object>();
            var domainEvents = new List<object> { domainEvent };
            var testAggregate = new Mock<TestEventSourcedAggregateRoot>();
            testAggregate.Setup(a => a.GetUncommittedChanges()).Returns(domainEvents);

            //Act
            await sut.SaveAsync(testAggregate.Object, CancellationToken.None);

            //Assert
            testAggregate.Verify(a => a.MarkChangesAsCommitted(), Times.Once);
        }

        [Fact]
        public async Task Should_publish_all_uncommitted_events_once_after_append()
        {
            //Arrange
            var calls = new List<string>();
            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock
                .Setup(x => x.AppendEventsToStreamAsync(It.IsAny<string>(), It.IsAny<IEnumerable<object>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .Callback(() => calls.Add("append"))
                .Returns(Task.CompletedTask);
            var eventPublisherMock = new Mock<IEventPublisher>();
            IEnumerable<object> publishedEvents = null;
            eventPublisherMock
                .Setup(x => x.PublishAsync(It.IsAny<IEnumerable<object>>(), It.IsAny<CancellationToken>()))
                .Callback((IEnumerable<object> events, CancellationToken _) => { calls.Add("publish"); publishedEvents = events; })
                .Returns(Task.CompletedTask);

            var sut = new EventSourcedRepository<TestEventSourcedAggregateRoot>(eventStoreMock.Object, Mock.Of<ISnapshotStore>(), eventPublisherMock.Object, new EventSourcingOptions(), Mock.Of<ILogger<EventSourcedRepository<TestEventSourcedAggregateRoot>>>());
            var testAggregate = new Mock<TestEventSourcedAggregateRoot>();
            var domainEvent = new TestDomainEvent();
            var otherEvent = new object();
            var domainEvents = new List<object> { domainEvent, otherEvent };
            testAggregate.Setup(a => a.GetUncommittedChanges()).Returns(domainEvents);

            //Act
            await sut.SaveAsync(testAggregate.Object, CancellationToken.None);

            //Assert
            eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<IEnumerable<object>>(), It.IsAny<CancellationToken>()), Times.Once);
            publishedEvents.Should().BeEquivalentTo(domainEvents, o => o.WithStrictOrdering());
            calls.Should().Equal("append", "publish");
        }

        [Fact]
        public async Task Should_not_publish_events_when_append_fails()
        {
            //Arrange
            var eventStoreMock = new Mock<IEventStore>();
            eventStoreMock
                .Setup(x => x.AppendEventsToStreamAsync(It.IsAny<string>(), It.IsAny<IEnumerable<object>>(), It.IsAny<int?>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new ConcurrencyException("concurrency"));
            var eventPublisherMock = new Mock<IEventPublisher>();

            var sut = new EventSourcedRepository<TestEventSourcedAggregateRoot>(eventStoreMock.Object, Mock.Of<ISnapshotStore>(), eventPublisherMock.Object, new EventSourcingOptions(), Mock.Of<ILogger<EventSourcedRepository<TestEventSourcedAggregateRoot>>>());
            var testAggregate = new Mock<TestEventSourcedAggregateRoot>();
            testAggregate.Setup(a => a.GetUncommittedChanges()).Returns(new List<object> { new TestDomainEvent() });

            //Act
            var act = () => sut.SaveAsync(testAggregate.Object, CancellationToken.None);

            //Assert
            await act.Should().ThrowAsync<ConcurrencyException>();
            eventPublisherMock.Verify(x => x.PublishAsync(It.IsAny<IEnumerable<object>>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public void Should_fail_to_resolve_repository_when_no_event_publisher_is_registered()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(Mock.Of<IEventStore>());
            services.AddSingleton(Mock.Of<ISnapshotStore>());
            services.AddEventSourcingDataAccess();
            services.AddEventSourcedRepository<TestEventSourcedAggregateRoot>();
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            //Act
            var act = () => scope.ServiceProvider.GetRequiredService<IEventSourcedRepository<TestEventSourcedAggregateRoot>>();

            //Assert
            act.Should().Throw<InvalidOperationException>().WithMessage("*IEventPublisher*");
        }

        [Fact]
        public void Should_resolve_repository_with_a_custom_event_publisher()
        {
            //Arrange
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(Mock.Of<IEventStore>());
            services.AddSingleton(Mock.Of<ISnapshotStore>());
            services.AddEventSourcingDataAccess();
            services.AddEventSourcedRepository<TestEventSourcedAggregateRoot>();
            services.AddSingleton(Mock.Of<IEventPublisher>());
            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();

            //Act
            var repository = scope.ServiceProvider.GetRequiredService<IEventSourcedRepository<TestEventSourcedAggregateRoot>>();

            //Assert
            repository.Should().NotBeNull();
        }
    }

    public class TestDomainEvent
    {
        public DateTime CreationDate => DateTime.Now;
        
        public Guid EventId => Guid.Empty;

        public int SequenceNumber { get; set; }
    }

    public class TestEventStore : IEventStore
    {
        public int AppendEventsToStreamAsyncCallsCount { get; private set; }

        public Task AppendEventsToStreamAsync(string stream, IEnumerable<object> events, int? expectedVersion, CancellationToken cancellationToken = default)
        {
            AppendEventsToStreamAsyncCallsCount++;
            return Task.CompletedTask;
        }

        public Task<List<object>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task DeleteStreamAsync(string stream, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
