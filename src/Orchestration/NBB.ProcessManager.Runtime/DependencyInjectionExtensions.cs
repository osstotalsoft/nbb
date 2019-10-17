using Microsoft.Extensions.DependencyInjection;
using NBB.ProcessManager.Definition.Effects;
using NBB.ProcessManager.Runtime.EffectRunners;
using NBB.ProcessManager.Runtime.Persistence;
using NBB.ProcessManager.Runtime.Timeouts;
using System;

namespace NBB.ProcessManager.Runtime
{
    public static class DependencyInjectionExtensions
    {
        public static void AddProcessManagerRuntime(this IServiceCollection services)
        {
            services.AddSingleton<ProcessExecutionCoordinator>();
            services.AddSingleton<IInstanceDataRepository, InstanceDataRepository>();
            services.AddSingleton(Functions.EffectRunnerFactory());
            services.AddSingleton(typeof(IEffectRunnerMarker<PublishEvent>), EffectRunners.EffectRunners.PublishEventEffectRunner());
            services.AddSingleton(typeof(IEffectRunnerMarker<SendCommand>), EffectRunners.EffectRunners.SendCommandEffectRunner());
            services.AddSingleton(typeof(IEffectRunnerMarker<NoEffect>), EffectRunners.EffectRunners.NoOpEffect());
            services.AddSingleton(typeof(IEffectRunnerMarker<RequestTimeout>), EffectRunners.EffectRunners.RequestTimeoutEffectRunner());

            services.AddTimeoutManager();
        }

        public static void AddTimeoutManager(this IServiceCollection services)
        {
            services.AddHostedService<TimeoutsService>();
            services.AddSingleton<TimeoutsManager>();
            services.AddSingleton<ITimeoutsRepository, InMemoryTimeoutRepository>();
            services.AddSingleton<Func<DateTime>>(provider => () => DateTime.UtcNow);
        }
    }
}