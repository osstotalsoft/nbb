// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;
using NBB.Contracts.ReadModel.Data;
using NBB.Correlation.AspNet;
using System;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "Contracts API", Version = "v1" }));
builder.Services.AddHttpContextAccessor();

var transport = builder.Configuration.GetValue("Messaging:Transport", "JetStream");
if (transport.Equals("JetStream", StringComparison.InvariantCultureIgnoreCase))
{
    builder.Services
        .AddMessageBus()
        .AddJetStreamTransport(builder.Configuration)
        .UseTopicResolutionBackwardCompatibility(builder.Configuration);
}
else if (transport.Equals("Rusi", StringComparison.InvariantCultureIgnoreCase))
{
    builder.Services
        .AddMessageBus()
        .AddRusiTransport(builder.Configuration)
        .UseTopicResolutionBackwardCompatibility(builder.Configuration);
}
else
{
    throw new Exception($"Messaging:Transport={transport} not supported");
}

builder.Services
    .AddMediator(options =>
    {
        options.ServiceLifetime = ServiceLifetime.Scoped;
        options.GenerateTypesAsInternal = true; // NBB.Mono references this project and has its own generator
        options.Assemblies = [typeof(Program)]; // this host only publishes to the bus: no in-process messages or handlers
    })
    .AddMediatorIntegration(); // contract classifier for the NBB 4 topic resolution
builder.Services.AddContractsReadModelDataAccess();

var app = builder.Build();

app.UseCorrelation();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Contracts API v1"));
}

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
