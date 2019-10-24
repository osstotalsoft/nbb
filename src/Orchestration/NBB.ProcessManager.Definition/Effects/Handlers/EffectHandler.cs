using System;
using System.Collections.Generic;
using System.Text;

namespace NBB.ProcessManager.Definition.Effects.Handlers
{
    public interface IEffectHandler<in TEvent, TData> where TData : struct
    {
        IEffect GetEffect(TEvent @event, InstanceData<TData> data);
    }

    public class EffectHandler<TEvent, TData> : IEffectHandler<TEvent, TData> where TData : struct
    {
        public IEffect GetEffect(TEvent @event, InstanceData<TData> data)
        {
            throw new NotImplementedException();
        }
    }
}