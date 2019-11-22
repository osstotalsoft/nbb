using System;

namespace NBB.Effects.Core.Program
{
    public class ParallelProgram<T1, T2, T> : IProgram<T>
    {
        public IProgram<T1> LeftProgram { get; }
        public IProgram<T2> RightProgram { get; }

        public Func<T1, T2, IProgram<T>> Next { get; }

        private ParallelProgram(IProgram<T1> leftProgram, IProgram<T2> rightProgram, Func<T1, T2, IProgram<T>> next)
        {
            LeftProgram = leftProgram;
            RightProgram = rightProgram;
            Next = next;
        }

        public static ParallelProgram<T1, T2, T> From(IProgram<T1> leftProgram, IProgram<T2> rightProgram, Func<T1, T2, T> selector)
        {
            return new ParallelProgram<T1, T2, T>(leftProgram, rightProgram, (t1, t2) => PureProgram<T>.From(selector(t1, t2)));
        }


        public IProgram<TResult> Map<TResult>(Func<T, TResult> selector)
        {
            return new ParallelProgram<T1, T2, TResult>(LeftProgram, RightProgram, (t1, t2) => Next(t1, t2).Map(selector));
        }

        public IProgram<TResult> Bind<TResult>(Func<T, IProgram<TResult>> computation)
        {
            return new ParallelProgram<T1, T2, TResult>(LeftProgram, RightProgram, (t1, t2) => Next(t1, t2).Bind(computation));
        }
    }
}
