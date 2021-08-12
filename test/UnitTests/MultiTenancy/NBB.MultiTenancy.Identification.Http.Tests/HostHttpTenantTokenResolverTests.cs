// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace NBB.MultiTenancy.Identification.Http.Tests
{
    public class HostHttpTenantTokenResolverTests
    {
        private readonly Mock<HttpContext> _mockHttpContext;
        private readonly Mock<HttpRequest> _mockHttpRequest;
        private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;

        public HostHttpTenantTokenResolverTests()
        {
            _mockHttpContext = new Mock<HttpContext>();
            _mockHttpRequest = new Mock<HttpRequest>();
            _mockHttpContext.Setup(c => c.Request).Returns(_mockHttpRequest.Object);

            _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            _mockHttpContextAccessor.Setup(a => a.HttpContext).Returns(_mockHttpContext.Object);
        }

        [Fact]
        public void Should_Not_Retrieve_Context_From_Accessor_At_Constructor()
        {
            // Arrange

            // Act
            var _ = new HostHttpTenantTokenResolver(_mockHttpContextAccessor.Object);

            // Assert
            _mockHttpContextAccessor.Verify(a => a.HttpContext, Times.Never());
        }

        [Fact]
        public void Should_Retrieve_Context_From_Accessor()
        {
            // Arrange
            var sut = new HostHttpTenantTokenResolver(_mockHttpContextAccessor.Object);

            // Act
            var _ = sut.GetTenantToken().Result;

            // Assert
            _mockHttpContextAccessor.Verify(a => a.HttpContext, Times.Once());
        }

        [Fact]
        public void Should_Retrieve_Request_From_Context()
        {
            // Arrange
            var sut = new HostHttpTenantTokenResolver(_mockHttpContextAccessor.Object);

            // Act
            var _ = sut.GetTenantToken().Result;

            // Assert
            _mockHttpContext.Verify(c => c.Request, Times.Once());
        }

        [Fact]
        public void Should_Retrieve_Host_From_Request()
        {
            // Arrange
            var host = new HostString("test.host");
            _mockHttpRequest.Setup(r => r.Host).Returns(host);
            var sut = new HostHttpTenantTokenResolver(_mockHttpContextAccessor.Object);

            // Act
            var _ = sut.GetTenantToken().Result;

            // Assert
            _mockHttpRequest.Verify(i => i.Host, Times.Once());
        }

        [Fact]
        public void Should_Return_Host_Value()
        {
            // Arrange
            var host = new HostString("test.host");
            _mockHttpRequest.Setup(r => r.Host).Returns(host);
            var sut = new HostHttpTenantTokenResolver(_mockHttpContextAccessor.Object);

            // Act
            var result = sut.GetTenantToken().Result;

            // Assert
            result.Should().Be(host.Host);
        }
    }
}
