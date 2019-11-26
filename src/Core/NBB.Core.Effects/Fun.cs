using System;

namespace NBB.Core.Effects
{
    public static class Fun
    {
        public static T Identity<T>(T x) => x;
        public static Func<T1, T3> Compose<T1, T2, T3>(Func<T2, T3> f, Func<T1, T2> g) => x => f(g(x));
    }
}
