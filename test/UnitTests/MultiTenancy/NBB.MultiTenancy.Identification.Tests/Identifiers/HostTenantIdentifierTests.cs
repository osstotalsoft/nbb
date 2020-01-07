using FluentAssertions;
using Moq;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NBB.MultiTenancy.Identification.Tests.Identifiers
{
    public class HostTenantIdentifierTests
    {
        private readonly Mock<IHostTenantRepository> _hostTenantRepository;

        public HostTenantIdentifierTests()
        {
            _hostTenantRepository = new Mock<IHostTenantRepository>();
        }

        [Fact]
        public void Should_Return_TenantId()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            _hostTenantRepository.Setup(r => r.GetTenantId(It.IsAny<string>())).Returns(Task.FromResult(tenantId));
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
            var sut = new HostTenantIdentifier(_hostTenantRepository.Object);

            // Act
            var result = sut.GetTenantIdAsync(tenantToken).Result;

            // Assert
            _hostTenantRepository.Verify(r => r.GetTenantId(It.Is<string>(s => string.Equals(s, tenantToken))), Times.Once());
        }
    }
}
