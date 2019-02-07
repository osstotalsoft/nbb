using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NBB.Application.DataContracts;
using Newtonsoft.Json;
using Xunit;

namespace NBB.Application.DataContracts.Tests
{
    public class CommandTests
    {
        private class TestCommand : Command
        {
            public TestCommand(ApplicationMetadata metadata = null)
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
            var command = new TestCommand(metadata);

            //Assert
            command.Metadata.Should().NotBeNull();
        }

       
    }
}
