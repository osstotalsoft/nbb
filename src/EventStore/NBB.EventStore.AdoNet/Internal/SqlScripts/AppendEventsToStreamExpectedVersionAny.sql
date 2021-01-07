﻿declare @NewEventsCount int
select @NewEventsCount = count(*) from @NewEvents
if @NewEventsCount = 0
begin
	return;
end

declare @ActualVersion int,
	@Succes bit

set @Succes = 0;

while @Succes = 0
begin
	select @ActualVersion = count(*) from EventStoreEvents where TenantId = @TenantId and StreamId = @StreamId
	BEGIN TRY
		insert into EventStoreEvents(EventId, EventData, EventType, CorrelationId, StreamId, StreamVersion, TenantId)
		select EventId, EventData, EventType, CorrelationId, @StreamId, @ActualVersion + OrderNo, @TenantId
		from @NewEvents
		set @Succes = 1
	END TRY
	BEGIN CATCH
		set @Succes = 0
	END CATCH
end

