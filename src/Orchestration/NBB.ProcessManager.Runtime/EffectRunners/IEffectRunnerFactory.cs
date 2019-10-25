using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NBB.ProcessManager.Definition;

namespace NBB.ProcessManager.Runtime.EffectRunners
{

    public interface IEffectRunnerMarker<TEffect>
    {
    }



    public static class Functions
    {
        public static Func<IServiceProvider, EffectRunnerFactory> EffectRunnerFactory()
        {
            return serviceProvider =>
            {
                return effectType => (EffectRunner) serviceProvider.GetRequiredService(typeof(IEffectRunnerMarker<>).MakeGenericType(effectType));
            };
        }
    }

    public delegate Task<TResult> EffectRunner<TResult>(IEffect<TResult> effect);

    public delegate Task EffectRunner(IEffect effect);

    public delegate EffectRunner EffectRunnerFactory(Type effectType);
}