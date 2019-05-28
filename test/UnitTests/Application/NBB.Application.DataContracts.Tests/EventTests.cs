using FluentAssertions;
using Xunit;

namespace NBB.Application.DataContracts.Tests
{
    public class EventTests
    {
        private class TestEvent : Event
        {
            public TestEvent(EventMetadata metadata)
                : base(metadata)
            {
            }
        }

        [Fact]
        public void Should_create_empty_metadata()
        {
            //Arrange

            //Act
            var integrationEvent = new TestEvent(null);

            //Assert
            integrationEvent.Metadata.Should().NotBeNull();
        }

       
    }
}
