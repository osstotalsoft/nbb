using FluentAssertions;
using Xunit;

namespace NBB.Application.DataContracts.Tests
{
    public class CommandTests
    {
        private class TestCommand : Command
        {
            public TestCommand(CommandMetadata metadata)
                : base(metadata)
            {
            }
        }

        [Fact]
        public void Should_create_empty_metadata()
        {
            //Arrange

            //Act
            var command = new TestCommand(null);

            //Assert
            command.Metadata.Should().NotBeNull();
        }

       
    }
}
