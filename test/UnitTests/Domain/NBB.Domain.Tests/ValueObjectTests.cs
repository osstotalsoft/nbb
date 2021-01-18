using System;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace NBB.Domain.Tests
{
    public class ValueObjectTests
    {
        private record TestValueObject(int A, string B, Guid C, decimal D);

        [Fact]
        public void Should_be_equal_to_another_instance_with_the_same_properties()
        {
            //Arrange
            var sut = new TestValueObject(3, "sadasd asd asdsd p qwevndofgewrio qwrlhw eqhncqw ehtuwehgfqwlerg", Guid.NewGuid(), 2324.52m);
            var another = new TestValueObject(3, "sadasd asd asdsd p qwevndofgewrio qwrlhw eqhncqw ehtuwehgfqwlerg", sut.C, 2324.52m);

            //Act
            var areEqual = sut.Equals(another) && sut == another;

            //Assert
            areEqual.Should().BeTrue();
        }


        [Fact]
        public void Should_support_serialization()
        {
            //Arrange
            var sut = new TestValueObject(3, "sadasd asd asdsd p qwevndofgewrio qwrlhw eqhncqw ehtuwehgfqwlerg", Guid.NewGuid(), 2324.52m);

            //Act
            var serializedEntity = JsonConvert.SerializeObject(sut);
            var deserializedEntity = JsonConvert.DeserializeObject<TestValueObject>(serializedEntity);

            //Assert
            deserializedEntity.Should().Be(sut);
        }
    }
}
