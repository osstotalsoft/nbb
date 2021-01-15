using Microsoft.Extensions.Logging;
using NBB.Core.Abstractions;
using NBB.Core.Pipeline;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.EventStore.Host.Pipeline
{
    public class ExceptionHandlingMiddleware : IPipelineMiddleware<object>
    {
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(ILogger<ExceptionHandlingMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task Invoke(object @event, CancellationToken cancellationToken, Func<Task> next)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            try
            {
                await next();

                _logger.LogInformation(
                    "Event of type {EventType} processed in {ElapsedMilliseconds} ms.",
                    @event.GetType().GetPrettyName(),
                    stopWatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Event of type {EventType} could not be process due to the following exception {Exception}.",
                    @event.GetType().GetPrettyName(), ex);
            }
            finally
            {
                stopWatch.Stop();
            }
        }
    }
}
