﻿// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using NBB.Messaging.OpenTelemetry.Subscriber;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host;

public static class MessagingPipelineExtensions
{
    /// <summary>
    /// Adds to the pipeline a middleware that creates an OpenTelemetry span.
    /// </summary>
    /// <param name="pipelineBuilder">The pipeline builder.</param>
    /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
    public static IPipelineBuilder<MessagingContext> UseOpenTelemetryMiddleware(
        this IPipelineBuilder<MessagingContext> pipelineBuilder)
        => UseMiddleware<OpenTelemetryMiddleware>(pipelineBuilder);


    private static IPipelineBuilder<MessagingContext> UseMiddleware<TMiddleware>(
        this IPipelineBuilder<MessagingContext> pipelineBuilder)
        where TMiddleware : IPipelineMiddleware<MessagingContext>
        => pipelineBuilder.UseMiddleware<TMiddleware, MessagingContext>();
}