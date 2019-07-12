using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Pipeline
{
    /// <summary>
    /// A middleware for a pipeline that processes data/context of type <typeparamref name="T"/>
    /// </summary>
    /// <typeparam name="T">The type of data/context processed in the pipeline</typeparam>
    public interface IPipelineMiddleware<in T>
    {
        /// <summary>
        /// Perform processing for the current <paramref name="data"/> next middleware data.
        /// </summary>
        /// <param name="data">The data/context processed in the pipeline.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="next">The next middleware.</param>
        Task Invoke(T data, CancellationToken cancellationToken, Func<Task> next);
    }
}
