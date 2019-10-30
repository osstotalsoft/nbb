using MediatR;
using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;

namespace NBB.ProcessManager.Definition.Effects
{
    public delegate IEffect<Unit> EffectFunc<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;

    public delegate TData SetStateFunc<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;

    public delegate bool EventPredicate<in TEvent, TData>(TEvent @event, InstanceData<TData> data) where TData : struct;


    public interface IEffect<TResult>
    {
        Func<IEffectRunner, Task<TResult>> Computation { get; }

        //bool IsCompleted { get; }
        //Task<T> Accept(IEffectVisitor visitor);
    }
}