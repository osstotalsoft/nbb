// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;
using STAN.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.Nats.Internal
{
    public class StanConnectionProvider : IDisposable, ITransportMonitor
    {
        private readonly IOptions<NatsOptions> _natsOptions;
        private readonly ILogger<StanConnectionProvider> _logger;
        private IStanConnection _connection;
        private Exception _unrecoverableException;
        private AtomicLazy<IStanConnection> _lazyConnection;

        public event TransportErrorHandler OnError;

        public StanConnectionProvider(IOptions<NatsOptions> natsOptions, ILogger<StanConnectionProvider> logger)
        {
            _natsOptions = natsOptions;
            _logger = logger;
            _lazyConnection = new AtomicLazy<IStanConnection>(GetConnection);
        }

        public async Task ExecuteAsync(Func<IStanConnection, Task> action)
        {
            var connection = GetAndCheckConnection();

            await action(connection);
        }

        public void Execute(Action<IStanConnection> action)
        {
            var connection = GetAndCheckConnection();

            action(connection);
        }

        private IStanConnection GetAndCheckConnection()
        {
            ThrowIfUnrecoverableState();

            return _lazyConnection.Value;
        }

        private IStanConnection GetConnection()
        {
            var clientId = _natsOptions.Value.ClientId?.Replace(".", "_");
            var options = StanOptions.GetDefaultOptions();
            options.NatsURL = _natsOptions.Value.NatsUrl;

            options.ConnectionLostEventHandler = (_, args) =>
            {
                SetConnectionLostState(args.ConnectionException ?? new Exception("NATS connection was lost"));
            };

            //fix https://github.com/nats-io/csharp-nats-streaming/issues/28 
            options.PubAckWait = 30000;

            var cf = new StanConnectionFactory();
            _connection = cf.CreateConnection(_natsOptions.Value.Cluster, clientId + Guid.NewGuid(), options);

            _logger.LogInformation($"NATS connection to {_natsOptions.Value.NatsUrl} was established");

            return _connection;
        }

        private void SetConnectionLostState(Exception exception)
        {
            // Set the field to the current exception if not already set
            var existingException =
                Interlocked.CompareExchange(ref _unrecoverableException, exception, null);

            // Send the application stop signal only once
            if (existingException != null)
                return;

            _logger.LogError(exception, "NATS connection unrecoverable");

            ResetConnection();

            OnError?.Invoke(exception);
        }

        private void ResetConnection()
        {
            _connection?.Dispose();
            _connection = null;
            _lazyConnection = new AtomicLazy<IStanConnection>(GetConnection);

            Interlocked.CompareExchange(ref _unrecoverableException, null, _unrecoverableException);

            _logger.LogInformation("NATS connection was reset");
        }

        private void ThrowIfUnrecoverableState()
        {
            // For consistency, read the field using the same primitive used for writing instead of using Thread.VolatileRead
            var exception = Interlocked.CompareExchange(ref _unrecoverableException, null, null);
            if (exception != null)
            {
                throw new Exception("NATS connection encountered an unrecoverable exception", exception);
            }
        }

        private static bool IsUnrecoverableException(Exception ex) =>
            ex is NATSConnectionClosedException ||
            ex is StanConnectionClosedException ||
            ex is NATSConnectionException ||
            ex is StanConnectionException ||
            ex is NATSBadSubscriptionException ||
            ex is StanBadSubscriptionException ||
            ex is StanTimeoutException ||
            ex is NATSTimeoutException ||
            ex is NATSStaleConnectionException ||
            ex is NATSNoServersException ||
            ex is StanConnectRequestException ||
            ex is StanMaxPingsException;

        public void Dispose() => _connection?.Dispose();
    }

    public class AtomicLazy<T>
    {
        private readonly Func<T> _factory;

        private T _value;

        private bool _initialized;

        private object _lock;

        public AtomicLazy(Func<T> factory)
        {
            _factory = factory;
        }

        public AtomicLazy(T value)
        {
            _value = value;
            _initialized = true;
        }

        public T Value => LazyInitializer.EnsureInitialized(ref _value, ref _initialized, ref _lock, _factory);
    }
}
