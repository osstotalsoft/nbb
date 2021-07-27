﻿using NBB.Core.Effects;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.ProjectR
{
    public static class Cmd
    {
        public static Effect<Unit> None => Effect.Pure(Unit.Value);

        public static Effect<Unit> Project<TProjection>(object projectionId, IMessage<TProjection> message) =>
            Effect.Of<ProjectMessage.SideEffect<TProjection>, Unit>(new ProjectMessage.SideEffect<TProjection>(projectionId, message));

    }

    public static class ProjectMessage
    {
        public record SideEffect<TProjection>(object ProjectionId, IMessage<TProjection> Message) : ISideEffect, IAmHandledBy<Handler<TProjection>>;

        public class Handler<TProjection> : ISideEffectHandler<SideEffect<TProjection>, Unit>
        {
            private readonly IProjector<TProjection> _projector;
            private readonly IProjectionStore<TProjection> _projectionStore;
            private readonly IInterpreter _effectInterpreter;
            
            public Handler(IProjector<TProjection> projector, IProjectionStore<TProjection> projectionStore, IInterpreter effectInterpreter)
            {
                _projector = projector;
                _projectionStore = projectionStore;
                _effectInterpreter = effectInterpreter;
            }
            
            public async Task<Unit> Handle(SideEffect<TProjection> sideEffect, CancellationToken cancellationToken = default)
            {
                var (projection, loadedAtVersion) = await _projectionStore.Load(sideEffect.ProjectionId, cancellationToken);
                var (newProjection, effect) = _projector.Project(sideEffect.Message, projection);
                await _projectionStore.Save(sideEffect.Message, sideEffect.ProjectionId, loadedAtVersion, newProjection, cancellationToken);
                await _effectInterpreter.Interpret(effect, cancellationToken);
                return Unit.Value;
            }
        }
    }
}
