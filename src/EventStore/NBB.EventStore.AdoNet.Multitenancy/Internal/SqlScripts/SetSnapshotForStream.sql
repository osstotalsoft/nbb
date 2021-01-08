IF EXISTS (SELECT 1 FROM EventStoreSnapshots WITH (UPDLOCK)  WHERE TenantId = @TenantId AND StreamId = @StreamId AND StreamVersion = @StreamVersion)
BEGIN
	RAISERROR('VersionAlreadyExists', 16, 1);
END

INSERT INTO EventStoreSnapshots(SnapshotData, SnapshotType, StreamId, StreamVersion, TenantId)
	VALUES (@SnapshotData, @SnapshotType, @StreamId, @StreamVersion, @TenantId)
