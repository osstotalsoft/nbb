﻿using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Messaging.Host.MessagingPipeline
{
    /// <summary>
    /// A messaging pipeline middleware that logs and swallows all exceptions.
    /// </summary>
    /// <seealso cref="NBB.Core.Pipeline.IPipelineMiddleware{NBB.Messaging.DataContracts.MessagingEnvelope}" />
    public class ExceptionHandlingMiddleware : IPipelineMiddleware<MessagingEnvelope>
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(MessagingEnvelope message, CancellationToken cancellationToken, Func<Task> next)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                await next();

                _logger.LogInformation(
                    "Message of type {EventType} processed in {ElapsedMilliseconds} ms.",
                    message.Payload.GetType().GetPrettyName(),
                    stopWatch.ElapsedMilliseconds);
            }
            catch(Exception ex)
            {
                _logger.LogError(
                    "Message of type {MessageType} could not be processed due to the following exception {Exception}.",
                    message.Payload.GetType().GetPrettyName(), ex);
            }
            finally
            {
                stopWatch.Stop();
            }
        }
    }
}
