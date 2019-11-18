using EventStore.ClientAPI;
using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.GetEventStore.Internal;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.GetEventStore
{
    public class GetEventStoreSubscriber : IEventStoreSubscriber
    {
        private readonly ISerDes _serDes;
        private readonly ILogger<GetEventStoreSubscriber> _logger;

        public GetEventStoreSubscriber(ISerDes serDes, ILogger<GetEventStoreSubscriber> logger)
        {
            _serDes = serDes;
            _logger = logger;
        }

        public async Task SubscribeToAllAsync(Func<IEvent, Task> handler, CancellationToken cancellationToken)
        {
            using (var connection = await GetConnectionAsync())
            {

                //try
                //{
                //    var settings = PersistentSubscriptionSettings.Create()
                //        .DoNotResolveLinkTos()
                //        .StartFromCurrent();


                //    await connection.CreatePersistentSubscriptionAsync("NBB.Contracts.Domain.ContractAggregate.Contract", "x",
                //        settings, new UserCredentials("admin", "changeit"));
                //}
                //catch (Exception ex)
                //{

                //}


                var sub = await connection.ConnectToPersistentSubscriptionAsync("NBB.Contracts.Domain.ContractAggregate.Contract", "x", (x, re) =>
                {

                    //wtf?
                    if (!re.Event.IsJson)
                    {
                        return Task.CompletedTask;
                    }

                    var metadata = _serDes.Deserialize<EventMetadata>(re.Event.Metadata);
                    var eventType = metadata.GetEventType();

                    _logger.LogDebug("GetEventStore subscriber received message of type {EventType}", eventType.FullName);

                    var @event = _serDes.Deserialize(re.Event.Data, eventType) as IEvent;
                    var res = handler(@event);

                    return res;

                }, (_,r, exc) =>
                {

                }, null, 10, true);


                await cancellationToken.WhenCanceled();
            }

        }


        private async Task<IEventStoreConnection> GetConnectionAsync()
        {
            var settings = ConnectionSettings.Create()
                //.UseConsoleLogger()
                //.EnableVerboseLogging()
                .KeepReconnecting();
            var connection = EventStoreConnection.Create(settings, new IPEndPoint(IPAddress.Loopback, 1113));
            await connection.ConnectAsync();

            return connection;

        }
    }
}
