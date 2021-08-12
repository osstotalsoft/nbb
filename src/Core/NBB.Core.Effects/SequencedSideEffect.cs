// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Effects
{
    public class Sequenced
    {
        public class SideEffect<T> : ISideEffect<IEnumerable<T>>, IAmHandledBy<Handler<T>>
        {
            public IEnumerable<Effect<T>> EffectList { get; }

            public SideEffect(IEnumerable<Effect<T>> effectList)
            {
                EffectList = effectList;
            }
        }

        public class Handler<T> : ISideEffectHandler<SideEffect<T>, IEnumerable<T>>
        {
            private readonly IInterpreter _interpreter;
            
            public Handler(IInterpreter interpreter)
            {
                _interpreter = interpreter;
            }
            
            public async Task<IEnumerable<T>> Handle(SideEffect<T> sideEffect, CancellationToken cancellationToken = default)
            {
                var tasks = sideEffect.EffectList.Select(effect => _interpreter.Interpret(effect, cancellationToken)).ToList();
                await Task.WhenAll(tasks);
                return tasks.Select(t => t.Result);
            }
        }

        public static SideEffect<T> From<T>(IEnumerable<Effect<T>> effectList)
            => new(effectList);

    }

}
