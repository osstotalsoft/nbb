using System;
using FluentAssertions;
using Xunit;

namespace NBB.Domain.Tests
{
    public class DomainEventTests
    
    {
        private class TestDomainEvent : DomainEvent
        {
            public TestDomainEvent(DomainEventMetadata metadata = null)
                : base(Guid.NewGuid(), metadata)
            {
            }
        }

        [Fact]
        public void Should_create_empty_metadata()
        {
            //Arrange
            DomainEventMetadata metadata = null;

            //Act
            var domainEvent = new TestDomainEvent(metadata);

            //Assert
            domainEvent.Metadata.Should().NotBeNull();
        }
    }
}
