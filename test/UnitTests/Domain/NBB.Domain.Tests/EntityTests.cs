// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace NBB.Domain.Tests
{
    public class EntityTests
    {
        private class TestEntity : Entity<Guid>
        {
            public Guid Id { get; }
            public TestEntity(Guid id)
            {
                Id = id;
            }

            public override Guid GetIdentityValue() => Id;
        }

        [Fact]
        public void Should_be_equal_to_another_entity_with_the_same_identity()
        {
            //Arrange
            var identity = Guid.NewGuid();
            var sut = new TestEntity(identity);
            var other = new TestEntity(identity);

            //Act
            var areEqual = sut.Equals(other)
                && sut == other;

            //Assert
            areEqual.Should().BeTrue();
        }

        [Fact]
        public void Should_support_serialization()
        {
            //Arrange
            var identity = Guid.NewGuid();
            var initialEntity = new TestEntity(identity);

            //Act
            var serializedEntity = JsonConvert.SerializeObject(initialEntity);
            var deserializedEntity = JsonConvert.DeserializeObject<TestEntity>(serializedEntity);

            //Assert
            deserializedEntity.Id.Should().Be(initialEntity.Id);
            deserializedEntity.Should().Be(initialEntity);
        }
    }
}
