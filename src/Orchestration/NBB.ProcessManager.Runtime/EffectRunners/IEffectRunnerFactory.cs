using System;
using System.Threading.Tasks;
using MediatR;
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
            return serviceProvider => effectType => (EffectRunner) serviceProvider.GetRequiredService(typeof(IEffectRunnerMarker<>).MakeGenericType(effectType));
        }
    }

    public delegate Task<TResult> EffectRunner<TResult>(IEffect<TResult> effect);

    public delegate Task EffectRunner(IEffect<Unit> effect);

    public delegate EffectRunner EffectRunnerFactory(Type effectType);

    public delegate EffectRunner<TResult> EffectRunnerFactory<TResult>(Type effectType);
}