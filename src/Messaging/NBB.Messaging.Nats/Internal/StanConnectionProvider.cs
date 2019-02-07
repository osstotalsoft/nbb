using System;
using Microsoft.Extensions.Configuration;
using STAN.Client;

namespace NBB.Messaging.Nats.Internal
{
    public class StanConnectionProvider : IDisposable
    {
        private readonly IConfiguration _configuration;
        private IStanConnection _connection;

        public StanConnectionProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public IStanConnection GetConnection()
        {
            if (_connection == null)
            {
                lock (typeof(StanConnectionProvider))
                {
                    if (_connection == null)
                    {
                        var natsUrl = _configuration.GetSection("Messaging").GetSection("Nats")["natsUrl"];
                        var cluster = _configuration.GetSection("Messaging").GetSection("Nats")["cluster"];
                        var clientId = _configuration.GetSection("Messaging").GetSection("Nats")["clientId"]?.Replace(".", "_");
                        var options = StanOptions.GetDefaultOptions();
                        options.NatsURL = natsUrl;

                        //fix https://github.com/nats-io/csharp-nats-streaming/issues/28 
                        options.PubAckWait = 30000;

                        var cf = new StanConnectionFactory();
                        _connection = cf.CreateConnection(cluster, clientId + Guid.NewGuid(), options);
                    }
                }
            }

            return _connection;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}
