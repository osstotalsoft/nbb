// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NBB.Core.Abstractions;
using NBB.EventStore.Abstractions;
using NBB.EventStore.AdoNet.Internal;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;

namespace NBB.EventStore.AdoNet
{
    public class AdoNetSnapshotRepository : ISnapshotRepository
    {
        private readonly Scripts _scripts;
        private readonly ILogger<AdoNetSnapshotRepository> _logger;
        private readonly IOptions<EventStoreOptions> _eventstoreOptions;

        public AdoNetSnapshotRepository(Scripts scripts,
            ILogger<AdoNetSnapshotRepository> logger, IOptions<EventStoreOptions> eventstoreOptions)
        {
            _scripts = scripts;
            _logger = logger;
            _eventstoreOptions = eventstoreOptions;
        }

        public async Task<SnapshotDescriptor> LoadSnapshotAsync(string stream, CancellationToken cancellationToken = default)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            SnapshotDescriptor snapshotDescriptor = null;

            using (var cnx = new SqlConnection(_eventstoreOptions.Value.ConnectionString))
            {
                cnx.Open();

                var cmd = new SqlCommand(_scripts.GetSnapshotForStream, cnx);

                cmd.Parameters.Add(new SqlParameter("@StreamId", SqlDbType.VarChar, 200)
                { Value = stream });

                cmd.Parameters.Add(new SqlParameter("@MaxStreamVersion", SqlDbType.Int)
                { Value = DBNull.Value }); // To be implemented for loading aggregate at a specific version

                foreach (var param in GetGlobalFilterParams())
                {
                    cmd.Parameters.Add(param);
                }

                using (var reader = await cmd.ExecuteReaderAsync(cancellationToken))
                {
                    if (reader.Read())
                    {
                        snapshotDescriptor = new SnapshotDescriptor(reader.GetString(0), reader.GetString(1), stream,
                            reader.GetInt32(2));
                    }
                }
            }

            stopWatch.Stop();
            _logger.LogDebug("AdoNetSnapshotRepository.LoadSnapshotAsync for {Stream} took {ElapsedMilliseconds} ms.",
                stream, stopWatch.ElapsedMilliseconds);

            return snapshotDescriptor;
        }

        public async Task StoreSnapshotAsync(string stream, SnapshotDescriptor snapshotDescriptor,
            CancellationToken cancellationToken = default)
        {
            if (snapshotDescriptor == null) throw new ArgumentException(nameof(snapshotDescriptor));

            var stopWatch = new Stopwatch();
            stopWatch.Start();


            using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            using (var cnx = new SqlConnection(_eventstoreOptions.Value.ConnectionString))
            {
                cnx.Open();

                var cmd = new SqlCommand(_scripts.SetSnapshotForStream, cnx);
                cmd.Parameters.Add(new SqlParameter("@SnapshotType", SqlDbType.VarChar, 300)
                { Value = snapshotDescriptor.SnapshotType });

                cmd.Parameters.Add(new SqlParameter("@SnapshotData", SqlDbType.NVarChar, -1)
                { Value = snapshotDescriptor.SnapshotData });

                cmd.Parameters.Add(new SqlParameter("@StreamVersion", SqlDbType.Int)
                { Value = snapshotDescriptor.AggregateVersion });

                cmd.Parameters.Add(new SqlParameter("@StreamId", SqlDbType.VarChar, 200)
                { Value = stream });

                foreach (var param in GetGlobalFilterParams())
                {
                    cmd.Parameters.Add(param);
                }

                try
                {
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }
                catch (SqlException ex) when (ex.Errors[0].Message == "VersionAlreadyExists")
                {
                    if (snapshotDescriptor.AggregateVersion == 0)
                        throw new ConcurrencyUnrecoverableException("SnapshotStore unrecoverable concurrency exception", ex);

                    throw new ConcurrencyException("SnapshotStore concurrency exception", ex);
                }

                ts.Complete();
            }

            stopWatch.Stop();
            _logger.LogDebug("AdoNetSnapshotRepository.StoreSnapshotAsync for stream {Stream} took {ElapsedMilliseconds} ms.",
                stream, stopWatch.ElapsedMilliseconds);
        }

        protected virtual IEnumerable<SqlParameter> GetGlobalFilterParams()
        {
            yield break;
        }
    }
}
