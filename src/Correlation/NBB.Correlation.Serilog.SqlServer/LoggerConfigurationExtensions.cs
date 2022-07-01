// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using System;
using System.Collections.Generic;
using Serilog.Sinks.MSSqlServer;
using System.Linq;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace NBB.Correlation.Serilog.SqlServer
{
    public static class LoggerConfigurationExtensions
    {
        /// <summary>
        /// Adds a sink that writes log events to a table in a MSSqlServer database.
        /// Adds a column named CorrelationId to the resulting table
        /// Create a database and execute the table creation script found here
        /// https://gist.github.com/mivano/10429656
        /// or use the autoCreateSqlTable option.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="tableName">Name of the table to store the events in.</param>
        /// <param name="schemaName">Name of the schema for the table to store the data in. The default is 'dbo'.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions"></param>
        /// <param name="additionalColumns">Additional columns to be added</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="T:System.ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration MsSqlServerWithAdditionalColumns(
          this LoggerSinkConfiguration loggerConfiguration,
          string connectionString,
          string tableName,
          LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
          int batchPostingLimit = 50,
          TimeSpan? period = null,
          IFormatProvider formatProvider = null,
          bool autoCreateSqlTable = false,
          ColumnOptions columnOptions = null,
          string schemaName = "dbo",
          Dictionary<string, SqlDbType> additionalColumns = null
          )
        {
            if (columnOptions == null)
            {
                columnOptions = new ColumnOptions();
            }

            if (columnOptions.AdditionalColumns == null)
            {
                columnOptions.AdditionalColumns = new List<SqlColumn>();
            }

            if (additionalColumns != null)
            {
                foreach (var columnName in additionalColumns.Keys)
                {
                    if (columnOptions.AdditionalColumns.Any(x => x.ColumnName.Equals(columnName)))
                    {
                        continue;
                    }
                    columnOptions.AdditionalColumns.Add(
                        new SqlColumn
                        {
                            ColumnName = columnName,
                            DataType = additionalColumns[columnName]
                        });
                }
            }

            return loggerConfiguration.MSSqlServer(
                connectionString,
                new MSSqlServerSinkOptions
                {
                    TableName = tableName,
                    BatchPostingLimit = batchPostingLimit,
                    BatchPeriod = period ?? TimeSpan.FromSeconds(5),
                    AutoCreateSqlTable = autoCreateSqlTable,
                    SchemaName = schemaName,
                }, null, null,
                restrictedToMinimumLevel,
                formatProvider,
                columnOptions);
        }

        /// <summary>
        /// Adds a sink that writes log events to a table in a MSSqlServer database.
        /// Adds a column named CorrelationId to the resulting table
        /// Create a database and execute the table creation script found here
        /// https://gist.github.com/mivano/10429656
        /// or use the autoCreateSqlTable option.
        /// </summary>
        /// <param name="loggerConfiguration">The logger configuration.</param>
        /// <param name="connectionString">The connection string to the database where to store the events.</param>
        /// <param name="tableName">Name of the table to store the events in.</param>
        /// <param name="schemaName">Name of the schema for the table to store the data in. The default is 'dbo'.</param>
        /// <param name="restrictedToMinimumLevel">The minimum log event level required in order to write an event to the sink.</param>
        /// <param name="batchPostingLimit">The maximum number of events to post in a single batch.</param>
        /// <param name="period">The time to wait between checking for event batches.</param>
        /// <param name="formatProvider">Supplies culture-specific formatting information, or null.</param>
        /// <param name="autoCreateSqlTable">Create log table with the provided name on destination sql server.</param>
        /// <param name="columnOptions"></param>
        /// <param name="correlationId">CorrelationId parameter name. Default 'CorrelationId'</param>
        /// <param name="correlationIdType">Type of the correlationId parameter. Default is System.Guid</param>
        /// <returns>Logger configuration, allowing configuration to continue.</returns>
        /// <exception cref="T:System.ArgumentNullException">A required parameter is null.</exception>
        public static LoggerConfiguration MsSqlServerWithCorrelation(
          this LoggerSinkConfiguration loggerConfiguration,
          string connectionString,
          string tableName,
          LogEventLevel restrictedToMinimumLevel = LogEventLevel.Verbose,
          int batchPostingLimit = 50,
          TimeSpan? period = null,
          IFormatProvider formatProvider = null,
          bool autoCreateSqlTable = false,
          ColumnOptions columnOptions = null,
          string schemaName = "dbo",
          string correlationId = "CorrelationId",
          IConfigurationSection columnOptionsSection = null
          )
        {
            if (columnOptions == null)
            {
                columnOptions = new ColumnOptions();
            }

            if (columnOptions.AdditionalColumns == null)
            {
                columnOptions.AdditionalColumns = new List<SqlColumn>();
            }

            if (!columnOptions.AdditionalColumns.Any(x => x.ColumnName.Equals(correlationId)))
            {
                columnOptions.AdditionalColumns.Add(
                    new SqlColumn
                    {
                        ColumnName = correlationId,
                        DataType = SqlDbType.UniqueIdentifier
                    });
            }

            return loggerConfiguration.MSSqlServer(
                connectionString,
                new MSSqlServerSinkOptions
                {
                    TableName = tableName,
                    BatchPostingLimit = batchPostingLimit,
                    BatchPeriod = period ?? TimeSpan.FromSeconds(5),
                    AutoCreateSqlTable = autoCreateSqlTable,
                    SchemaName = schemaName,
                }, null, null,
                restrictedToMinimumLevel,
                formatProvider,
                columnOptions, columnOptionsSection);
        }
    }
}
