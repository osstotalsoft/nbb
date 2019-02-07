using System;
using System.Text.RegularExpressions;
using FluentAssertions;
using NBB.Application.DataContracts;
using Newtonsoft.Json;
using Xunit;

namespace NBB.Application.DataContracts.Tests
{
    public class QueryTests
    {
        private class TestQuery : Query<string>
        {
            public TestQuery(ApplicationMetadata metadata = null)
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
            var query = new TestQuery(metadata);

            //Assert
            query.Metadata.Should().NotBeNull();
        }

       
    }
}
