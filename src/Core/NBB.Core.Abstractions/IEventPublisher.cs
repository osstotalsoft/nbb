// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NBB.Core.Abstractions
{
    /// <summary>
    /// Publishes events in-process (e.g. to mediator notification handlers) after they were persisted.
    /// Implementations receive all uncommitted changes and publish the ones they support.
    /// </summary>
    public interface IEventPublisher
    {
        Task PublishAsync(IEnumerable<object> events, CancellationToken cancellationToken = default);
    }
}
