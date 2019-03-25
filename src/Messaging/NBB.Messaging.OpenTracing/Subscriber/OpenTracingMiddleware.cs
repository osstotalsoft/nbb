using NBB.Core.Pipeline;
using NBB.Messaging.DataContracts;
using OpenTracing;
using OpenTracing.Propagation;
using OpenTracing.Tag;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NBB.Core.Abstractions;
using NBB.Correlation;
using NBB.Messaging.Abstractions;

namespace NBB.Messaging.OpenTracing.Subscriber
{
    public class OpenTracingMiddleware : IPipelineMiddleware<MessagingEnvelope>
    {
        private readonly ITracer _tracer;

        public OpenTracingMiddleware(ITracer tracer)
        {
            _tracer = tracer;
        }

        public async Task Invoke(MessagingEnvelope data, CancellationToken cancellationToken, Func<Task> next)
        {
            var extractedSpanContext = _tracer.Extract(BuiltinFormats.TextMap, new TextMapExtractAdapter(data.Headers));
            string operationName =  $"Subscriber {data.Payload.GetType().GetPrettyName()}"; 

            using(var scope = _tracer.BuildSpan(operationName)
                .AsChildOf(extractedSpanContext)
                .WithTag(Tags.Component, "NBB.Messaging.Subscriber")
                .WithTag(Tags.SpanKind, Tags.SpanKindServer)
                .WithTag("correlationId", CorrelationManager.GetCorrelationId()?.ToString())
                .StartActive(finishSpanOnDispose: true)) {

                try
                {
                    await next();
                }
                catch (Exception exception)
                {
                    scope.Span.Log(new Dictionary<string, object>(3)
                    {
                        { LogFields.Event, Tags.Error.Key },
                        { LogFields.ErrorKind, exception.GetType().Name },
                        { LogFields.ErrorObject, exception }
                    });
                
                    scope.Span.SetTag(Tags.Error, true);

                    throw;
                }
            }
            return;
        }
    }
}
