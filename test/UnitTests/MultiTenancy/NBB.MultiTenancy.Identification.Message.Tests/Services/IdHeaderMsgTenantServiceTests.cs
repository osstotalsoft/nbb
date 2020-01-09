using Moq;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using NBB.MultiTenancy.Identification.Identifiers;
using System.Collections.Generic;
using NBB.MultiTenancy.Identification.Message.Services;
using Xunit;

namespace NBB.MultiTenancy.Identification.Message.Tests.Services
{
    public class IdHeaderMsgTenantServiceTests
    {
        private readonly Mock<ITenantIdentifier> _mockTenantIdentifier;
        private readonly MessagingContextAccessor _mockMessagingContextAccessor;
        private readonly MessagingContext _mockMessagingContext;
        private readonly MessagingEnvelope _mockMessagingEnvelope;
        private readonly Dictionary<string, string> _headers;

        public IdHeaderMsgTenantServiceTests()
        {
            _mockTenantIdentifier = new Mock<ITenantIdentifier>();
            _mockMessagingContextAccessor = new MessagingContextAccessor();
            _headers = new Dictionary<string, string>();
            _mockMessagingEnvelope = new MessagingEnvelope(_headers, new object());
            _mockMessagingContext = new MessagingContext(_mockMessagingEnvelope);

            _mockMessagingContextAccessor.MessagingContext = _mockMessagingContext;
        }

        [Fact]
        public void Should_Pass_Token_To_Identifier()
        {
            // Arrange
            const string tenantToken = "tenantToken";
            _headers.Add("tenantId", tenantToken);
            var sut = new IdHeaderMsgTenantService(_mockTenantIdentifier.Object, _mockMessagingContextAccessor);

            // Act
            var result = sut.GetTenantIdAsync().Result;

            // Assert
            _mockTenantIdentifier.Verify(i => i.GetTenantIdAsync(It.Is<string>(s => string.Equals(s, tenantToken))), Times.Once());
        }
    }
}
