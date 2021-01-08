using Microsoft.Extensions.DependencyInjection;
using NBB.ProcessManager.Runtime.Persistence;
using NBB.ProcessManager.Runtime.Timeouts;
using System;
using NBB.Application.Effects;
using NBB.Core.Effects;
using NBB.Http.Effects;
using NBB.Messaging.Effects;
using NBB.ProcessManager.Definition.SideEffects;
using NBB.ProcessManager.Runtime.SideEffectHandlers;

namespace NBB.ProcessManager.Runtime
{
    public static class DependencyInjectionExtensions
    {
        public static void AddProcessManagerRuntime(this IServiceCollection services)
        {
            services.AddSingleton<ProcessExecutionCoordinator>();
            services.AddSingleton<IInstanceDataRepository, InstanceDataRepository>();
            services.AddEffects();
            services.AddTimeoutEffects();
            services.AddMessagingEffects();
            services.AddHttpEffects();
            services.AddMediatorEffects();
        }

        public static void AddTimeoutEffects(this IServiceCollection services)
        {
            services.AddHostedService<TimeoutsService>();
            services.AddSingleton<TimeoutsManager>();
            services.AddSingleton<ITimeoutsRepository, InMemoryTimeoutRepository>();
            services.AddSingleton<Func<DateTime>>(provider => () => DateTime.UtcNow);
            services.AddSingleton<ISideEffectHandler<CancelTimeouts, Unit>, CancelTimeoutsHandler>();
            services.AddSingleton(typeof(IRequestTimeoutHandler<>), typeof(RequestTimeoutHandler<>));
        }
    }
}