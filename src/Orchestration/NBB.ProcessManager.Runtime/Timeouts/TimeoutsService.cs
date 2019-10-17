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

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return _timeoutsManager.Poll(stoppingToken);
        }
    }
}