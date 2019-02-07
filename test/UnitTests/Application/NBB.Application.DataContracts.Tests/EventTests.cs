using FluentAssertions;
using Newtonsoft.Json;
using System;
using System.Text.RegularExpressions;
using Xunit;

namespace NBB.Application.DataContracts.Tests
{
    public class EventTests
    {
        private class TestEvent : Event
        {
            public TestEvent(ApplicationMetadata metadata = null)
                : base(Guid.NewGuid(), metadata)
            {
            }
        }

        [Fact]
        public void Should_create_empty_metadata()
        {
            //Arrange
            ApplicationMetadata metadata = null;

            //Act
            var integrationEvent = new TestEvent(metadata);

            //Assert
            integrationEvent.Metadata.Should().NotBeNull();
        }

       
    }
}
