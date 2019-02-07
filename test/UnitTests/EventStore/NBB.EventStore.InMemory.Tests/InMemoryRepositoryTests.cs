using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Abstractions;
using Xunit;

namespace NBB.EventStore.InMemory.Tests
{
    public class InMemoryRepositoryTests
    {
        [Fact]
        public async Task Should_get_appended_events_in_same_order()
        {
            //Arrange
            var sut = new InMemoryRepository();
            var stream = "stream";
            var appendedDescriptors = new List<EventDescriptor>
            {
                new EventDescriptor(Guid.NewGuid(), "type", "data", stream, Guid.NewGuid()),
                new EventDescriptor(Guid.NewGuid(), "type", "data", stream, Guid.NewGuid())
            };


            //Act
            await  sut.AppendEventsToStreamAsync(stream, appendedDescriptors, 0, CancellationToken.None);
            var receivedEvents = await sut.GetEventsFromStreamAsync(stream, null,  CancellationToken.None);

            //Assert
            receivedEvents.Should().NotBeNull();
            receivedEvents.Should().Equal(appendedDescriptors);
        }

        [Fact]
        public async Task Should_not_get_appended_events_with_wrong_stream()
        {
            //Arrange
            var sut = new InMemoryRepository();
            var stream = "stream";
            var otherStream = "stream2";
            var appendedDescriptors = new List<EventDescriptor>
            {
                new EventDescriptor(Guid.NewGuid(), "type", "data", stream, Guid.NewGuid()),
                new EventDescriptor(Guid.NewGuid(), "type", "data", stream, Guid.NewGuid())
            };


            //Act
            await sut.AppendEventsToStreamAsync(stream, appendedDescriptors, 0, CancellationToken.None);
            var receivedEvents = await sut.GetEventsFromStreamAsync(otherStream, null, CancellationToken.None);

            //Assert
            receivedEvents.Should().NotBeNull();
            receivedEvents.Should().BeEmpty();
        }

        [Fact]
        public void Should_throw_concurrency_exception_for_wrong_version()
        {
            //Arrange
            var sut = new InMemoryRepository();
            var stream = "stream";
            var appendedDescriptors = new List<EventDescriptor>
            {
                new EventDescriptor(Guid.NewGuid(), "type", "data", stream, Guid.NewGuid()),
                new EventDescriptor(Guid.NewGuid(), "type", "data", stream, Guid.NewGuid())
            };


            //Act
            Func<Task> action = async() => 
                await sut.AppendEventsToStreamAsync(stream, appendedDescriptors, 999, CancellationToken.None);

            //Assert
            action.Should().Throw<ConcurrencyException>();

        }

        [Fact]
        public async Task Should_throw_unrecoverable_concurrency_exception_for_initial_version()
        {
            //Arrange
            var sut = new InMemoryRepository();
            var stream = "stream";
            var appendedDescriptors = new List<EventDescriptor>
            {
                new EventDescriptor(Guid.NewGuid(), "type", "data", stream, Guid.NewGuid()),
                new EventDescriptor(Guid.NewGuid(), "type", "data", stream, Guid.NewGuid())
            };
            await sut.AppendEventsToStreamAsync(stream, appendedDescriptors, 0, CancellationToken.None);

            //Act
            Func<Task> action2 = async () =>
                await sut.AppendEventsToStreamAsync(stream, appendedDescriptors, 0, CancellationToken.None);

            //Assert
            action2.Should().Throw<ConcurrencyUnrecoverableException>();
        }
    }
}
