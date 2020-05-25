using System;

namespace NBB.Core.Pipeline
{
    /// <summary>
    /// Used to configure a pipeline for data/context of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type of data/context </typeparam>
    public interface IPipelineBuilder<T>
    {
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        /// Adds an inline defined middleware to the pipeline.
        /// </summary>
        /// <param name="middleware">The middleware.</param>
        /// <returns>The pipeline builder for further configuring the pipeline. It is used used in the fluent configuration API.</returns>
        IPipelineBuilder<T> Use(Func<PipelineDelegate<T>, PipelineDelegate<T>> middleware);

        /// <summary>
        /// Gets constructed the pipeline.
        /// </summary>
        /// <value>
        /// The pipeline.
        /// </value>
        PipelineDelegate<T> Pipeline { get; }
    }
}
