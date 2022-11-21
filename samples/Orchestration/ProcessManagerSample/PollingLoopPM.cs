// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Application.MediatR.Effects;
using NBB.Core.Effects;
using NBB.Messaging.Effects;
using NBB.ProcessManager.Definition.Builder;
using ProcessManagerSample.Events;
using ProcessManagerSample.Queries;
using System;

namespace ProcessManagerSample;

public class PollingLoopPM : AbstractDefinition<PollingLoopPM.State>
{
    public record struct State(Guid OrderId, bool IsWorking, int LoopsNr);

    public PollingLoopPM()
    {
        Event<OrderCreated>(builder => builder.CorrelateById(@event => @event.OrderId));
        Event<TimerTicked<Guid>>(builder => builder.CorrelateById(@event => @event.Id));
        Event<LoopCompleted>(builder => builder.CorrelateById(@event => @event.Id));
        Event<LoopcycleCompleted>(builder => builder.CorrelateById(@event => @event.Id));

        StartWith<OrderCreated>()
            .SetState((orderCreated, state) => state.Data with { OrderId = orderCreated.OrderId })
            .Schedule((orderCreated, state) => new TimerTicked<Guid>(orderCreated.OrderId),
                TimeSpan.FromSeconds(5));

        When<TimerTicked<Guid>>((_, state) => !state.Data.IsWorking)
            .SetState((_, state) => state.Data with { IsWorking = true, LoopsNr = state.Data.LoopsNr + 1 })
            .Then((_, state) =>
            {
                var q1 = Mediator.Send(new GetClientQuery());
                return q1
                    .Then(client =>
                    {
                        //do some other client checks 
                        if (state.Data.LoopsNr > 5)
                            return MessageBus.Publish(new LoopCompleted(state.Data.OrderId));
                        return MessageBus.Publish(new LoopcycleCompleted(state.Data.OrderId));
                    });
            });

        When<LoopcycleCompleted>()
            .SetState((_, state) => state.Data with { IsWorking = false })
            .Schedule((_, state) => new TimerTicked<Guid>(state.Data.OrderId), TimeSpan.FromSeconds(5));

        When<LoopcycleCompleted>()
            .Complete((@event, state) => state.Data.LoopsNr > 100);

        When<LoopCompleted>()
            .Complete();

    }
}
