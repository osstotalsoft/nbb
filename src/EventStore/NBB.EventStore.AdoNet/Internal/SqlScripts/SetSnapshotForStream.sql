IF EXISTS (SELECT 1 FROM EventStoreSnapshots WITH (UPDLOCK)  WHERE StreamId = @StreamId AND StreamVersion = @StreamVersion)
BEGIN
	RAISERROR('VersionAlreadyExists', 16, 1);
END

INSERT INTO EventStoreSnapshots(SnapshotData, SnapshotType, StreamId, StreamVersion)
	VALUES (@SnapshotData, @SnapshotType, @StreamId, @StreamVersion)
