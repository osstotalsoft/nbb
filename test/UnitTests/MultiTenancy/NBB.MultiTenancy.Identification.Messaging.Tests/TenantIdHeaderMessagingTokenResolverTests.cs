using System.Collections.Generic;
using NBB.Messaging.Abstractions;
using NBB.Messaging.DataContracts;
using Xunit;

namespace NBB.MultiTenancy.Identification.Messaging.Tests
{
    public class TenantIdHeaderMessagingTokenResolverTests
    {
        private readonly MessagingContextAccessor _mockMessagingContextAccessor;
        private readonly MessagingContext _mockMessagingContext;
        private readonly MessagingEnvelope _mockMessagingEnvelope;
        private readonly Dictionary<string, string> _headers;

        public TenantIdHeaderMessagingTokenResolverTests()
        {
            _mockMessagingContextAccessor = new MessagingContextAccessor();
            _headers = new Dictionary<string, string>();
            _mockMessagingEnvelope = new MessagingEnvelope(_headers, new object());
            _mockMessagingContext = new MessagingContext(_mockMessagingEnvelope);

            _mockMessagingContextAccessor.MessagingContext = _mockMessagingContext;
        }

        [Fact]
        public void Should_Pass_Token_To_Identifier()
        {
        }
    }
}
