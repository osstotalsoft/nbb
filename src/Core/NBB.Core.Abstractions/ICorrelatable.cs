using System;

namespace NBB.Core.Abstractions
{
    public interface ICorrelatable
    {
        Guid? CorrelationId { get; set; }
    }
}