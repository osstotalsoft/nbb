DELETE FROM EventStoreEvents
WHERE TenantId = @TenantId AND StreamId = @StreamId