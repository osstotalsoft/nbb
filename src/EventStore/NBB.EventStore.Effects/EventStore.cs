using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NBB.Core.Effects;
using NBB.EventStore.Abstractions;

namespace NBB.EventStore.Effects
{
    public static class AppendEventsToStream
    {
        public record SideEffect(string Stream, IEnumerable<object> Events, int? ExpectedVersion) : ISideEffect;
        internal class Handler : ISideEffectHandler<SideEffect, Unit>
        {
            private readonly IEventStore _eventStore;

            public Handler(IEventStore eventStore)
            {
                _eventStore = eventStore;
            }

            public async Task<Unit> Handle(SideEffect sideEffect, CancellationToken cancellationToken = default)
            {
                var (stream, events, expectedVersion) = sideEffect;
                await _eventStore.AppendEventsToStreamAsync(stream, events, expectedVersion, cancellationToken);
                return Unit.Value;
            }
        }
    }

    public static class GetEventsFromStream
    {
        public record SideEffect(string Stream, int? StartFromVersion) : ISideEffect<List<object>>;
        internal class Handler : ISideEffectHandler<SideEffect, List<object>>
        {
            private readonly IEventStore _eventStore;

            public Handler(IEventStore eventStore)
            {
                _eventStore = eventStore;
            }

            public Task<List<object>> Handle(SideEffect sideEffect, CancellationToken cancellationToken = default)
            {
                var (stream, startFromVersion) = sideEffect;
                return _eventStore.GetEventsFromStreamAsync(stream, startFromVersion, cancellationToken);
            }
        }
    }

    public static class EventStore
    {
        public static Effect<Unit> AppendEventsToStream(string stream, IEnumerable<object> events, int? expectedVersion = null)
            => Effect.Of<AppendEventsToStream.SideEffect, Unit>(new AppendEventsToStream.SideEffect(stream, events, expectedVersion));
        public static Effect<List<object>> GetEventsFromStream(string stream, int? startFromVersion = null)
            => Effect.Of<GetEventsFromStream.SideEffect, List<object>>(new GetEventsFromStream.SideEffect(stream, startFromVersion));
    }

    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddEventStoreEffects(this IServiceCollection services)
        {
            services.AddSingleton<ISideEffectHandler<AppendEventsToStream.SideEffect, Unit>, AppendEventsToStream.Handler>();
            services.AddSingleton<ISideEffectHandler<GetEventsFromStream.SideEffect, List<object>>, GetEventsFromStream.Handler>();
            return services;
        }
    }
}
