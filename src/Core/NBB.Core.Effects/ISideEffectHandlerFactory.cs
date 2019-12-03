namespace NBB.Core.Effects
{
    public interface ISideEffectHandlerFactory
    {
        ISideEffectHandler<ISideEffect<TOutput>, TOutput> GetSideEffectHandlerFor<TOutput>(
            ISideEffect<TOutput> sideEffect);
    }
}
