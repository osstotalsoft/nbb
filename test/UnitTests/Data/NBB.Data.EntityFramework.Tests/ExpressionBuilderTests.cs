// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Metadata;
using Moq;
using NBB.Data.EntityFramework.Internal;
using Xunit;

namespace NBB.Data.EntityFramework.Tests
{
    public class ExpressionBuilderTests
    {
        public class TestEntity
        {
            public int Id { get; set; }
        }

        [Fact]
        public void Should_build_primary_key_expression_correct()
        {
            //Arrange
            var keyProperty = Mock.Of<IProperty>(property => property.Name == nameof(TestEntity.Id) && property.ClrType == typeof(int));
            var id = 1;
            var testEntity = new TestEntity {Id = id};

            //Act
            var sut = new ExpressionBuilder();
            var lambda = sut.BuildPrimaryKeyExpression<TestEntity>(new List<IProperty> {keyProperty}, new List<object>{id});
            //Expression<Func<TestEntity, bool>> lambda = x => x.Id == id;


            //Assert
            //lambda.ToString().Should().Be("entity => (entity.Id == id)");
            lambda.Compile()(testEntity).Should().BeTrue();

        }
    }
}
