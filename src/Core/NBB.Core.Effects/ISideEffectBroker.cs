// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public interface ISideEffectBroker
    {
        Task<TSideEffectResult> Run<TSideEffect, TSideEffectResult>(TSideEffect sideEffect, CancellationToken cancellationToken = default)
            where TSideEffect : ISideEffect<TSideEffectResult>;
    }
}
