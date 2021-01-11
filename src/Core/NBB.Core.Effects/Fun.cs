using System;

namespace NBB.Core.Effects
{
    public static class Fun
    {
        public static T Identity<T>(T x) => x;

        public static Func<Effect<T1>, Effect<T2>> Bind<T1, T2>(Func<T1, Effect<T2>> kFunc) => x => x.Bind(kFunc);

        public static Func<T1, T3> Compose<T1, T2, T3>(Func<T2, T3> g, Func<T1, T2> f) => x => g(f(x));
    }
}
