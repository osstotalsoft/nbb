// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);

// External infrastructure: the SQL Server connection string comes from user secrets, the NATS JetStream server from appsettings
var sql = builder.AddConnectionString("sql");
var jetstream = builder.AddConnectionString("jetstream");

var contractsDb = sql.AddDatabase("contracts-db", "NBB_Contracts");
var invoicesDb = sql.AddDatabase("invoices-db", "NBB_Invoices");
var paymentsDb = sql.AddDatabase("payments-db", "NBB_Payments");

// Contracts
var contractsMigrations = builder.AddProject<Projects.NBB_Contracts_Migrations>("contracts-migrations")
    .WithReference(contractsDb, connectionName: "DefaultConnection")
    .WithEnvironment("EventStore__NBB__ConnectionString", contractsDb);

builder.AddProject<Projects.NBB_Contracts_Api>("contracts-api")
    .WithReference(contractsDb, connectionName: "DefaultConnection")
    .WithJetStream(jetstream)
    .WithHttpHealthCheck("/health")
    .WaitForCompletion(contractsMigrations);

builder.AddProject<Projects.NBB_Contracts_Worker>("contracts-worker")
    .WithReference(contractsDb, connectionName: "DefaultConnection")
    .WithEnvironment("EventStore__NBB__ConnectionString", contractsDb)
    .WithJetStream(jetstream)
    .WaitForCompletion(contractsMigrations);

// Invoices
var invoicesMigrations = builder.AddProject<Projects.NBB_Invoices_Migrations>("invoices-migrations")
    .WithReference(invoicesDb, connectionName: "DefaultConnection")
    .WithEnvironment("EventStore__NBB__ConnectionString", invoicesDb);

// Invoices:UseFSharp switches to the F# implementation (NBB.Invoices.FSharp), a drop-in replacement
// that serves the same API and handles the same published language messages
var useFSharpInvoices = builder.Configuration.GetValue<bool>("Invoices:UseFSharp");

var invoicesApi = useFSharpInvoices
    ? builder.AddProject<Projects.NBB_Invoices_FSharp_Api>("invoices-api")
    : builder.AddProject<Projects.NBB_Invoices_Api>("invoices-api");

invoicesApi
    .WithReference(invoicesDb, connectionName: "DefaultConnection")
    .WithJetStream(jetstream)
    .WithHttpHealthCheck("/health")
    .WaitForCompletion(invoicesMigrations);

var invoicesWorker = useFSharpInvoices
    ? builder.AddProject<Projects.NBB_Invoices_FSharp_Worker>("invoices-worker")
    : builder.AddProject<Projects.NBB_Invoices_Worker>("invoices-worker");

invoicesWorker
    .WithReference(invoicesDb, connectionName: "DefaultConnection")
    .WithEnvironment("EventStore__NBB__ConnectionString", invoicesDb)
    .WithJetStream(jetstream)
    .WaitForCompletion(invoicesMigrations);

// Payments
var paymentsMigrations = builder.AddProject<Projects.NBB_Payments_Migrations>("payments-migrations")
    .WithReference(paymentsDb, connectionName: "DefaultConnection")
    .WithEnvironment("EventStore__NBB__ConnectionString", paymentsDb);

builder.AddProject<Projects.NBB_Payments_Api>("payments-api")
    .WithReference(paymentsDb, connectionName: "DefaultConnection")
    .WithJetStream(jetstream)
    .WithHttpHealthCheck("/health")
    .WaitForCompletion(paymentsMigrations);

builder.AddProject<Projects.NBB_Payments_Worker>("payments-worker")
    .WithReference(paymentsDb, connectionName: "DefaultConnection")
    .WithEnvironment("EventStore__NBB__ConnectionString", paymentsDb)
    .WithJetStream(jetstream)
    .WaitForCompletion(paymentsMigrations);

// Orchestration: the process manager keeps its event store in the Invoices database
builder.AddProject<Projects.NBB_MicroServicesOrchestration>("orchestration")
    .WithEnvironment("EventStore__NBB__ConnectionString", invoicesDb)
    .WithJetStream(jetstream)
    .WaitForCompletion(invoicesMigrations);

builder.Build().Run();
