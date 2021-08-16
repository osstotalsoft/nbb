// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Pipeline
{
    /// <summary>
    /// A middleware for a pipeline that processes data/context of type <typeparamref name="TContext"/>
    /// </summary>
    /// <typeparam name="TContext">The type of data/context processed in the pipeline</typeparam>
    public interface IPipelineMiddleware<in TContext>
        where TContext : IPipelineContext
    {
        /// <summary>
        /// Perform processing for the current <paramref name="data"/> next middleware data.
        /// </summary>
        /// <param name="data">The data/context processed in the pipeline.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="next">The next middleware.</param>
        Task Invoke(TContext data, CancellationToken cancellationToken, Func<Task> next);
    }
}