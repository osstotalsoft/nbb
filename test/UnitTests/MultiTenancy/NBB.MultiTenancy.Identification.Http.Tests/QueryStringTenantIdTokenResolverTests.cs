using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace NBB.MultiTenancy.Identification.Http.Tests
{
    public class QueryStringTenantIdTokenResolverTests
    {
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<HttpRequest> _mockHttpRequest;
        private readonly Mock<IQueryCollection> _mockQuery;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        public QueryStringTenantIdTokenResolverTests()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockQuery = new Mock<IQueryCollection>();

            
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);
            _mockHttpRequest.Setup(c => c.Query).Returns(_mockQuery.Object);
            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(_mockHttpContext.Object);
        }

        [Fact]
        public void Should_Not_Retrieve_Context_From_Accessor_At_Constructor()
        {
            // Arrange

            // Act
            var _ = new QueryStringTenantIdTokenResolver(_mockHttpContextAccessor.Object, string.Empty);

            // Assert
            _mockHttpContextAccessor.Verify(a => a.HttpContext, Times.Never());
        }

        [Fact]
        public void Should_Retrieve_QueryString_From_Request_From_Context_From_Accessor()
        {
            // Arrange
            var paramName = "name";
            var paramValue = "value";
            _mockQuery.Setup(q => q[paramName]).Returns(paramValue);
            var sut = new QueryStringTenantIdTokenResolver(_mockHttpContextAccessor.Object, paramName);

            // Act
            var _ = sut.GetTenantToken().Result;

            // Assert
            _mockHttpContextAccessor.Verify(a => a.HttpContext, Times.Once());
            _mockHttpContext.Verify(c => c.Request, Times.Once());
            _mockHttpRequest.Verify(r => r.Query, Times.Once());
        }

        [Fact]
        public void Should_Return_Value_From_QueryString()
        {
            // Arrange
            var paramName = "name";
            var paramValue = "value";
            _mockQuery.Setup(q => q[paramName]).Returns(paramValue);
            var sut = new QueryStringTenantIdTokenResolver(_mockHttpContextAccessor.Object, paramName);

            // Act
            var result = sut.GetTenantToken().Result;

            // Assert
            result.Should().Be(paramValue);
        }

        [Fact]
        public void Should_Return_Null_When_The_ParamName_Is_Incorrect()
        {
            // Arrange
            var paramName = "badName";
            var paramValue = "value";
            _mockQuery.Setup(q => q[paramName]).Returns(paramValue);
            var sut = new QueryStringTenantIdTokenResolver(_mockHttpContextAccessor.Object, "name");

            // Act
            var result = sut.GetTenantToken().Result;

            // Assert
            result.Should().BeNull();
        }

        [Fact]
        public void Should_Return_Null_If_Context_Is_Null()
        {
            // Arrange
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns((HttpContext)null);
            var sut = new QueryStringTenantIdTokenResolver(_mockHttpContextAccessor.Object, string.Empty);

            // Act
            var result = sut.GetTenantToken().Result;

            // Assert
            result.Should().BeNull();
        }
    }
}
