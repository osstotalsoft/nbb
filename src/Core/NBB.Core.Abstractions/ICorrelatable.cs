// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.Core.Abstractions
{
    public interface ICorrelatable
    {
        Guid? CorrelationId { get; set; }
    }
}