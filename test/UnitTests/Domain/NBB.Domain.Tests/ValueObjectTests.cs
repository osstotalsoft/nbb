using System;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace NBB.Domain.Tests
{
    public class ValueObjectTests
    {
        private class TestValueObject : ValueObject
        {
            public int A { get; }
            public string B { get; }
            public Guid C { get; }
            public decimal D { get; }

            protected override IEnumerable<object> GetAtomicValues()
            {
                yield return A;
                yield return B;
                yield return C;
                yield return D;
            }

            public TestValueObject(int a, string b, Guid c, decimal d)
            {
                A = a;
                B = b;
                C = c;
                D = d;
            }
        }


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

        [Fact]
        public void Should_be_different()
        {
            //Arrange
            var sut = new TestValueObject(3, "different", Guid.NewGuid(), 2324.52m);
            var another = new TestValueObject(3, "same", sut.C, 2324.52m);

            //Act
            var areEqual = !sut.Equals(another) && sut != another;

            //Assert
            areEqual.Should().BeTrue();
        }

        [Fact]
        public void Should_be_equal_if_cloned()
        {
            //Arrange
            var sut = new TestValueObject(3, "different", Guid.NewGuid(), 2324.52m);
            var another = sut.GetCopy();

            //Act
            var areEqual = sut.Equals(another) && sut == another && sut.GetHashCode() == another.GetHashCode();

            //Assert
            areEqual.Should().BeTrue();
        }
    }
}
