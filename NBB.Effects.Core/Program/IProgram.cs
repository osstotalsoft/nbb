using System;

namespace NBB.Effects.Core.Program
{
    public interface IProgram<out T>
    {
        IProgram<TResult> Map<TResult>(Func<T, TResult> selector);
        IProgram<TResult> Bind<TResult>(Func<T, IProgram<TResult>> computation);
    }
}
