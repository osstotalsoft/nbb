// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using Mediator;

namespace NBB.ProcessManager.Runtime.Timeouts
{
    public record TimeoutOccured(string ProcessManagerInstanceId, object Message) : INotification;
}