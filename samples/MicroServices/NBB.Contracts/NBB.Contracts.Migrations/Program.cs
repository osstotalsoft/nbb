// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NBB.Contracts.Migrations;
using System;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddScoped<Migrator>();

var host = builder.Build();
await host.StartAsync();
try
{
    using var scope = host.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<Migrator>().RunAsync();
}
catch (Exception ex)
{
    host.Services.GetRequiredService<ILogger<Program>>().LogError(ex, "Database migration failed");
    Environment.ExitCode = 1;
}
await host.StopAsync();
