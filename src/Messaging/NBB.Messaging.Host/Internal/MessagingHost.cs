// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Polly;
using Microsoft.Extensions.Hosting;

namespace NBB.Messaging.Host.Internal
{
    internal class MessagingHost : IMessagingHost
    {
        private readonly ILogger<MessagingHost> _logger;
        private readonly IEnumerable<IMessagingHostStartup> _configurators;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceCollection _serviceCollection;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly List<HostedSubscription> _subscriptions = new();
        private readonly ExecutionMonitor _executionMonitor = new();

        private CancellationTokenSource _stoppingSource = new CancellationTokenSource();
        private CancellationTokenSource _subscriberStopSource;

        private bool _isStarted;
        private bool _isStarting;

        public MessagingHost(ILogger<MessagingHost> logger, IEnumerable<IMessagingHostStartup> configurators,
            IServiceProvider serviceProvider, IServiceCollection serviceCollection, IHostApplicationLifetime applicationLifetime)
        {
            _logger = logger;
            _configurators = configurators;
            _serviceProvider = serviceProvider;
            _serviceCollection = serviceCollection;
            _applicationLifetime = applicationLifetime;
        }

        public void ScheduleRestart(TimeSpan delay = default)
        {
            Task.Run(async () =>
            {
                await Task.Delay(delay);

                if (!ExecutionContext.IsFlowSuppressed())
                    ExecutionContext.SuppressFlow();

                await TryStopAsync();

                _stoppingSource = new CancellationTokenSource();

                await StartAsync();
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(20, _ => TimeSpan.FromSeconds(10),
                    async (_, _, retryCount, _) => {
                        _logger.LogInformation("Retrying message host start");
                        await TryStopAsync();
                    });

            var result = await policy.ExecuteAndCaptureAsync(StartAsyncInternal, cancellationToken);

            if (result.Outcome == OutcomeType.Failure)
            {
                _logger.LogCritical(result.FinalException, "Messaging host could not start");
                _applicationLifetime.StopApplication();
            }
        }

        private async Task TryStopAsync()
        {
            try
            {
                await StopAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Message host could not be gracefully stopped");
            }
        }

        private async Task StartAsyncInternal(CancellationToken cancellationToken = default)
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

                foreach (var subscriber in configuration.Subscribers)
                {
                    var hostedSubscriberType = typeof(HostedSubscriber<>).MakeGenericType(subscriber.MessageType);
                    var hostedSubscriber =
                        (IHostedSubscriber)ActivatorUtilities.CreateInstance(_serviceProvider,
                            hostedSubscriberType, _executionMonitor);

                    var subscription = await hostedSubscriber.SubscribeAsync(subscriber.Pipeline, subscriber.Options, subscriptionToken);

                    _subscriptions.Add(subscription);
                }

                _logger.LogInformation("Messaging host has started");
                _isStarted = true;
            }
            finally
            {
                _isStarting = false;
            }            
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
