namespace NBB.Core.Effects
{
    public static class Effect
    {
        public static IEffect<T> Of<T>(ISideEffect<T> sideEffect)
        {
            return new FreeEffect<T, T>(sideEffect, Pure);
        }

        public static IEffect<T> Pure<T>(T value)
        {
            return new PureEffect<T>(value);
        }

        public static IEffect Pure()
        {
            return new PureEffect<Unit>(Unit.Value).ToUnit();
        }

        public static IEffect<(T1, T2)> Parallel<T1, T2>(IEffect<T1> e1, IEffect<T2> e2)
        {
            return ParallelEffect<T1, T2, (T1, T2)>.From(e1, e2, (t1, t2) => (t1, t2));
        }

        public static IEffect Parallel(IEffect e1, IEffect e2)
        {
            return ParallelEffect<Unit, Unit, Unit>.From(e1, e2, (_, __) => Unit.Value).ToUnit();
        }

        
    }
}
