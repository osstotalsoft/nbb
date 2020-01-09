using NBB.Messaging.Abstractions;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Messaging
{
    public class TenantIdHeaderMessagingTokenResolver : ITenantTokenResolver
    {
        private readonly MessagingContext _messageContext;

        public TenantIdHeaderMessagingTokenResolver(MessagingContextAccessor messageContextAccessor)
        {
            _messageContext = messageContextAccessor.MessagingContext;
        }

        public Task<string> GetTenantToken()
        {
            var tenantId = _messageContext.ReceivedMessageEnvelope.Headers["tenantId"];
            return Task.FromResult(tenantId);
        }
    }
}
