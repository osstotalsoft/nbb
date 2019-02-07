using NBB.Core.Abstractions;
using NBB.Core.Pipeline;

namespace NBB.EventStore.Host.Pipeline
{
    public static class EventPipelineExtensions
    {
        public static IPipelineBuilder<IEvent> UseMiddleware<TMiddleware>(this IPipelineBuilder<IEvent> pipelineBuilder) where TMiddleware : IPipelineMiddleware<IEvent>
           => pipelineBuilder.UseMiddleware<TMiddleware, IEvent>();

        public static IPipelineBuilder<IEvent> UseExceptionHandlingMiddleware(this IPipelineBuilder<IEvent> pipelineBuilder)
            => UseMiddleware<ExceptionHandlingMiddleware>(pipelineBuilder);

        public static IPipelineBuilder<IEvent> UseMediatRMiddleware(this IPipelineBuilder<IEvent> pipelineBilder)
            => UseMiddleware<MediatRMiddleware>(pipelineBilder);

        public static IPipelineBuilder<IEvent> UseDefaultResiliencyMiddleware(this IPipelineBuilder<IEvent> pipelineBuilder)
            => UseMiddleware<DefaultResiliencyMiddleware>(pipelineBuilder);
    }
}
