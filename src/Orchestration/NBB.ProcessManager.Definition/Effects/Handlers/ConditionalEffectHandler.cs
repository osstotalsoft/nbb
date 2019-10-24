namespace NBB.ProcessManager.Definition.Effects.Handlers
{
    public class ConditionalEffectHandler<TEvent, TData> : IEffectHandler<TEvent, TData> where TData : struct
    {
        private readonly EventPredicate<TEvent, TData> _condition;
        private readonly IEffectHandler<TEvent, TData> _inner;

        public ConditionalEffectHandler(EventPredicate<TEvent, TData> condition, IEffectHandler<TEvent, TData> inner)
        {
            _condition = condition;
            _inner = inner;
        }

        public IEffect GetEffect(TEvent @event, InstanceData<TData> data)
        {
            if (_condition(@event, data) && _inner != null)
                return _inner.GetEffect(@event, data);

            return NoEffect.Instance;
        }
    }
}