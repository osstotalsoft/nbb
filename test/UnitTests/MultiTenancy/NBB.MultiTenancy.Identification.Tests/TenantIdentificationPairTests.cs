using FluentAssertions;
using Moq;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Resolvers;
using System;
using System.Collections.Generic;
using Xunit;

namespace NBB.MultiTenancy.Identification.Tests
{
    public class TenantIdentificationPairTests
    {
        [Fact]
        public void Should_Throw_ArgumentNullException_If_Resolvers_Are_Null()
        {
            // Arrange
            var identifier = new Mock<ITenantIdentifier>();

            // Act
            Action act = () => new TenantIdentificationPair(null, identifier.Object);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_Throw_ArgumentException_If_Resolvers_Are_Empty()
        {
            // Arrange
            var identifier = new Mock<ITenantIdentifier>();

            // Act
            Action act = () => new TenantIdentificationPair(new List<ITenantTokenResolver>(), identifier.Object);

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Should_Throw_ArgumentNullException_If_Identifier_Is_Null()
        {
            // Arrange
            var tokenResolver = new Mock<ITenantTokenResolver>();

            // Act
            Action act = () => new TenantIdentificationPair(new List<ITenantTokenResolver>() { tokenResolver.Object }, null);

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }
    }
}
