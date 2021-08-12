// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.ProcessManager.Definition
{
    public struct InstanceData<TData>
    {
        public object InstanceId { get; }
        public TData Data { get; }

        public InstanceData(object instanceId, TData data)
        {
            InstanceId = instanceId;
            Data = data;
        }
    }
}