// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using NBB.Messaging.Abstractions;
using Proto.V1;

namespace NBB.Messaging.Rusi
{
    public class RusiMessageBusPublisher : IMessageBusPublisher
    {
        private readonly Proto.V1.Rusi.RusiClient _client;
        private readonly IOptions<RusiOptions> _options;

        public RusiMessageBusPublisher(Proto.V1.Rusi.RusiClient client, IOptions<RusiOptions> options)
        {
            _client = client;
            _options = options;
        }

        public async Task PublishAsync<T>(T message, MessagingPublisherOptions options = null,
            CancellationToken cancellationToken = default)
        {

            await _client.PublishAsync(new PublishRequest()
            {
                PubsubName = _options.Value.PubsubName,
               // Topic = ...
            });

        }
    }
}
