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

                await _messageBusPublisher.PublishAsync(new
                {
                    ex.Message,
                    ex.StackTrace,
                    ex.Source,
                    Data = context.MessagingEnvelope,
                    OriginalTopic = context.TopicName,
                    OriginalSystem = context.MessagingEnvelope.Headers.TryGetValue(MessagingHeaders.Source, out var source) ? source : string.Empty,
                    CorrelationId = context.MessagingEnvelope.GetCorrelationId(),
                    MessageType = context.MessagingEnvelope.GetMessageTypeId(),
                    PublishTime = context.MessagingEnvelope.Headers.TryGetValue(MessagingHeaders.PublishTime, out var value)
                                                                         ? DateTime.TryParse(value, out var publishTime)
                                                                        ? publishTime
                                                                        : default
                                                                    : default
                },
                    MessagingPublisherOptions.Default with { TopicName = "_error" }, cancellationToken);
            }
            finally
            {
                stopWatch.Stop();
            }
        }
    }
}
