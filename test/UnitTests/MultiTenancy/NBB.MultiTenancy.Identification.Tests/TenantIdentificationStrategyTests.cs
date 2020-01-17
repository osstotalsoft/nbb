using FluentAssertions;
using Moq;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Resolvers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace NBB.MultiTenancy.Identification.Tests
{
    public class TenantIdentificationStrategyTests
    {
        private readonly Mock<ITenantIdentifier> _identifier;
        private readonly Mock<ITenantTokenResolver> _firstResolver;
        private readonly Mock<ITenantTokenResolver> _secondResolver;
        private readonly Mock<ITenantTokenResolver> _thirdResolver;

        public TenantIdentificationStrategyTests()
        {
            _identifier = new Mock<ITenantIdentifier>();
            _firstResolver = new Mock<ITenantTokenResolver>();
            _secondResolver = new Mock<ITenantTokenResolver>();
            _thirdResolver = new Mock<ITenantTokenResolver>();
        }

        [Fact]
        public void Should_Throw_ArgumentNullException_If_Resolvers_Are_Null()
        {
            // Arrange
            var identifier = new Mock<ITenantIdentifier>();

            // Act
            Action act = () =>
            {
                var _ = new TenantIdentificationStrategy(null, identifier.Object);
            };

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Should_Throw_ArgumentException_If_Resolvers_Are_Empty()
        {
            // Arrange
            var identifier = new Mock<ITenantIdentifier>();

            // Act
            Action act = () =>
            {
                var _ = new TenantIdentificationStrategy(new List<ITenantTokenResolver>(), identifier.Object);
            };

            // Assert
            act.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void Should_Throw_ArgumentNullException_If_Identifier_Is_Null()
        {
            // Arrange
            var tokenResolver = new Mock<ITenantTokenResolver>();

            // Act
            Action act = () =>
            {
                var _ = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { tokenResolver.Object }, null);
            };

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Strategy_Should_Pass_Token_To_Identifier()
        {
            // Arrange
            const string tenantToken = "mock token";
            _firstResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            var sut = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);

            // Act
            var _ = sut.TryGetTenantIdAsync().Result;

            // Assert
            _identifier.Verify(i => i.GetTenantIdAsync(It.Is<string>(s => string.Equals(s, tenantToken))), Times.Once());
        }

        [Fact]
        public void Strategy_Should_Pass_LastToken_ToIdentifier()
        {
            // Arrange
            const string tenantToken = "mock token";
            _firstResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            _secondResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            _thirdResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            var sut = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);

            // Act
            var _ = sut.TryGetTenantIdAsync().Result;

            // Assert
            _identifier.Verify(i => i.GetTenantIdAsync(It.Is<string>(s => string.Equals(s, tenantToken))), Times.Once());
        }

        [Fact]
        public void Service_Should_Return_Identifier_Result()
        {
            // Arrange
            const string tenantToken = "mock token";
            _firstResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            var tenantId = Guid.NewGuid();
            _identifier.Setup(i => i.GetTenantIdAsync(It.IsAny<string>())).Returns(Task.FromResult(tenantId));
            var sut = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);

            // Act
            var result = sut.TryGetTenantIdAsync().Result;

            // Assert
            result.Should().Be(tenantId);
        }

        [Fact]
        public void Should_Throw_Exception_If_Resolver_Fails_Unexpected()
        {
            // Arrange
            _firstResolver.Setup(r => r.GetTenantToken()).Throws<Exception>();
            var sut = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);

            // Act
            Action act = () => Task.WaitAll(sut.TryGetTenantIdAsync());

            // Assert
            act.Should().Throw<Exception>();
        }

        [Fact]
        public void Should_Return_Null_If_All_Resolvers_Fail()
        {
            // Arrange
            _firstResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            _secondResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            _thirdResolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult<string>(null));
            var sut = new TenantIdentificationStrategy(new List<ITenantTokenResolver>() { _firstResolver.Object, _secondResolver.Object, _thirdResolver.Object }, _identifier.Object);

            // Act
            var result = sut.TryGetTenantIdAsync().Result;

            // Assert
            result.Should().BeNull();
        }
    }
}
