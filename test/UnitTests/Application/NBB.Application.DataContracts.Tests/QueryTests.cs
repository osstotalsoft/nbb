using FluentAssertions;
using Xunit;

namespace NBB.Application.DataContracts.Tests
{
    public class QueryTests
    {
        private class TestQuery : Query<string>
        {
            public TestQuery(QueryMetadata metadata)
                : base(metadata)
            {
            }
        }

        [Fact]
        public void Should_create_empty_metadata()
        {
            //Arrange

            //Act
            var query = new TestQuery(null);

            //Assert
            query.Metadata.Should().NotBeNull();
        }

       
    }
}
