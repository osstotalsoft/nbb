// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Application.MediatR.Effects;
using NBB.Core.Effects;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMediatorEffects(this IServiceCollection services)
        {
            services.AddSingleton(typeof(MediatorEffects.Send.Handler<>));
            services.AddSingleton<ISideEffectHandler<MediatorEffects.Publish.SideEffect, Unit>, MediatorEffects.Publish.Handler>();
            return services;
        }
    }
}
