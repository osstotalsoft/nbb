using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using Xunit;

namespace NBB.MultiTenancy.Identification.Http.Tests
{
    public class TenantIdHeaderHttpTokenResolverTests
    {
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<HttpRequest> _mockHttpRequest;
        private readonly Mock<IHeaderDictionary> _mockHeaders;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        public TenantIdHeaderHttpTokenResolverTests()
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
            var _ = new TenantIdHeaderHttpTokenResolver(_mockHttpContextAccessor.Object, string.Empty);

            // Assert
            _mockHttpContextAccessor.Verify(a => a.HttpContext, Times.Once());
        }

        [Fact]
        public void Should_Retrieve_Headers_From_Request_From_Context()
        {
            // Arrange
            var sut = new TenantIdHeaderHttpTokenResolver(_mockHttpContextAccessor.Object, string.Empty);

            // Act
            var _ = sut.GetTenantToken().Result;

            // Assert
            _mockHttpContext.Verify(c => c.Request, Times.Once());
            _mockHttpRequest.Verify(r => r.Headers, Times.Once());
        }

        [Fact]
        public void Should_Return_Value_From_Header()
        {
            // Arrange
            var hKey = "key";
            var hValue = "value";
            _mockHeaders.Setup(h => h[hKey]).Returns(new StringValues(hValue));
            var sut = new TenantIdHeaderHttpTokenResolver(_mockHttpContextAccessor.Object, hKey);

            // Act
            var result = sut.GetTenantToken().Result;

            // Assert
            result.Should().Be(hValue);
        }
    }
}
