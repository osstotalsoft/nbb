﻿using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public interface IInterpreter
    {
        Task<T> Interpret<T>(IEffect<T> effect, CancellationToken cancellationToken = default);
    }
}
