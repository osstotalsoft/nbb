// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host
{
    public interface IMessagingHost
    {
        public Task StartAsync(CancellationToken cancellationToken = default);
        public Task StopAsync(CancellationToken cancellationToken = default);
        public void ScheduleRestart(TimeSpan delay = default);
        public bool IsRunning();
    }


    public static class MessagingHostExtensions
    {
        /// <summary>
        /// Attempts to gracefully stop the host with the given timeout.
        /// </summary>
        /// <param name="host">The <see cref="IMessagingHost"/> to stop.</param>
        /// <param name="timeout">The timeout for stopping gracefully. Once expired the
        /// server may terminate any remaining active connections.</param>
        /// <returns>The <see cref="Task"/> that represents the asynchronous operation.</returns>
        public static async Task StopAsync(this IMessagingHost host, TimeSpan timeout)
        {
            using var cts = new CancellationTokenSource(timeout);
            await host.StopAsync(cts.Token).ConfigureAwait(false);
        }
    }
}
