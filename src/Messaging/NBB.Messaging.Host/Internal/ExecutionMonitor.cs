using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host.Internal
{
    internal class ExecutionMonitor
    {
        private int _executingHandlerCount;

        private void StartHandler()
            => Interlocked.Increment(ref _executingHandlerCount);

        private void StopHandler()
            => Interlocked.Decrement(ref _executingHandlerCount);

        public async Task Handle(Func<Task> action)
        {
            try
            {
                StartHandler();
                await action();
            }
            finally
            {
                StopHandler();
            }
        }

        public Task WaitForHandlers(CancellationToken token)
        {
            var spinWait = new SpinWait();
            while (_executingHandlerCount > 0)
            {
                if (token.IsCancellationRequested)
                    break;

                spinWait.SpinOnce();
            }

            return Task.CompletedTask;
        }
    }
}
