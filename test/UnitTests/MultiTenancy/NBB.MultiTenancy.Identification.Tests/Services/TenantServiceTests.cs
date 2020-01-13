using FluentAssertions;
using Moq;
using NBB.MultiTenancy.Identification.Identifiers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NBB.MultiTenancy.Identification.Resolvers;
using NBB.MultiTenancy.Identification.Services;
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
        public void Service_Should_Pass_Token_To_Identifier()
        {
            // Arrange
            const string tenantToken = "mock token";
            _firstResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            var identifierPair = new TenantIdentificationPair(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);
            var sut = new TenantService(new List<TenantIdentificationPair>() { identifierPair });

            // Act
            var result = sut.GetTenantIdAsync().Result;

            // Assert
            _identifier.Verify(i => i.GetTenantIdAsync(It.Is<string>(s => string.Equals(s, tenantToken))), Times.Once());
        }

        [Fact]
        public void Service_Should_Pass_LastToken_ToIdentifier()
        {
            // Arrange
            const string tenantToken = "mock token";
            _firstResolver.Setup(r => r.GetTenantToken()).Throws<CannotResolveTokenException>();
            _secondResolver.Setup(r => r.GetTenantToken()).Throws<CannotResolveTokenException>();
            _thirdResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            var identifierPair = new TenantIdentificationPair(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);
            var sut = new TenantService(new List<TenantIdentificationPair>() { identifierPair });

            // Act
            var result = sut.GetTenantIdAsync().Result;

            // Assert
            _identifier.Verify(i => i.GetTenantIdAsync(It.Is<string>(s => string.Equals(s, tenantToken))), Times.Once());
        }

        [Fact]
        public void Service_Should_Return_Identifier_Result()
        {

            // Arrange
            var tenantId = Guid.NewGuid();
            _identifier.Setup(i => i.GetTenantIdAsync(It.IsAny<string>())).Returns(Task.FromResult(tenantId));
            var identifierPair = new TenantIdentificationPair(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);
            var sut = new TenantService(new List<TenantIdentificationPair>() { identifierPair });

            // Act
            var result = sut.GetTenantIdAsync().Result;

            // Assert
            result.Should().Be(tenantId);
        }

        [Fact]
        public void Should_Cache_TenantId()
        {
            // Arrange
            const string tenantToken = "mock token";
            _firstResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            var identifierPair = new TenantIdentificationPair(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);
            var sut = new TenantService(new List<TenantIdentificationPair>() { identifierPair });

            // Act
            var resultFirst = sut.GetTenantIdAsync().Result;
            var resultSecond = sut.GetTenantIdAsync().Result;

            // Assert
            _identifier.Verify(i => i.GetTenantIdAsync(It.Is<string>(s => string.Equals(s, tenantToken))), Times.Once());
            _firstResolver.Verify(r => r.GetTenantToken(), Times.Once());
        }

        [Fact]
        public void Should_Get_First_Cache_Value()
        {
            // Arrange
            const string tenantToken = "mock token";
            _identifier.SetupSequence(i => i.GetTenantIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(Guid.NewGuid()))
                .Returns(Task.FromResult(Guid.NewGuid()));
            _firstResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            var identifierPair = new TenantIdentificationPair(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);
            var sut = new TenantService(new List<TenantIdentificationPair>() { identifierPair });

            // Act
            var resultFirst = sut.GetTenantIdAsync().Result;
            var resultSecond = sut.GetTenantIdAsync().Result;

            // Assert
            resultFirst.Should().Be(resultSecond);
        }

        [Fact]
        public void Should_Throw_TenantNotFoundException_If_All_Resolvers_Fail()
        {
            // Arrange
            _firstResolver.Setup(r => r.GetTenantToken()).Throws<CannotResolveTokenException>();
            _secondResolver.Setup(r => r.GetTenantToken()).Throws<CannotResolveTokenException>();
            _thirdResolver.Setup(r => r.GetTenantToken()).Throws<CannotResolveTokenException>();
            var identifierPair = new TenantIdentificationPair(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);
            var sut = new TenantService(new List<TenantIdentificationPair>() { identifierPair });

            // Act
            Action act = () => Task.WaitAll(sut.GetTenantIdAsync());

            // Assert
            act.Should().Throw<TenantNotFoundException>();
        }

        [Fact]
        public void Should_Throw_Exception_If_Resolver_Fails_Unexpected()
        {
            // Arrange
            _firstResolver.Setup(r => r.GetTenantToken()).Throws<Exception>();
            var identifierPair = new TenantIdentificationPair(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);
            var sut = new TenantService(new List<TenantIdentificationPair>() { identifierPair });

            // Act
            Action act = () => Task.WaitAll(sut.GetTenantIdAsync());

            // Assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Should_Pass_Token_And_Stop()
        {
            // Arrange
            const string tenantToken = "mock token";
            _firstResolver.Setup(r => r.GetTenantToken()).Throws<CannotResolveTokenException>();
            _secondResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            _thirdResolver.Setup(r => r.GetTenantToken()).Throws<CannotResolveTokenException>();
            var firstPair = new TenantIdentificationPair(new List<ITenantTokenResolver>() { _firstResolver.Object }, _identifier.Object);
            var secondPair = new TenantIdentificationPair(new List<ITenantTokenResolver>() { _secondResolver.Object }, _identifier.Object);
            var thirdPair = new TenantIdentificationPair(new List<ITenantTokenResolver>() { _thirdResolver.Object }, _identifier.Object);
            var sut = new TenantService(new List<TenantIdentificationPair>() { firstPair, secondPair, thirdPair });

            // Act
            var result = sut.GetTenantIdAsync().Result;

            // Assert
            _identifier.Verify(i => i.GetTenantIdAsync(It.Is<string>(s => string.Equals(s, tenantToken))), Times.Once());
        }
    }
}
