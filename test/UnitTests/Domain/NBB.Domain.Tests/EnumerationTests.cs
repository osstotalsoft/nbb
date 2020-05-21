using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace NBB.Domain.Tests
{
    public class EnumerationTests
    {
        private class TestEnumeration: Enumeration
        {
            public static TestEnumeration Enum1 = new First();
            public static TestEnumeration Enum2 = new Second();
           
            public TestEnumeration(int id, string name)
                : base(id, name)
            {
            }

            private class First : TestEnumeration
            {
                public First() : base(0, "First")
                { }
            }

            private class Second : TestEnumeration
            {
                public Second() : base(1, "Second")
                { }
            }
        }


        [Fact]
        public void Should_be_equal_to_another_instance_with_the_same_id()
        {
            //Arrange
            var sut = new TestEnumeration(3, "aaa");
            var another = new TestEnumeration(3, "bbb");

            //Act
            var areEqual = sut.Equals(another) && sut == another;

            //Assert
            areEqual.Should().BeTrue();
        }


        [Fact]
        public void Should_support_serialization()
        {
            //Arrange
            var sut = new TestEnumeration(3, "sadasd asd asdsd p qwevndofgewrio qwrlhw eqhncqw ehtuwehgfqwlerg");

            //Act
            var serializedEntity = JsonConvert.SerializeObject(sut);
            var deserializedEntity = JsonConvert.DeserializeObject<TestEnumeration>(serializedEntity);

            //Assert
            deserializedEntity.Should().Be(sut);
        }
    }
}
