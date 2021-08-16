// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using NBB.MultiTenancy.Identification.Identifiers;
using System;
using Xunit;

namespace NBB.MultiTenancy.Identification.Tests.Identifiers
{
    public class IdTenantIdentifierTests
    {
        [Fact]
        public void Should_Return_Given_Guid()
        {
            // Arrange
            var sut = new IdTenantIdentifier();
            var testGuid = Guid.NewGuid();

            // Act
            var tenantId = sut.GetTenantIdAsync(testGuid.ToString()).Result;

            // Assert
            tenantId.Should().Be(testGuid);
        }
    }
}
