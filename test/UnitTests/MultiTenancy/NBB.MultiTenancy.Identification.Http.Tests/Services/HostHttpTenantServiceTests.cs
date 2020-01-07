using Microsoft.AspNetCore.Http;
using Moq;
using NBB.MultiTenancy.Identification.Http.Services;
using NBB.MultiTenancy.Identification.Identifiers;
using Xunit;

namespace NBB.MultiTenancy.Identification.Http.Tests.Services
{
    public class HostHttpTenantServiceTests
    {
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<HttpRequest> _mockHttpRequest;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
        private readonly Mock<ITenantIdentifier> _mockIdentifier;

        public HostHttpTenantServiceTests()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);

            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(_mockHttpContext.Object);

            _mockIdentifier = new Mock<ITenantIdentifier>();
        }

        [Fact]
        public void Should_Retrieve_Context_From_Accessor()
        {
            // Arrange

            // Act
            var sut = new HostHttpTenantService(_mockIdentifier.Object, _mockHttpContextAccessor.Object);

            // Assert
            _mockHttpContextAccessor.Verify(a => a.HttpContext, Times.Once());
        }

        [Fact]
        public void Should_Retrieve_Request_From_Context()
        {
            // Arrange
            var sut = new HostHttpTenantService(_mockIdentifier.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = sut.GetTenantIdAsync().Result;

            // Assert
            _mockHttpContext.Verify(c => c.Request, Times.Once());
        }

        [Fact]
        public void Should_Retrieve_Host_From_Request()
        {
            // Arrange
            var host = new HostString("test.host");
            _mockHttpRequest.Setup(r => r.Host).Returns(host);
            var sut = new HostHttpTenantService(_mockIdentifier.Object, _mockHttpContextAccessor.Object);

            // Act
            var result = sut.GetTenantIdAsync().Result;

            // Assert
            _mockHttpRequest.Verify(i => i.Host, Times.Once());
        }
    }
}
