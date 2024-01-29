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
using NBB.Messaging.Abstractions;
using Microsoft.Extensions.Options;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace NBB.Messaging.Host.Internal
{
    internal class MessagingHost : IMessagingHost, IDisposable
    {
        private readonly ILogger<MessagingHost> _logger;
        private readonly IEnumerable<IMessagingHostStartup> _configurators;
        private readonly IServiceProvider _serviceProvider;
        private readonly IServiceCollection _serviceCollection;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly ITransportMonitor _transportMonitor;
        private readonly IOptions<MessagingHostOptions> _hostOptions;
        private readonly List<HostedSubscription> _subscriptions = new();
        private readonly ExecutionMonitor _executionMonitor = new();

        private CancellationTokenSource _stoppingSource = new();
        private CancellationTokenSource _subscriberStopSource;

        private uint _isStarted = 0;
        private uint _isStarting = 0;
        private uint _isStopping = 0;
        private uint _isScheduledRestart = 0;

        public MessagingHost(ILogger<MessagingHost> logger, IEnumerable<IMessagingHostStartup> configurators,
            IServiceProvider serviceProvider, IServiceCollection serviceCollection,
            IHostApplicationLifetime applicationLifetime, ITransportMonitor transportMonitor,
            IOptions<MessagingHostOptions> hostOptions)
        {
            _logger = logger;
            _configurators = configurators;
            _serviceProvider = serviceProvider;
            _serviceCollection = serviceCollection;
            _applicationLifetime = applicationLifetime;
            _transportMonitor = transportMonitor;
            _hostOptions = hostOptions;
            _transportMonitor.OnError += OnTransportError;
        }

        public void ScheduleRestart(TimeSpan delay = default)
        {
            //if already stopping do nothing
            if (1 == Interlocked.Exchange(ref _isScheduledRestart, 1))
                return;

            _logger.LogInformation($"Messaging host is scheduled for restart in {delay.TotalSeconds} seconds");

            // TODO: when upgrading to .NET 8 remove the "suppressed" check since it doesn't throw any more
            // https://github.com/dotnet/runtime/pull/82912
            using IDisposable _ = ExecutionContext.IsFlowSuppressed() ? null : ExecutionContext.SuppressFlow();

            Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(delay);

                    await TryStopAsync();
                    await StartAsync();
                }
                finally
                {
                    Interlocked.Exchange(ref _isScheduledRestart, 0);
                }
            });
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            var policy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(_hostOptions.Value.StartRetryCount,
                    retryCount => TimeSpan.FromSeconds(Math.Min(0.1 * Math.Pow(2, retryCount - 1), 60)),
                    async (exception, _, retryCount, _) =>
                    {
                        _logger.LogError(exception, $"Messaging host failed to start");
                        await TryStopAsync();
                        _logger.LogInformation($"Retrying message host start, atempt {retryCount}");
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

                _subscriptions.Clear();
                Interlocked.Exchange(ref _isStarted, 0);
            }
        }

        private void OnTransportError(Exception ex)
        {
            var strategy = _hostOptions.Value.TransportErrorStrategy;

            if (strategy == TransportErrorStrategy.Retry)
            {
                ScheduleRestart(TimeSpan.FromSeconds(_hostOptions.Value.RestartDelaySeconds));
            }
            else if (strategy == TransportErrorStrategy.Throw)
            {
                _logger.LogCritical(ex, "Critical transport connection error, shutting down");
                _applicationLifetime.StopApplication();
            }
        }

        private async Task StartAsyncInternal(CancellationToken cancellationToken = default)
        {
            // if started do nothing
            if (_isStarted == 1)
                return;

            //if already starting do nothing
            if (1 == Interlocked.Exchange(ref _isStarting, 1))
                return;

            try
            {
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
                Interlocked.Exchange(ref _isStarted, 1);
            }
            finally
            {
                Interlocked.Exchange(ref _isStarting, 0);
            }
        }

        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            // if not started do nothing
            if (_isStarted == 0)
                return;

            //if already stopping do nothing
            if (1 == Interlocked.Exchange(ref _isStopping, 1))
                return;

            try
            {
                _logger.LogInformation("Messaging host is stopping");

                ExecuteCancellation(_stoppingSource);

                await _executionMonitor.WaitForHandlers(cancellationToken);

                foreach (var subscription in ((IEnumerable<IDisposable>)_subscriptions).Reverse())
                {
                    subscription.Dispose();
                }

                _subscriptions.Clear();

                _logger.LogInformation("Messaging host has stopped");

                Interlocked.Exchange(ref _isStarted, 0);
            }
            finally
            {
                Interlocked.Exchange(ref _isStopping, 0);
                _stoppingSource = new CancellationTokenSource();
            }

        }

        public bool IsRunning() => _isStarted == 1 && _isStarting == 0 && _isStopping == 0 && _isScheduledRestart == 0;

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

        public void Dispose()
        {
            _transportMonitor.OnError -= OnTransportError;
        }

    }
}
