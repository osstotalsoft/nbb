// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client;
using NBB.Messaging.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.JetStream.Internal
{
    public class NatsConnectionProvider : IDisposable, ITransportMonitor
    {
        private readonly IOptions<NatsOptions> _natsOptions;
        private readonly ILogger<NatsConnectionProvider> _logger;
        private IConnection _connection;
        private static readonly object InstanceLoker = new();
        private Exception _unrecoverableException;

        public event TransportErrorHandler OnError;

        public NatsConnectionProvider(IOptions<NatsOptions> natsOptions, ILogger<NatsConnectionProvider> logger)
        {
            _natsOptions = natsOptions;
            _logger = logger;
        }

        public async Task ExecuteAsync(Func<IConnection, Task> action)
        {
            var connection = GetAndCheckConnection();

            await action(connection);
        }

        public void Execute(Action<IConnection> action)
        {
            var connection = GetAndCheckConnection();

            action(connection);
        }

        private IConnection GetAndCheckConnection()
        {
            if (_connection == null)
                lock (InstanceLoker)
                {
                    if (_connection == null)
                        _connection = CreateConnection();
                }
            return _connection;
        }

        private IConnection CreateConnection()
        {
            var options = ConnectionFactory.GetDefaultOptions();
            options.Url = _natsOptions.Value.NatsUrl;

            //https://github.com/nats-io/nats.net/issues/804
            options.AllowReconnect = false;

            options.ClosedEventHandler += (_, args) =>
            {
                SetConnectionLostState(args.Error ?? new Exception("NATS connection was lost"));
            };

            _connection = new ConnectionFactory().CreateConnection(options);
            _logger.LogInformation($"NATS connection to {_natsOptions.Value.NatsUrl} was established");

            return _connection;
        }

        private void SetConnectionLostState(Exception exception)
        {
            _connection = null;

            // Set the field to the current exception if not already set
            var existingException = Interlocked.CompareExchange(ref _unrecoverableException, exception, null);

            // Send the application stop signal only once
            if (existingException != null)
                return;

            _logger.LogError(exception, "NATS connection unrecoverable");

            OnError?.Invoke(exception);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _connection?.Dispose();
            }
        }
    }
}
