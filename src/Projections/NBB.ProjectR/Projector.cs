using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Effects;

namespace NBB.ProjectR
{
    class Projector : IProjector
    {
        private readonly IServiceProvider _serviceProvider;

        public Projector(IServiceProvider serviceProvider)
        {
            this._serviceProvider = serviceProvider;
        }

        private Effect<Unit> InternalProject<TEvent>(TEvent ev)
        {
            var eventProjectors = _serviceProvider.GetServices<IEventProjector<TEvent>>();
            return Effect.Sequence(eventProjectors.Select(p => p.Project(ev)));
        }

        public Effect<Unit> Project(object ev)
        {
            var mi = this.GetType().GetMethod("InternalProject", BindingFlags.NonPublic | BindingFlags.Instance);
            var gmi = mi.MakeGenericMethod(ev.GetType());
            return gmi.Invoke(this, new[]{ev}) as Effect<Unit>;
        }

    }
}
