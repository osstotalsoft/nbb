using NBB.Messaging.Abstractions;
using NBB.MultiTenancy.Identification.Identifiers;
using NBB.MultiTenancy.Identification.Services;
using System.Threading.Tasks;

namespace NBB.MultiTenancy.Identification.Message.Services
{
    public class IdHeaderMsgTenantService : AbstractTenantService
    {
        private readonly MessagingContext _messageContext;

        public IdHeaderMsgTenantService(ITenantIdentifier identifier, MessagingContextAccessor messageContextAccessor) : base(identifier)
        {
            _messageContext = messageContextAccessor.MessagingContext;
        }

        protected override Task<string> GetTenantToken()
        {
            var tenantId = _messageContext.ReceivedMessageEnvelope.Headers["tenantId"];
            return Task.FromResult(tenantId);
        }
    }
}
