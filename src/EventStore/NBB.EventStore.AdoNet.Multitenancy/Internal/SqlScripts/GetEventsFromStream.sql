﻿SELECT EventId, EventType, EventData, CorrelationId
	FROM EventStoreEvents
	WHERE TenantId = @TenantId and StreamId = @StreamId 
		AND (@MinStreamVersion IS NULL OR StreamVersion >= @MinStreamVersion)
		AND (@MaxStreamVersion IS NULL OR StreamVersion <= @MaxStreamVersion)
	ORDER BY StreamVersion