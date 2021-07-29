using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using NBB.Core.Effects;

namespace NBB.ProjectR
{

    class ProjectorNotificationHandler<TEvent, TProjection> : INotificationHandler<TEvent>
        where TProjection : IEquatable<TProjection>
        where TEvent : INotification
    {
        private readonly IProjector<TProjection> _projector;
        private readonly IInterpreter _effectInterpreter;

        public ProjectorNotificationHandler(IProjector<TProjection> projector, IInterpreter effectInterpreter)
        {
            _projector = projector;
            _effectInterpreter = effectInterpreter;
        }


        public async Task Handle(TEvent ev, CancellationToken cancellationToken)
        {
            var message = _projector.Subscribe(ev);
            if (message is null)
                return;

            var effect = Cmd.Project(message);
            await _effectInterpreter.Interpret(effect, cancellationToken);
        }
    }
}
