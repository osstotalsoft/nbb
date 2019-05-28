using FluentAssertions;
using Xunit;

namespace NBB.Domain.Tests
{
    public class DomainEventTests
    
    {
        private class TestDomainEvent : DomainEvent
        {
            public TestDomainEvent(DomainEventMetadata metadata)
                : base(metadata)
            {
            }
        }

        [Fact]
        public void Should_create_empty_metadata()
        {
            //Arrange

            //Act
            var domainEvent = new TestDomainEvent(null);

            //Assert
            domainEvent.Metadata.Should().NotBeNull();
        }
    }
}
