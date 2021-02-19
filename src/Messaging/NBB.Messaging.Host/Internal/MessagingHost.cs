using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NBB.Messaging.Host.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host.Internal
{
    internal class MessagingHost : IMessagingHost
    {
        private readonly ILogger<MessagingHost> _logger;
        private readonly IEnumerable<IMessagingHostStartup> _configurators;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceCollection _serviceCollection;
        private readonly List<HostedSubscription> _subscriptions = new();
        private readonly ExecutionMonitor _executionMonitor = new();

        private CancellationTokenSource _stoppingSource = new CancellationTokenSource();
        private CancellationTokenSource _subscriberStopSource;

        private bool _isStarted;
        private bool _isStarting;

        public MessagingHost(ILogger<MessagingHost> logger, IEnumerable<IMessagingHostStartup> configurators,
            IServiceProvider serviceProvider, IServiceCollection serviceCollection)
        {
            _logger = logger;
            _configurators = configurators;
            _serviceProvider = serviceProvider;
            _serviceCollection = serviceCollection;
        }

        public void ScheduleRestart()
        {
            Task.Run(async () =>
            {
                if (!ExecutionContext.IsFlowSuppressed())
                    ExecutionContext.SuppressFlow();

                await StopAsync();

                _stoppingSource = new CancellationTokenSource();

                await StartAsync();
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Add synchronization
            if (_isStarted || _isStarting)
                return;

            try
            {
                _isStarting = true;
                _logger.LogInformation("Messaging host is starting");

                _subscriberStopSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stoppingSource.Token);
                CancellationToken subscriptionToken = _subscriberStopSource.Token;

                subscriptionToken.ThrowIfCancellationRequested();

                var builder = new MessagingHostConfigurationBuilder(_serviceProvider, _serviceCollection);
                foreach (var configurator in _configurators)
                {
                    await configurator.Configure(builder);
                }

                var configuration = builder.Build();

                foreach (var subscriberGroup in configuration.SubscriberGroups)
                {
                    foreach (var (messageType, options) in subscriberGroup.Subscribers)
                    {
                        var hostedSubscriberType = typeof(HostedSubscriber<>).MakeGenericType(messageType);
                        var hostedSubscriber =
                            (IHostedSubscriber)ActivatorUtilities.CreateInstance(_serviceProvider,
                                hostedSubscriberType, _executionMonitor);

                        var subscription = await hostedSubscriber.SubscribeAsync(subscriberGroup.Pipeline, options, subscriptionToken);

                        _subscriptions.Add(subscription);
                    }
                }
            }
            finally
            {
                _isStarting = false;
            }

            _logger.LogInformation("Messaging host has started");

            _isStarted = true;
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Add synchronization
            if (!_isStarted) return;
            
            _logger.LogInformation("Messaging host is stopping");

            ExecuteCancellation(_stoppingSource);

            await _executionMonitor.WaitForHandlers(cancellationToken);

            foreach (var subscription in ((IEnumerable<IDisposable>)_subscriptions).Reverse())
            {
                subscription.Dispose();
            }

            _subscriptions.Clear();

            _logger.LogInformation("Messaging host has stopped");

            _isStarted = false;
        }

        private void ExecuteCancellation(CancellationTokenSource cancel)
        {
            // Noop if this is already cancelled
            if (cancel.IsCancellationRequested)
            {
                return;
            }

            // Run the cancellation token callbacks
            cancel.Cancel(throwOnFirstException: false);
        }
    }
}
