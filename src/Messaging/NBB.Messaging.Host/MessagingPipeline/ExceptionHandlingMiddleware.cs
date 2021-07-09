using Microsoft.Extensions.Logging;
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
    /// <seealso cref="NBB.Core.Pipeline.IPipelineMiddleware{MessagingEnvelope}" />
    public class ExceptionHandlingMiddleware : IPipelineMiddleware<MessagingContext>
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;
        private readonly IMessageBusPublisher _messageBusPublisher;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger, IMessageBusPublisher messageBusPublisher)
        {
            _logger = logger;
            _messageBusPublisher = messageBusPublisher;
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
                _logger.LogError(
                    "Message of type {MessageType} could not be processed due to the following exception {Exception}.",
                    context.MessagingEnvelope.Payload.GetType().GetPrettyName(), ex);

                await _messageBusPublisher.PublishAsync(new { ex.Message, ex.StackTrace, ex.Source },
                    MessagingPublisherOptions.Default with { TopicName = $"{context.TopicName}_error" }, cancellationToken);
            }
            finally
            {
                stopWatch.Stop();
            }
        }
    }
}
