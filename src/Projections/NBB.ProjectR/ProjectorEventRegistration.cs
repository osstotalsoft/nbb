// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System;

namespace NBB.ProjectR
{
    /// <summary>
    /// An event a projector subscribes to. Mediator adapters register one in-process handler per registration,
    /// delegating to <see cref="ProjectorEventProcessor{TEvent,TModel,TMessage,TIdentity}"/>.
    /// </summary>
    public record ProjectorEventRegistration(Type EventType, Type ModelType, Type MessageType, Type IdentityType);
}
