// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

internal static class SqlServerExtensions
{
    /// <summary>
    /// Adds a connection string for a database on the server described by <paramref name="server"/>,
    /// by appending <c>Database=</c> to the server connection string.
    /// </summary>
    internal static IResourceBuilder<ConnectionStringResource> AddDatabase(
        this IResourceBuilder<IResourceWithConnectionString> server, string name, string databaseName) =>
        server.ApplicationBuilder
            .AddConnectionString(name, ReferenceExpression.Create($"{server.Resource};Database={databaseName}"))
            .WithParentRelationship(server);
}
