using Microsoft.Extensions.DependencyInjection;
using NBB.ProcessManager.Runtime.EffectRunners;
using NBB.ProcessManager.Runtime.Persistence;
using NBB.ProcessManager.Runtime.Timeouts;
using System;
using NBB.ProcessManager.Definition;

namespace NBB.ProcessManager.Runtime
{
    public static class DependencyInjectionExtensions
    {
        public static void AddProcessManagerRuntime(this IServiceCollection services)
        {
            services.AddSingleton<ProcessExecutionCoordinator>();
            services.AddSingleton<IInstanceDataRepository, InstanceDataRepository>();
            services.AddTransient<IEffectVisitor, EffectVisitor>();
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