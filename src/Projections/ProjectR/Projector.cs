using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Effects;

namespace ProjectR
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
            return InternalProject(ev as dynamic);
        }

    }
}
