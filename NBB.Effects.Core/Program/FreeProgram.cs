using System;

namespace NBB.Effects.Core.Program
{
    public class FreeProgram<T> : IProgram<T>
    {
        public IEffect<IProgram<T>> Effect { get; }

        private FreeProgram(IEffect<IProgram<T>> effect)
        {
            Effect = effect;
        }

        public static FreeProgram<T> From(IEffect<T> effect)
        {
            return new FreeProgram<T>(effect.Map(value => PureProgram<T>.From(value)));
        }

        public IProgram<TResult> Map<TResult>(Func<T, TResult> selector)
        {
            return new FreeProgram<TResult>(Effect.Map(x => x.Map(selector)));
        }

        public IProgram<TResult> Bind<TResult>(Func<T, IProgram<TResult>> computation)
        {
            return new FreeProgram<TResult>(Effect.Map(x => x.Bind(computation)));
        }
    }
}
