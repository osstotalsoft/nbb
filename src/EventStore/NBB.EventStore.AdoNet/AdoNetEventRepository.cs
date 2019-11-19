using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Server;
using NBB.Core.Abstractions;
using NBB.EventStore.AdoNet.Internal;

namespace NBB.EventStore.AdoNet
{
    public class AdoNetEventRepository : IEventRepository
    {
        private readonly Scripts _scripts;
        private readonly string _connectionString;
        private readonly ILogger<AdoNetEventRepository> _logger;

        private readonly SqlMetaData[] _appendEventsMetadata = new List<SqlMetaData>
        {
            new SqlMetaData("OrderNo", SqlDbType.Int, true, false, SortOrder.Unspecified, -1),
            new SqlMetaData("EventId", SqlDbType.UniqueIdentifier),
            new SqlMetaData("EventData", SqlDbType.NVarChar, SqlMetaData.Max),
            new SqlMetaData("EventType", SqlDbType.VarChar, 300),
            new SqlMetaData("CorrelationId", SqlDbType.UniqueIdentifier),
            

        }.ToArray();

        public AdoNetEventRepository(Scripts scripts, IConfiguration configuration, ILogger<AdoNetEventRepository> logger)
        {
            _scripts = scripts;
            _connectionString = configuration.GetSection("EventStore").GetSection("NBB")["ConnectionString"];
            _logger = logger;
        }

        public async Task AppendEventsToStreamAsync(string streamId, IList<EventDescriptor> eventDescriptors, int? expectedVersion, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            if (expectedVersion.HasValue)
            {
                await AppendEventsToStreamExpectedVersionAsync(streamId, eventDescriptors, expectedVersion.Value,
                    cancellationToken);
            }
            else
            {
                await AppendEventsToStreamExpectedVersionAnyAsync(streamId, eventDescriptors, cancellationToken);
            }

            stopWatch.Stop();
            _logger.LogDebug("AdoNetEventRepository.AppendEventsToStreamAsync for {Stream} took {ElapsedMilliseconds} ms.", streamId, stopWatch.ElapsedMilliseconds);
        }

        public async Task<IList<EventDescriptor>> GetEventsFromStreamAsync(string stream, int? startFromVersion, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var eventDescriptors = new List<EventDescriptor>();

            using (var cnx = new SqlConnection(_connectionString))
            {
                cnx.Open();

                var cmd = new SqlCommand(_scripts.GetEventsFromStream, cnx);
                cmd.Parameters.Add(new SqlParameter("@StreamId", SqlDbType.VarChar, 200) {Value = stream});
                cmd.Parameters.Add(new SqlParameter("@MinStreamVersion", SqlDbType.Int) {Value = (object)startFromVersion ?? DBNull.Value});
                cmd.Parameters.Add(new SqlParameter("@MaxStreamVersion", SqlDbType.Int) {Value = DBNull.Value}); // To be implemented for loading aggregate at a specific version

                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    while (reader.Read())
                    {
                        var ed = new EventDescriptor(reader.GetGuid(0), reader.GetString(1), reader.GetString(2), stream, reader.IsDBNull(3) ? (Guid?) null : reader.GetGuid(3));
                        eventDescriptors.Add(ed);
                    }
                }
            }

            stopWatch.Stop();
            _logger.LogDebug("AdoNetEventRepository.GetEventsFromStreamAsync for {Stream} took {ElapsedMilliseconds} ms.", stream, stopWatch.ElapsedMilliseconds);

            return eventDescriptors;
        }


        private async Task AppendEventsToStreamExpectedVersionAsync(string streamId, IList<EventDescriptor> eventDescriptors, int expectedVersion, CancellationToken cancellationToken = default)
        {
            if (!eventDescriptors.Any())
                return;

            var sqlDataRecords = CreateSqlDataRecords(eventDescriptors);

            using (var cnx = new SqlConnection(_connectionString))
            {
                cnx.Open();

                var cmd = new SqlCommand(_scripts.AppendEventsToStreamExpectedVersion, cnx);
                cmd.Parameters.Add(new SqlParameter("@StreamId", SqlDbType.VarChar, 200)
                {
                    Value = streamId
                });
                cmd.Parameters.Add(new SqlParameter("@ExpectedVersion", SqlDbType.Int)
                {
                    Value = expectedVersion
                });

                var eventsParam = CreateNewEventsSqlParameter(sqlDataRecords);
                cmd.Parameters.Add(eventsParam);

                try
                {
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (SqlException ex)
                {
                    var sqlError = ex.Errors[0];
                    if (sqlError.Message == "WrongExpectedVersion")
                    {
                        if (expectedVersion == 0)
                        {
                            throw new ConcurrencyUnrecoverableException("EventStore unrecoverable concurrency exception", ex);
                        }

                        throw new ConcurrencyException("EventStore concurrency exception", ex);
                    }

                    throw;
                }
            }
        }

        private async Task AppendEventsToStreamExpectedVersionAnyAsync(string streamId, IList<EventDescriptor> eventDescriptors, CancellationToken cancellationToken = default)
        {
            if (!eventDescriptors.Any())
                return;

            var sqlDataRecords = CreateSqlDataRecords(eventDescriptors);

            using (var cnx = new SqlConnection(_connectionString))
            {
                cnx.Open();

                var cmd = new SqlCommand(_scripts.AppendEventsToStreamExpectedVersionAny, cnx);
                cmd.Parameters.Add(new SqlParameter("@StreamId", SqlDbType.VarChar, 200)
                {
                    Value = streamId
                });

                var eventsParam = CreateNewEventsSqlParameter(sqlDataRecords);
                cmd.Parameters.Add(eventsParam);

                await cmd.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        private SqlParameter CreateNewEventsSqlParameter(SqlDataRecord[] sqlDataRecords)
        {
            var eventsParam = new SqlParameter("@NewEvents", SqlDbType.Structured)
            {
                TypeName = $"NewEventStoreEvents",
                Value = sqlDataRecords
            };
            return eventsParam;
        }

        private SqlDataRecord[] CreateSqlDataRecords(IList<EventDescriptor> events)
        {
            var sqlDataRecords = events.Select(ev =>
            {
                var record = new SqlDataRecord(_appendEventsMetadata);
                record.SetGuid(1, ev.EventId);
                record.SetString(2, ev.EventData);
                record.SetString(3, ev.EventType);
                if (ev.CorrelationId.HasValue)
                    record.SetGuid(4, ev.CorrelationId.Value);

                return record;
            }).ToArray();
            return sqlDataRecords;
        }
    }
}
