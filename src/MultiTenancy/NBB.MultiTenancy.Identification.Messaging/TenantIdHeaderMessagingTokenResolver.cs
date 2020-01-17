using NBB.Messaging.Abstractions;
using System.Threading.Tasks;
using NBB.MultiTenancy.Identification.Resolvers;

namespace NBB.MultiTenancy.Identification.Messaging
{
    public class TenantIdHeaderMessagingTokenResolver : ITenantTokenResolver
    {
        private readonly string _headerKey;
        private readonly MessagingContext _messageContext;

        public TenantIdHeaderMessagingTokenResolver(MessagingContextAccessor messageContextAccessor, string headerKey)
        {
            _headerKey = headerKey;
            _messageContext = messageContextAccessor?.MessagingContext;
        }

        public Task<string> GetTenantToken()
        {
            var headers = _messageContext?.ReceivedMessageEnvelope?.Headers;
            if (headers == null || !headers.TryGetValue(_headerKey, out var token))
            {
                return Task.FromResult<string>(null);
            }

            return Task.FromResult(token);
        }
    }
}
