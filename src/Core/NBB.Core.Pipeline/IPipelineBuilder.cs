using System;

namespace NBB.Core.Pipeline
{
    /// <summary>
    /// Used to configure a pipeline for data/context of type <typeparamref name="TContext"/>
    /// </summary>
    /// <typeparam name="TContext">The type of data/context </typeparam>
    public interface IPipelineBuilder<TContext> where TContext: IPipelineContext
    {
        /// <summary>
        /// Adds an inline defined middleware to the pipeline.
        /// </summary>
        /// <param name="middleware">The middleware.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        IPipelineBuilder<TContext> Use(Func<PipelineDelegate<TContext>, PipelineDelegate<TContext>> middleware);

        /// <summary>
        /// Gets constructed the pipeline.
        /// </summary>
        /// <value>
        /// The pipeline.
        /// </value>
        PipelineDelegate<TContext> Pipeline { get; }
    }
}
