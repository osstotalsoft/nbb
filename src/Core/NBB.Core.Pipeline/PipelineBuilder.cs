// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBB.Core.Pipeline
{
    public class PipelineBuilder<TContext> : IPipelineBuilder<TContext> where TContext : IPipelineContext
    {
        private readonly IList<Func<PipelineDelegate<TContext>, PipelineDelegate<TContext>>> _components =
            new List<Func<PipelineDelegate<TContext>, PipelineDelegate<TContext>>>();

        private PipelineDelegate<TContext> _builtPipeline;

        public PipelineDelegate<TContext> Pipeline => _builtPipeline ??= Build();

        public IPipelineBuilder<TContext> Use(Func<PipelineDelegate<TContext>, PipelineDelegate<TContext>> middleware)
        {
            _components.Add(middleware);
            _builtPipeline = null;

            return this;
        }

        protected virtual PipelineDelegate<TContext> GetPipelineTerminator()
            => (_, _) => Task.CompletedTask;

        private PipelineDelegate<TContext> Build()
        {
            var pipeline = GetPipelineTerminator();

            return _components.Reverse().Aggregate(pipeline, (current, component) => component(current));
        }
    }
}