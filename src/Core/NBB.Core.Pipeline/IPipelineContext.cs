using System;

namespace NBB.Core.Pipeline
{
    public interface IPipelineContext
    {
        IServiceProvider Services { get; }
    }
}
