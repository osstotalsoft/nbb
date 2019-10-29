using System.Threading.Tasks;
using MediatR;

namespace NBB.ProcessManager.Definition
{
    public delegate IEffect<Unit> EffectFunc<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;

    public delegate TData SetStateFunc<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;

    public delegate bool EventPredicate<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;


    public interface IBaseEffect
    {
    }

    public interface IEffect<T> : IBaseEffect
    {
        //bool IsCompleted { get; }

        Task<T> Accept(IEffectVisitor visitor);
    }

    public interface IEffect : IEffect<Unit>
    {
    }
}