// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using NBB.Messaging.Abstractions;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace NBB.Messaging.Host
{
    /// <summary>
    /// A messaging pipeline middleware that logs and swallows all exceptions.
    /// </summary>
    /// <seealso cref="NBB.Core.Pipeline.IPipelineMiddleware{MessagingEnvelope}" />
    public class ExceptionHandlingMiddleware : IPipelineMiddleware<MessagingContext>
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IDeadLetterQueue _deadLetterQueue;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, IDeadLetterQueue deadLetterQueue)
        {
            _logger = logger;
            _deadLetterQueue = deadLetterQueue;
        }

        public async Task Invoke(MessagingContext context, CancellationToken cancellationToken, Func<Task> next)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                await next();

                _logger.LogInformation(
                    "Message of type {EventType} processed in {ElapsedMilliseconds} ms.",
                    context.MessagingEnvelope.Payload.GetType().GetPrettyName(),
                    stopWatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "An unhandled exception has occurred while processing message of type {MessageType}.",
                    context.MessagingEnvelope.Payload.GetType().GetPrettyName());

                //Activity.Current?.SetException(ex);
                //Activity.Current?.SetStatus(Status.Error);

                //_deadLetterQueue.Push(context.MessagingEnvelope, context.TopicName, ex);

                throw;
            }
            finally
            {
                stopWatch.Stop();
            }
        }
    }
}
