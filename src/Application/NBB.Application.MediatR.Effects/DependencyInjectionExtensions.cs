// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NBB.Application.MediatR.Effects;
using NBB.Core.Effects;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddMediatREffects(this IServiceCollection services)
        {
            services.TryAddSingleton(typeof(MediatorEffects.Send.QueryHandler<>));
            services.TryAddSingleton(typeof(MediatorEffects.Send.CommandHandler));
            services.TryAddSingleton<ISideEffectHandler<MediatorEffects.Publish.SideEffect, Unit>, MediatorEffects.Publish.Handler>();
            return services;
        }

        [Obsolete("Use AddMediatREffects() instead. Will be removed in NBB 11.")]
        public static IServiceCollection AddMediatorEffects(this IServiceCollection services)
            => services.AddMediatREffects();
    }
}
