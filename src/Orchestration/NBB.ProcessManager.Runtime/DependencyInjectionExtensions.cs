using MediatR;
using Microsoft.Extensions.DependencyInjection;
using NBB.Application.MediatR.Effects;
using NBB.Core.Effects;
using NBB.Http.Effects;
using NBB.Messaging.Effects;
using NBB.ProcessManager.Definition;
using NBB.ProcessManager.Definition.SideEffects;
using NBB.ProcessManager.Runtime.Persistence;
using NBB.ProcessManager.Runtime.SideEffectHandlers;
using NBB.ProcessManager.Runtime.Timeouts;
using System;
using System.Reflection;
using Unit = NBB.Core.Effects.Unit;

namespace NBB.ProcessManager.Runtime
{
    public static class DependencyInjectionExtensions
    {
        public static void AddProcessManager(this IServiceCollection services, params Assembly[] assemblies)
        {
            services.AddProcessManagerDefinition(assemblies);
            services.AddSingleton<ProcessExecutionCoordinator>();
            services.AddSingleton<IInstanceDataRepository, InstanceDataRepository>();
            services.AddEffects();
            services.AddTimeoutEffects();
            services.AddMessagingEffects();
            services.AddHttpEffects();
            services.AddMediatorEffects();
            services.AddScoped<INotificationHandler<TimeoutOccured>, TimeoutOccuredHandler>();
            services.AddNotificationHandlers(typeof(ProcessManagerNotificationHandler<,,>));
        }

        private static void AddTimeoutEffects(this IServiceCollection services)
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