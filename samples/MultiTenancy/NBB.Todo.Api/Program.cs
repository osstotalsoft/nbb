// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NBB.Correlation.AspNet;
using NBB.Messaging.OpenTelemetry;
using NBB.MultiTenancy.Abstractions.Repositories;
using NBB.MultiTenancy.AspNet;
using NBB.Todo.Api;
using NBB.Todos.Data;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing
        .AddMessageBusInstrumentation()
        .AddSqlClientInstrumentation());

builder.Services.AddControllers();
builder.Services.AddMessageBus().AddJetStreamTransport(builder.Configuration);
builder.Services.AddTodoDataAccess();

builder.Services.AddHttpContextAccessor();
builder.Services.AddSwaggerGen(options =>
{
    if (builder.Configuration.IsMultiTenant())
    {
        options.OperationFilter<SwaggerTenantHeaderFilter>();
    }
});

builder.Services.AddMultitenancy(builder.Configuration)
    .AddDefaultHttpTenantIdentification()
    .AddMultiTenantMessaging()
    .AddTenantRepository<ConfigurationTenantRepository>();

builder.Services.AddProblemDetails(options => ProblemDetailsConfiguration.Configure(options));

var app = builder.Build();

app.UseRouting();

app.UseAuthorization();
app.UseSwagger();
app.UseSwaggerUI();

app.UseCorrelation();
app.UseWhen(
    ctx => ctx.Request.Path.StartsWithSegments(new PathString("/api")),
    appBuilder => appBuilder.UseTenantMiddleware());

app.UseProblemDetails();

app.MapControllers();
app.MapDefaultEndpoints();

app.Run();
