using FluentAssertions;
using Moq;
using NBB.MultiTenancy.Identification.Identifiers;
using System;
using System.Threading.Tasks;
using NBB.MultiTenancy.Identification.Services;
using Xunit;

namespace NBB.MultiTenancy.Identification.Tests.Services
{
    public class TenantServiceTests
    {
        private readonly Mock<ITenantIdentifier> _identifier;
        private readonly Mock<ITenantTokenResolver> _resolver;

        public TenantServiceTests()
        {
            _identifier = new Mock<ITenantIdentifier>();
            _resolver = new Mock<ITenantTokenResolver>();
        }

        [Fact]
        public void Service_Should_Pass_Token_To_Identifier()
        {
            // Arrange
            const string tenantToken = "mock token";
            _resolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            var sut = new TenantService(_identifier.Object, _resolver.Object);

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
            var sut = new TenantService(_identifier.Object, _resolver.Object);

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
            _resolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            var sut = new TenantService(_identifier.Object, _resolver.Object);

            // Act
            var resultFirst = sut.GetTenantIdAsync().Result;
            var resultSecond = sut.GetTenantIdAsync().Result;

            // Assert
            _identifier.Verify(i => i.GetTenantIdAsync(It.Is<string>(s => string.Equals(s, tenantToken))), Times.Once());
        }

        [Fact]
        public void Should_Get_First_Cache_Value()
        {
            // Arrange
            const string tenantToken = "mock token";
            _identifier.SetupSequence(i => i.GetTenantIdAsync(It.IsAny<string>()))
                .Returns(Task.FromResult(Guid.NewGuid()))
                .Returns(Task.FromResult(Guid.NewGuid()));
            _resolver.Setup(r => r.GetTenantToken()).Returns(Task.FromResult(tenantToken));
            var sut = new TenantService(_identifier.Object, _resolver.Object);

            // Act
            var resultFirst = sut.GetTenantIdAsync().Result;
            var resultSecond = sut.GetTenantIdAsync().Result;

            // Assert
            resultFirst.Should().Be(resultSecond);
        }
    }
}
