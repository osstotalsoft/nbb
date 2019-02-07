using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NBB.Core.Pipeline
{
    public class PipelineBuilder<T> : IPipelineBuilder<T>
    {
        private readonly IList<Func<PipelineDelegate<T>, PipelineDelegate<T>>> _components = new List<Func<PipelineDelegate<T>, PipelineDelegate<T>>>();
        private PipelineDelegate<T> _builtPipeline = null;

        public PipelineBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider
        {
            get;
        }

        public PipelineDelegate<T> Pipeline {
            get
            {
                if (_builtPipeline == null)
                {
                    _builtPipeline = Build();
                }

                return _builtPipeline;
            }
        }

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

            foreach (var component in _components.Reverse())
            {
                pipeline = component(pipeline);
            }

            return pipeline;
        }
    }
}
