using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBB.Core.Pipeline
{
    public class PipelineBuilder<T> : IPipelineBuilder<T>
    {
        private readonly IList<Func<PipelineDelegate<T>, PipelineDelegate<T>>> _components = new List<Func<PipelineDelegate<T>, PipelineDelegate<T>>>();
        private PipelineDelegate<T> _builtPipeline;

        public PipelineBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider
        {
            get;
        }

        public PipelineDelegate<T> Pipeline => _builtPipeline ?? (_builtPipeline = Build());

        public IPipelineBuilder<T> Use(Func<PipelineDelegate<T>, PipelineDelegate<T>> middleware)
        {
            _components.Add(middleware);
            _builtPipeline = null;

            return this;
        }

        protected virtual PipelineDelegate<T> GetPipelineTerminator()
            => (data, token) => Task.CompletedTask;

        private PipelineDelegate<T> Build()
        {
            var pipeline = GetPipelineTerminator();

            return _components.Reverse().Aggregate(pipeline, (current, component) => component(current));
        }
    }
}