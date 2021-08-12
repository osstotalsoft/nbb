// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.Core.Pipeline
{
    public interface IPipelineContext
    {
        IServiceProvider Services { get; }
    }
}
