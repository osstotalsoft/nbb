declare @NewEventsCount int
select @NewEventsCount = count(*) from @NewEvents
if @NewEventsCount = 0
begin
	return;
end

declare @ActualVersion int
select @ActualVersion = count(*) from EventStoreEvents where TenantId = @TenantId and StreamId = @StreamId

if @ActualVersion <> @ExpectedVersion
BEGIN
	RAISERROR('WrongExpectedVersion', 16, 1);
	RETURN;
END



BEGIN TRY
	insert into EventStoreEvents(EventId, EventData, EventType, CorrelationId, StreamId, StreamVersion, TenantId)
	select EventId, EventData, EventType, CorrelationId, @StreamId, @ExpectedVersion + OrderNo, @TenantId
	from @NewEvents
END TRY
BEGIN CATCH
	RAISERROR('WrongExpectedVersion', 16, 1);
END CATCH

