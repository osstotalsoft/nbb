// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

var builder = DistributedApplication.CreateBuilder(args);

// External infrastructure: the SQL Server connection string comes from user secrets, the NATS JetStream server from appsettings
var sql = builder.AddConnectionString("sql");
var jetstream = builder.AddConnectionString("jetstream");

var todoDb = sql.AddDatabase("todo-db", "NBB_Todo");

// The migrations run mono-tenant; the API and worker are multi-tenant and all tenants share the default database
var todoMigrations = builder.AddProject<Projects.NBB_Todo_Migrations>("todo-migrations")
    .WithReference(todoDb, connectionName: "DefaultConnection");

builder.AddProject<Projects.NBB_Todo_Api>("todo-api")
    .WithEnvironment("MultiTenancy__Defaults__ConnectionStrings__DefaultConnection", todoDb)
    .WithJetStream(jetstream)
    .WithHttpHealthCheck("/health")
    .WaitForCompletion(todoMigrations);

builder.AddProject<Projects.NBB_Todo_Worker>("todo-worker")
    .WithEnvironment("MultiTenancy__Defaults__ConnectionStrings__DefaultConnection", todoDb)
    .WithJetStream(jetstream)
    .WaitForCompletion(todoMigrations);

builder.Build().Run();
