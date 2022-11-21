// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Moq;
using NBB.MultiTenancy.Identification.Identifiers;
using System;
using System.Threading.Tasks;
using NBB.MultiTenancy.Abstractions;
using NBB.MultiTenancy.Abstractions.Repositories;
using Xunit;
using System.Threading;

namespace NBB.MultiTenancy.Identification.Tests.Identifiers
{
    public class HostTenantIdentifierTests
    {
        private readonly Mock<ITenantRepository> _tenantRepository;

        public HostTenantIdentifierTests()
        {
            _tenantRepository = new Mock<ITenantRepository>();
        }

        [Fact]
        public void Should_Return_TenantId()
        {
            // Arrange
            var tenantId = Guid.NewGuid();
            var tenant = new Tenant(tenantId, string.Empty);
            _tenantRepository.Setup(r => r.GetByHost(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(tenant));
            var sut = new HostTenantIdentifier(_tenantRepository.Object);

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
            _tenantRepository.Setup(r => r.GetByHost(It.IsAny<string>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(new Tenant(Guid.Empty, string.Empty)));
            var sut = new HostTenantIdentifier(_tenantRepository.Object);

            // Act
            var _ = sut.GetTenantIdAsync(tenantToken).Result;

            // Assert
            _tenantRepository.Verify(r => r.GetByHost(It.Is<string>(s => string.Equals(s, tenantToken)), It.IsAny<CancellationToken>()), Times.Once());
        }
    }
}
