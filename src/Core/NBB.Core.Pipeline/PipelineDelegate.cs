using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Pipeline
{
    /// <summary>
    /// The delegate that invokes the pipeline
    /// </summary>
    /// <typeparam name="T">The type of data/context processed in the pipeline.</typeparam>
    /// <param name="data">The data/context used as input in the pipeline.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    public delegate Task PipelineDelegate<in T>(T data, CancellationToken cancellationToken);
}
