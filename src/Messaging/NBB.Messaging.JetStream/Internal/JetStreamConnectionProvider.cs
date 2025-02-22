// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NATS.Client.Core;
using System;
using System.Threading.Tasks;

namespace NBB.Messaging.JetStream.Internal
{
    public class JetStreamConnectionProvider : IAsyncDisposable
    {
        private readonly IOptions<JetStreamOptions> _natsOptions;
        private readonly ILogger<JetStreamConnectionProvider> _logger;
        private readonly ILoggerFactory _loggerFactory;
        private INatsConnection _connection;
        private static readonly object InstanceLoker = new();

        public JetStreamConnectionProvider(IOptions<JetStreamOptions> natsOptions,
            ILogger<JetStreamConnectionProvider> logger, ILoggerFactory loggerFactory)
        {
            _natsOptions = natsOptions;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        public INatsConnection GetConnection()
        {
            var connection = GetAndCheckConnection();
            return connection;
        }

        private INatsConnection GetAndCheckConnection()
        {
            if (_connection == null)
                lock (InstanceLoker)
                {
                    if (_connection == null)
                        _connection = CreateConnection();
                }
            return _connection;
        }

        private INatsConnection CreateConnection()
        {
            var options = NatsOpts.Default with { Url = _natsOptions.Value.NatsUrl, LoggerFactory = _loggerFactory };

            _connection = new NatsConnection(options);
            //_connection.ConnectionDisconnected += Connection_ConnectionDisconnected;
            //_connection.ReconnectFailed += Connection_ReconnectFailed;
            _logger.LogInformation($"NATS Jetstream connection to {_natsOptions.Value.NatsUrl} was established");

            return _connection;
        }

        private ValueTask Connection_ConnectionDisconnected(object sender, NatsEventArgs args)
        {
            _logger.LogWarning($"NATS Jetstream connection was disconnected, {args.Message}");
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            return _connection?.DisposeAsync() ?? ValueTask.CompletedTask;
        }
    }
}
