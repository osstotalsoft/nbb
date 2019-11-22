using System;

namespace NBB.Effects.Core.Program
{
    public class PureProgram<T> : IProgram<T>
    {
        public T Value { get; }

        private PureProgram(T value)
        {
            Value = value;
        }

        public static PureProgram<T> From(T value)
        {
            return new PureProgram<T>(value);
        }

        public IProgram<TResult> Map<TResult>(Func<T, TResult> selector)
        {
            return new PureProgram<TResult>(selector(Value));
        }

        public IProgram<TResult> Bind<TResult>(Func<T, IProgram<TResult>> computation)
        {
            return computation(Value);
        }
    }
}
