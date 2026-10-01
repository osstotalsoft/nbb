// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Microsoft.Extensions.DependencyInjection.Extensions;
using NBB.Application.Mediator.Effects;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class MediatorEffectsDependencyInjectionExtensions
    {
        /// <summary>
        /// Registers the handlers of the <see cref="MediatorEff"/> side effects.
        /// The handlers are scoped: they resolve the Mediator from the scope of the effect interpreter.
        /// </summary>
        public static IServiceCollection AddMediatorEffects(this IServiceCollection services)
        {
            services.TryAddScoped(typeof(MediatorEffects.Send.RequestHandler<>));
            services.TryAddScoped(typeof(MediatorEffects.Publish.Handler));
            return services;
        }
    }
}
