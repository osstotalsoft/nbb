// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Moq;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Resolvers;
using NBB.MultiTenancy.Identification.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace NBB.MultiTenancy.Identification.Tests.Services
{
    public class TenantServiceTests
    {
        private readonly Mock<ITenantIdentifier> _identifier;
        private readonly Mock<ITenantTokenResolver> _firstResolver;
        private readonly Mock<ITenantTokenResolver> _secondResolver;
        private readonly Mock<ITenantTokenResolver> _thirdResolver;

        public TenantServiceTests()
        {
            _identifier = new Mock<ITenantIdentifier>();
            _firstResolver = new Mock<ITenantTokenResolver>();
            _secondResolver = new Mock<ITenantTokenResolver>();
            _thirdResolver = new Mock<ITenantTokenResolver>();
        }

        [Fact]
        public void Should_Throw_TenantNotFoundException_If_All_Resolvers_Return_Null()
        {
            // Arrange
            _firstResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            _secondResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            _thirdResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            var identifierPair = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);
            var sut = new DefaultTenantIdentificationService(new List<TenantIdentificationStrategy>() { identifierPair });

            // Act
            Action act = () => Task.WaitAll(sut.GetTenantIdAsync());

            // Assert
            act.Should().Throw<TenantNotFoundException>();
        }

        [Fact]
        public async Task Should_Pass_Token_And_Stop()
        {
            // Arrange
            const string tenantToken = "mock token";
            _firstResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            _secondResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            _thirdResolver.Setup(r => r.GetTenantToken()).Throws<Exception>();
            var firstPair = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _firstResolver.Object }, _identifier.Object);
            var secondPair = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _secondResolver.Object }, _identifier.Object);
            var thirdPair = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _thirdResolver.Object }, _identifier.Object);
            var sut = new DefaultTenantIdentificationService(new List<TenantIdentificationStrategy>() { firstPair, secondPair, thirdPair });

            // Act
            _ = await sut.GetTenantIdAsync();

            // Assert
            _identifier.Verify(i => i.GetTenantIdAsync(It.Is<string>(s => string.Equals(s, tenantToken))), Times.Once());
        }

        [Fact]
        public async Task Try_Method_Should_Return_Null_If_All_Resolvers_Return_Null()
        {
            // Arrange
            _firstResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            _secondResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            _thirdResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            var identifierPair = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);
            var sut = new DefaultTenantIdentificationService(new List<TenantIdentificationStrategy>() { identifierPair });

            // Act
            var result = await sut.TryGetTenantIdAsync();

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task Try_Method_Should_Pass_Token_And_Stop()
        {
            // Arrange
            const string tenantToken = "mock token";
            _firstResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            _secondResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            _thirdResolver.Setup(r => r.GetTenantToken()).Throws<Exception>();
            var firstPair = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _firstResolver.Object }, _identifier.Object);
            var secondPair = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _secondResolver.Object }, _identifier.Object);
            var thirdPair = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _thirdResolver.Object }, _identifier.Object);
            var sut = new DefaultTenantIdentificationService(new List<TenantIdentificationStrategy>() { firstPair, secondPair, thirdPair });

            // Act
            _ = await sut.TryGetTenantIdAsync();

            // Assert
            _identifier.Verify(i => i.GetTenantIdAsync(It.Is<string>(s => string.Equals(s, tenantToken))), Times.Once());
        }
    }
}
