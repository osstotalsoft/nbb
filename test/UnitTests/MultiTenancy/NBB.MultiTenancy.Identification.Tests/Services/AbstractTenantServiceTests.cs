using System;
using Moq;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Services;
using System.Threading.Tasks;
using FluentAssertions;
using Xunit;

namespace NBB.MultiTenancy.Identification.Tests.Services
{
    public class AbstractTenantServiceTests
    {
        public class SutTenantService : AbstractTenantService
        {
            private string _tenantToken;

            public SutTenantService(ITenantIdentifier identifier) : base(identifier)
            {
            }

            protected override Task<string> GetTenantToken()
            {
                return Task.FromResult(_tenantToken);
            }

            public void SetTenantToken(string tenantToken)
            {
                _tenantToken = tenantToken;
            }
        }

        private readonly Mock<ITenantIdentifier> _identifier;

        public AbstractTenantServiceTests()
        {
            _identifier = new Mock<ITenantIdentifier>();
        }

        [Fact]
        public void Service_Should_Pass_Token_To_Identifier()
        {
            // Arrange
            const string tenantToken = "mock token";
            _identifier.Setup(i => i.GetTenantIdAsync(It.IsAny<string>())).Returns(Task.FromResult(Guid.Empty));
            var sut = new SutTenantService(_identifier.Object);
            sut.SetTenantToken(tenantToken);

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
            var sut = new SutTenantService(_identifier.Object);

            // Act
            var result = sut.GetTenantIdAsync().Result;

            // Assert
            result.Should().Be(tenantId);
        }
    }
}
