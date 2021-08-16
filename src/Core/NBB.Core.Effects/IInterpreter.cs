// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public interface IInterpreter
    {
        Task<T> Interpret<T>(Effect<T> effect, CancellationToken cancellationToken = default);
    }
}
