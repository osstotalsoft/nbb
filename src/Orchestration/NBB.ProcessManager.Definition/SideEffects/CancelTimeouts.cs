// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

using NBB.Core.Effects;

namespace NBB.ProcessManager.Definition.SideEffects
{
    public class CancelTimeouts : ISideEffect
    {
        public string InstanceId { get; }

        public CancelTimeouts(string instanceId)
        {
            InstanceId = instanceId;
        }
    }
}
