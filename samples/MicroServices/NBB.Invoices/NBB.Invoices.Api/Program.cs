// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Correlation.AspNet;
using NBB.Invoices.Data;
using NBB.Messaging.OpenTelemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddMessageBus().AddJetStreamTransport(builder.Configuration);
builder.Services.AddInvoicesReadDataAccess();

// Registered after the message bus: the instrumentation decorates the bus publisher and subscriber
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddMessageBusInstrumentation()
        .AddSqlClientInstrumentation());

var app = builder.Build();

app.UseCorrelation();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
