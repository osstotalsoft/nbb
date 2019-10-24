using MediatR;

namespace NBB.ProcessManager.Definition
{
    public delegate IEffect EffectFunc<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;

    public delegate TData SetStateFunc<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;

    public delegate bool EventPredicate<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;

    public interface IEffect<out T>
    {
        T Value { get; }
    }

    public interface IEffect : IEffect<Unit>
    {
    }

    public abstract class Effect : IEffect
    {
        public Unit Value { get; }
    }
}