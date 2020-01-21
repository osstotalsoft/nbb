using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace NBB.MultiTenancy.Identification.Http.Tests
{
    public class HostRefererHttpTokenResolverTests
    {
        private const string HeaderReferer = "Referer";
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<HttpRequest> _mockHttpRequest;
        private readonly Mock<IHeaderDictionary> _mockHeaders;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        public HostRefererHttpTokenResolverTests()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockHeaders = new Mock<IHeaderDictionary>();
            _mockHttpRequest.Setup(r => r.Headers).Returns(_mockHeaders.Object);
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);

            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(_mockHttpContext.Object);
        }

        [Fact]
        public void Should_Retrieve_Context_From_Accessor()
        {
            // Arrange

            // Act
            var _ = new HostRefererHttpTokenResolver(_mockHttpContextAccessor.Object);

            // Assert
            _mockHttpContextAccessor.Verify(a => a.HttpContext, Times.Once());
        }

        [Fact]
        public void Should_Retrieve_Request_From_Context()
        {
            // Arrange
            var sut = new HostRefererHttpTokenResolver(_mockHttpContextAccessor.Object);

            // Act
            var _ = sut.GetTenantToken().Result;

            // Assert
            _mockHttpContext.Verify(c => c.Request, Times.Once());
        }

        [Fact]
        public void Should_Retrieve_Header_From_Request()
        {
            // Arrange
            var sut = new HostRefererHttpTokenResolver(_mockHttpContextAccessor.Object);

            // Act
            var _ = sut.GetTenantToken().Result;

            // Assert
            _mockHttpRequest.Verify(c => c.Headers, Times.Once());
        }

        [Fact]
        public void Should_Try_To_Retrieve_HeaderReferer_From_Headers()
        {
            // Arrange
            var sut = new HostRefererHttpTokenResolver(_mockHttpContextAccessor.Object);

            // Act
            var _ = sut.GetTenantToken().Result;

            // Assert
            _mockHeaders.Verify(h => h.TryGetValue(It.Is<string>(key => string.Equals(key, HeaderReferer)), out It.Ref<StringValues>.IsAny), Times.Once());
        }

        [Fact]
        public void Should_Return_Null_If_Context_Is_Null()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null);
            var sut = new HostRefererHttpTokenResolver(_mockHttpContextAccessor.Object);

            // Act
            var result = sut.GetTenantToken().Result;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Should_Return_Null_If_Request_Is_Null()
        {
            // Arrange
            _mockHttpContext.Setup(a => a.Request).Returns((HttpRequest)null);
            var sut = new HostRefererHttpTokenResolver(_mockHttpContextAccessor.Object);

            // Act
            var result = sut.GetTenantToken().Result;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Should_Return_Null_If_Headers_Are_Null()
        {
            // Arrange
            _mockHttpRequest.Setup(a => a.Headers).Returns((IHeaderDictionary)null);
            var sut = new HostRefererHttpTokenResolver(_mockHttpContextAccessor.Object);

            // Act
            var result = sut.GetTenantToken().Result;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Should_Return_Null_If_HeaderReferer_Does_Not_Exist()
        {
            // Arrange
            _mockHeaders.Setup(h => h.TryGetValue(It.Is<string>(key => string.Equals(key, HeaderReferer)), out It.Ref<StringValues>.IsAny)).Returns(false);
            var sut = new HostRefererHttpTokenResolver(_mockHttpContextAccessor.Object);

            // Act
            var result = sut.GetTenantToken().Result;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Should_Return_Host_From_HeaderReferer()
        {
            // Arrange
            const string host = "test.com";
            var headerReferrer = new StringValues($"HTTP://{host}:81/path/query?test=pass&success=true");
            _mockHeaders.Setup(h => h.TryGetValue(It.Is<string>(key => string.Equals(key, HeaderReferer)), out headerReferrer)).Returns(true);
            var sut = new HostRefererHttpTokenResolver(_mockHttpContextAccessor.Object);

            // Act
            var result = sut.GetTenantToken().Result;

            // Assert
            result.Should().Be(host);
        }
    }
}
