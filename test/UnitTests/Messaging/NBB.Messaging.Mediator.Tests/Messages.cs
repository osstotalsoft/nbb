// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Threading;
using System.Threading.Tasks;
using Mediator;

namespace NBB.Messaging.Mediator.Tests
{
    public class Probe
    {
        public TaskCompletionSource<string> Request { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<string> Command { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource<string> Event { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public record DoSomething(string Value) : IRequest;

    public record DoSomethingElse(string Value) : ICommand;

    public record SomethingHappened(string Value) : INotification;

    public record GetSomething(string Value) : IRequest<string>;

    public record CreateSomething(string Value) : ICommand<string>;

    public record QuerySomething(string Value) : IQuery<string>;

    public record PlainMessage(string Value);

    public class DoSomethingHandler(Probe probe) : IRequestHandler<DoSomething>
    {
        public ValueTask<Unit> Handle(DoSomething request, CancellationToken cancellationToken)
        {
            probe.Request.TrySetResult(request.Value);
            return Unit.ValueTask;
        }
    }

    public class DoSomethingElseHandler(Probe probe) : ICommandHandler<DoSomethingElse>
    {
        public ValueTask<Unit> Handle(DoSomethingElse command, CancellationToken cancellationToken)
        {
            probe.Command.TrySetResult(command.Value);
            return Unit.ValueTask;
        }
    }

    public class SomethingHappenedHandler(Probe probe) : INotificationHandler<SomethingHappened>
    {
        public ValueTask Handle(SomethingHappened notification, CancellationToken cancellationToken)
        {
            probe.Event.TrySetResult(notification.Value);
            return default;
        }
    }

    public class GetSomethingHandler : IRequestHandler<GetSomething, string>
    {
        public ValueTask<string> Handle(GetSomething request, CancellationToken cancellationToken) => new(request.Value);
    }

    public class CreateSomethingHandler : ICommandHandler<CreateSomething, string>
    {
        public ValueTask<string> Handle(CreateSomething command, CancellationToken cancellationToken) => new(command.Value);
    }

    public class QuerySomethingHandler : IQueryHandler<QuerySomething, string>
    {
        public ValueTask<string> Handle(QuerySomething query, CancellationToken cancellationToken) => new(query.Value);
    }
}
