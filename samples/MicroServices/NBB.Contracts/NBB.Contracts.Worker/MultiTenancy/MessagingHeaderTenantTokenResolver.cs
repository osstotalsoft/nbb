using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;
using NBB.Messaging.Abstractions;
using NBB.MultiTenancy.Identification.Resolvers;

namespace NBB.Contracts.Worker.MultiTenancy
{
    public class MessagingHeaderTenantTokenResolver : ITenantTokenResolver
    {
        private const string TenantIdHeader = "TenantId";
        private readonly MessagingContextAccessor _messageContextAccessor;

        public MessagingHeaderTenantTokenResolver(MessagingContextAccessor messageContextAccessor)
        {
            _messageContextAccessor = messageContextAccessor;
        }

        public Task<string> GetTenantToken()
        {
            var headers = _messageContextAccessor.MessagingContext?.ReceivedMessageEnvelope.Headers;
            if (headers == null || !headers.TryGetValue(TenantIdHeader, out var token))
            {
                throw new CannotResolveTokenException();
            }

            return Task.FromResult(headers[TenantIdHeader]);
        }
    }
}
