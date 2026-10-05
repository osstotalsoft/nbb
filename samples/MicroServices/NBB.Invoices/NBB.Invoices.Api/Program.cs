// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Correlation.AspNet;
using NBB.Invoices.Data;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddMessageBus().AddJetStreamTransport(builder.Configuration);
builder.Services.AddInvoicesReadDataAccess();

var app = builder.Build();

app.UseCorrelation();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
