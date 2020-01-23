using NBB.Messaging.Abstractions;
using System.Threading.Tasks;
using NBB.MultiTenancy.Identification.Resolvers;

namespace NBB.MultiTenancy.Identification.Messaging
{
    public class TenantIdHeaderMessagingTokenResolver : ITenantTokenResolver
    {
        private readonly string _headerKey;
        private readonly MessagingContextAccessor _messageContextAccessor;

        public TenantIdHeaderMessagingTokenResolver(MessagingContextAccessor messageContextAccessor, string headerKey)
        {
            _headerKey = headerKey;
            _messageContextAccessor = messageContextAccessor;
        }

        public Task<string> GetTenantToken()
        {
            var headers = _messageContextAccessor?.MessagingContext?.ReceivedMessageEnvelope?.Headers;
            if (headers == null || !headers.TryGetValue(_headerKey, out var token))
            {
                return Task.FromResult<string>(null);
            }

            return Task.FromResult(token);
        }
    }
}
