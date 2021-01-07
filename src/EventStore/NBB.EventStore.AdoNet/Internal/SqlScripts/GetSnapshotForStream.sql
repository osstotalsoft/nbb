SELECT TOP 1 SnapshotType, SnapshotData, StreamVersion
	FROM EventStoreSnapshots
	WHERE TenantId = @TenantId AND StreamId = @StreamId AND (@MaxStreamVersion IS NULL OR StreamVersion <= @MaxStreamVersion)
	ORDER BY StreamVersion DESC
