using System;

public interface ICorrelatable {
    Guid? CorrelationId { get; set; }
}