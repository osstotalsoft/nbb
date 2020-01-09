using NBB.Messaging.Abstractions;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Messaging
{
    public class TenantIdHeaderMessagingTokenResolver : ITenantTokenResolver
    {
        private readonly string _headerKey;
        private readonly MessagingContext _messageContext;

        public TenantIdHeaderMessagingTokenResolver(MessagingContextAccessor messageContextAccessor, string headerKey)
        {
            _headerKey = headerKey;
            _messageContext = messageContextAccessor.MessagingContext;
        }

        public Task<string> GetTenantToken()
        {
            var headers = _messageContext.ReceivedMessageEnvelope.Headers;
            if (!headers.TryGetValue(_headerKey, out var token))
            {
                throw new CannotResolveTokenException();
            }

            return Task.FromResult(headers[_headerKey]);
        }
    }
}
