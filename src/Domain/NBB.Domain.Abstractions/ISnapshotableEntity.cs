// Copyright (c) TotalSoft.
// This source code is licensed under the MIT license.

namespace NBB.Domain.Abstractions
{
    public interface ISnapshotableEntity
    {
        (object snapshot, int snapshotVersion) TakeSnapshot();
        void ApplySnapshot(object snapshot, int snapshotVersion);

        int SnapshotVersion { get; }
        int? SnapshotVersionFrequency { get; }
    }
}
