namespace NBB.ProcessManager.Definition.Effects.Handlers
{
    public class ConditionalEffectHandler<TEvent, TData> : IEffectHandler<TEvent, TData> where TData : struct
    {
        private EventPredicate<TEvent, TData> _condition;

        public ConditionalEffectHandler(EventPredicate<TEvent, TData> condition)
        {
            _condition = condition;
        }

        public IEffect GetEffect(TEvent @event, InstanceData<TData> data)
        {
            if (_condition(@event, data))
                return 
        }
    }
}