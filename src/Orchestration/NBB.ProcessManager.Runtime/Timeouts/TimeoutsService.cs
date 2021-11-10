// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Runtime.Timeouts
{
    public class TimeoutsService : BackgroundService
    {
        private readonly TimeoutsManager _timeoutsManager;

        public TimeoutsService(TimeoutsManager timeoutsManager)
        {
            _timeoutsManager = timeoutsManager;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken = default)
        {
            return _timeoutsManager.Poll(stoppingToken);
        }
    }
}
