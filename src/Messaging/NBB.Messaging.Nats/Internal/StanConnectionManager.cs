using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NATS.Client;
using STAN.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Nats.Internal
{
    public class StanConnectionProvider : IDisposable
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<StanConnectionProvider> _logger;
        private readonly IHostApplicationLifetime _applicationLifetime;
        private IStanConnection _connection;
        private Exception _unrecoverableException;
        private readonly Lazy<IStanConnection> _lazyConnection;

        public StanConnectionProvider(IConfiguration configuration, ILogger<StanConnectionProvider> logger,
            IHostApplicationLifetime applicationLifetime)
        {
            _configuration = configuration;
            _logger = logger;
            _applicationLifetime = applicationLifetime;
            _lazyConnection = new Lazy<IStanConnection>(GetConnection);
        }

        public async Task ExecuteAsync(Func<IStanConnection, Task> action)
        {
            var connection = GetAndCheckConnection();
            try
            {
                await action(connection);
            }
            catch (Exception ex)
                when (IsUnrecoverableException(ex))
            {
                SetUnrecoverableState(ex);
                throw;
            }
        }

        public void Execute(Action<IStanConnection> action)
        {
            var connection = GetAndCheckConnection();
            try
            {
                action(connection);
            }
            catch (Exception ex)
                when (IsUnrecoverableException(ex))
            {
                SetUnrecoverableState(ex);
                throw;
            }
        }

        private IStanConnection GetAndCheckConnection()
        {
            ThrowIfUnrecoverableState();
            try
            {
                // Exception from the lazy factory method is cached
                return _lazyConnection.Value;
            }
            catch (Exception ex)
            {
                SetUnrecoverableState(ex);
                throw;
            }
        }

        private IStanConnection GetConnection()
        {
            var natsUrl = _configuration.GetSection("Messaging").GetSection("Nats")["natsUrl"];
            var cluster = _configuration.GetSection("Messaging").GetSection("Nats")["cluster"];
            var clientId = _configuration.GetSection("Messaging").GetSection("Nats")["clientId"]
                ?.Replace(".", "_");
            var options = StanOptions.GetDefaultOptions();
            options.NatsURL = natsUrl;

            options.ConnectionLostEventHandler = (obj, args) =>
            {
                SetUnrecoverableState(args.ConnectionException ?? new Exception("NATS connection was lost"));
            };

            //fix https://github.com/nats-io/csharp-nats-streaming/issues/28 
            options.PubAckWait = 30000;

            var cf = new StanConnectionFactory();
            _connection = cf.CreateConnection(cluster, clientId + Guid.NewGuid(), options);

            return _connection;
        }

        private void SetUnrecoverableState(Exception exception)
        {
            // Set the field to the current exception if not already set
            var existingException =
                Interlocked.CompareExchange(ref _unrecoverableException, exception, null);

            // Send the application stop signal only once
            if (existingException != null)
                return;

            _logger.LogCritical(exception, "NATS connection unrecoverable");
            _applicationLifetime.StopApplication();

            Dispose();
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

        private static bool IsUnrecoverableException(Exception ex)
        {
            return
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
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
