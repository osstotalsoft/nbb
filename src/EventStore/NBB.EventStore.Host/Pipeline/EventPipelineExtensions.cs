using NBB.Core.Abstractions;
using NBB.Core.Pipeline;

namespace NBB.EventStore.Host.Pipeline
{
    public static class EventPipelineExtensions
    {
        public static IPipelineBuilder<object> UseMiddleware<TMiddleware>(this IPipelineBuilder<object> pipelineBuilder) where TMiddleware : IPipelineMiddleware<object>
           => pipelineBuilder.UseMiddleware<TMiddleware, object>();

        public static IPipelineBuilder<object> UseExceptionHandlingMiddleware(this IPipelineBuilder<object> pipelineBuilder)
            => UseMiddleware<ExceptionHandlingMiddleware>(pipelineBuilder);

        public static IPipelineBuilder<object> UseMediatRMiddleware(this IPipelineBuilder<object> pipelineBilder)
            => UseMiddleware<MediatRMiddleware>(pipelineBilder);

        public static IPipelineBuilder<object> UseDefaultResiliencyMiddleware(this IPipelineBuilder<object> pipelineBuilder)
            => UseMiddleware<DefaultResiliencyMiddleware>(pipelineBuilder);
    }
}
