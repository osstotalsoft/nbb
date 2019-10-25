using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace NBB.ProcessManager.Runtime.Timeouts
{
    public class TimeoutsManager : IDisposable
    {
        private readonly ITimeoutsRepository _timeoutsRepository;
        private readonly ILogger<TimeoutsManager> _logger;
        private readonly IMediator _mediator;
        static readonly TimeSpan MaxNextRetrievalDelay = TimeSpan.FromMinutes(1);
        static readonly TimeSpan NextRetrievalPollSleep = TimeSpan.FromMilliseconds(1000);
        readonly Func<DateTime> _currentTimeProvider;
        readonly object _lockObject = new object();
        public DateTime NextRetrieval { get; private set; }
        DateTime _startSlice;

        public TimeoutsManager(ITimeoutsRepository timeoutsRepository, ILogger<TimeoutsManager> logger, IServiceScopeFactory scopeFactory, Func<DateTime> currentTimeProvider)
        {
            _timeoutsRepository = timeoutsRepository;
            _logger = logger;
            _mediator = scopeFactory.CreateScope().ServiceProvider.GetRequiredService<IMediator>();
            _currentTimeProvider = currentTimeProvider;

            var now = _currentTimeProvider();
            _startSlice = now.AddYears(-10);
            NextRetrieval = now;
        }

        public async Task Poll(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await SpinOnce(cancellationToken).ConfigureAwait(false);
                    await Task.Delay(NextRetrievalPollSleep, cancellationToken).ConfigureAwait(false);
                }
                catch (OperationCanceledException)
                {
                    // ok, since the InnerPoll could observe the token
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to fetch timeouts from the timeout storage");
                }
            }
        }

        internal async Task SpinOnce(CancellationToken cancellationToken)
        {
            if (NextRetrieval > _currentTimeProvider() || cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _logger.LogDebug("Polling for timeouts at {0}.", _currentTimeProvider());
            var timeoutChunk = await _timeoutsRepository.GetNextBatch(_startSlice).ConfigureAwait(false);

            foreach (var timeoutData in timeoutChunk.DueTimeouts)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                await _mediator.Publish(new TimeoutOccured(timeoutData.ProcessManagerInstanceId, timeoutData.Message), cancellationToken);

                if (_startSlice < timeoutData.DueDate)
                {
                    _startSlice = timeoutData.DueDate;
                }
            }

            lock (_lockObject)
            {
                var nextTimeToQuery = timeoutChunk.NextTimeToQuery;

                // we cap the next retrieval to max 1 minute this will make sure that we trip the circuit breaker if we
                // loose connectivity to our storage. This will also make sure that timeouts added (during migration) direct to storage
                // will be picked up after at most 1 minute
                var maxNextRetrieval = _currentTimeProvider() + MaxNextRetrievalDelay;

                NextRetrieval = nextTimeToQuery > maxNextRetrieval ? maxNextRetrieval : nextTimeToQuery;

                _logger.LogDebug("Polling next retrieval is at {0}.", NextRetrieval.ToLocalTime());
            }
        }

        public void NewTimeoutRegistered(DateTime expiryTime)
        {
            lock (_lockObject)
            {
                if (NextRetrieval > expiryTime)
                {
                    NextRetrieval = expiryTime;
                }
            }
        }


        public void Dispose()
        {
        }
    }
}