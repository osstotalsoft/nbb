using FluentAssertions;
using Moq;
using NBB.MultiTenancy.Identification.Identifiers;
using System;
using System.Threading.Tasks;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Repositories;
using Xunit;

namespace NBB.MultiTenancy.Identification.Tests.Identifiers
{
    public class HostTenantIdentifierTests
    {
        private readonly Mock<ITenantRepository> _hostTenantRepository;

        public HostTenantIdentifierTests()
        {
            _hostTenantRepository = new Mock<ITenantRepository>();
        }

        [Fact]
        public void Should_Return_TenantId()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = new Tenant(tenantId);
            _hostTenantRepository.Setup(r => r.GetByHost(It.IsAny<string>())).Returns(Task.FromResult(tenant));
            var sut = new HostTenantIdentifier(_hostTenantRepository.Object);

            // Act
            var result = sut.GetTenantIdAsync(string.Empty).Result;

            // Assert
            result.Should().Be(tenantId);
        }

        [Fact]
        public void Should_Pass_Token_To_Repository()
        {
            // Arrange
            const string tenantToken = "tenant token";
            _hostTenantRepository.Setup(r => r.GetByHost(It.IsAny<string>())).Returns(Task.FromResult(new Tenant(Guid.Empty)));
            var sut = new HostTenantIdentifier(_hostTenantRepository.Object);

            // Act
            var _ = sut.GetTenantIdAsync(tenantToken).Result;

            // Assert
            _hostTenantRepository.Verify(r => r.GetByHost(It.Is<string>(s => string.Equals(s, tenantToken))), Times.Once());
        }
    }
}
