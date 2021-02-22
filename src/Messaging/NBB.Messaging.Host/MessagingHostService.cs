using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NBB.Core.Abstractions;

namespace NBB.Messaging.Host
{
    public class MessagingHostService : BackgroundService
    {
        private readonly IMessagingHost _messagingHost;

        public MessagingHostService(IMessagingHost messagingHost)
        {
            _messagingHost = messagingHost;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await _messagingHost.StartAsync(stoppingToken);

            await stoppingToken.WhenCanceled();

            await _messagingHost.StopAsync();
        }
    }
}
