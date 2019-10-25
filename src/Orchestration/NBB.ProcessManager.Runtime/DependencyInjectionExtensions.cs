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
            services.AddSingleton(typeof(IEffectRunnerMarker<PublishMessageEffect>), EffectRunners.EffectRunners.PublishMessageEffectRunner());
            services.AddSingleton(typeof(IEffectRunnerMarker<NoEffect>), EffectRunners.EffectRunners.NoOpEffect());
            services.AddSingleton(typeof(IEffectRunnerMarker<RequestTimeout>), EffectRunners.EffectRunners.RequestTimeoutEffectRunner());
            services.AddSingleton(typeof(IEffectRunnerMarker<CancelTimeouts>), EffectRunners.EffectRunners.CancelTimeoutsEffectRunner());
            services.AddSingleton(typeof(IEffectRunnerMarker<SequentialEffect>), EffectRunners.EffectRunners.CancelTimeoutsEffectRunner());

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