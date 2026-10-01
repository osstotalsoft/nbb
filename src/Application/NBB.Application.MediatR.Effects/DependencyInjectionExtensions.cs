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
        /// <summary>
        /// Registers the handlers of the <see cref="MediatorEff"/> side effects.
        /// The handlers are scoped: they resolve the mediator, and through it the request and notification handlers, from the scope of the effect interpreter.
        /// </summary>
        public static IServiceCollection AddMediatREffects(this IServiceCollection services)
        {
            services.TryAddScoped(typeof(MediatorEffects.Send.QueryHandler<>));
            services.TryAddScoped(typeof(MediatorEffects.Send.CommandHandler));
            services.TryAddScoped<ISideEffectHandler<MediatorEffects.Publish.SideEffect, Unit>, MediatorEffects.Publish.Handler>();
            return services;
        }

        [Obsolete("Use AddMediatREffects() instead. Will be removed in NBB 11.")]
        public static IServiceCollection AddMediatorEffects(this IServiceCollection services)
            => services.AddMediatREffects();
    }
}
