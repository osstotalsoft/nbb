namespace NBB.ProcessManager.Definition
{
    public delegate IEffect EffectHandler<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;
    public delegate TData StateHandler<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;
    public delegate bool EventPredicate<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;
    public delegate T EventPredicate<in TEvent, TData, out T>(TEvent @event, InstanceData<TData> data) where TData : struct;

    public interface IEffect
    {
    }
}