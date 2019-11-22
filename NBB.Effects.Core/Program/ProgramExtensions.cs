using System;

namespace NBB.Effects.Core.Program
{
    public static class ProgramExtensions
    {
        public static IProgram<(T1, T2)> Parallel<T1, T2>(IProgram<T1> p1, IProgram<T2> p2)
        {
            return ParallelProgram<T1, T2, (T1, T2)>.From(p1, p2, (t1, t2) => (t1, t2));
        }

        public static IProgram<TResult> Then<T, TResult>(this IProgram<T> program, Func<T, IProgram<TResult>> computation)
        {
            return program.Bind(computation);
        }

        public static IProgram<TResult> Then<T, TResult>(this IProgram<T> program, Func<T, TResult> selector)
        {
            return program.Map(selector);
        }
    }
}
