IF object_id('EventStoreEvents', 'U') IS NULL
BEGIN
	CREATE TABLE [EventStoreEvents](
		[EventId] [uniqueidentifier] NOT NULL,
		[EventData] [nvarchar](max) NOT NULL,
		[EventType] [varchar](300) NOT NULL,
		[CorrelationId] [uniqueidentifier] NULL,
		[StreamId] [varchar](200) NOT NULL,
		[StreamVersion] int NOT NULL,
		[TenantId] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_EventStoreEvents] PRIMARY KEY NONCLUSTERED([EventId] ASC)
	);
END

IF NOT EXISTS(
    SELECT * 
    FROM sys.indexes
    WHERE name='IX_EventStoreEvents_TenantId_StreamId' AND object_id = OBJECT_ID('EventStoreEvents', 'U'))
BEGIN
	CREATE CLUSTERED INDEX [IX_EventStoreEvents_TenantId_StreamId] 
		ON [EventStoreEvents](TenantId, StreamId);
END

IF NOT EXISTS(
    SELECT * 
    FROM sys.indexes
    WHERE name='IX_EventStoreEvents_StreamId_StreamVersion' AND object_id = OBJECT_ID('EventStoreEvents', 'U'))
BEGIN
	CREATE UNIQUE NONCLUSTERED INDEX [IX_EventStoreEvents_StreamId_StreamVersion] 
		ON [EventStoreEvents](TenantId, StreamId, StreamVersion); 
END


IF NOT EXISTS(
    SELECT * 
    FROM sys.table_types tt JOIN sys.schemas s ON tt.schema_id = s.schema_id
    WHERE tt.name='NewEventStoreEvents')
BEGIN
	CREATE TYPE NewEventStoreEvents AS TABLE (
			OrderNo             INT IDENTITY                            NOT NULL,
			EventId             UNIQUEIDENTIFIER                        NOT NULL,
			EventData           NVARCHAR(max)                           NOT NULL,
			EventType           VARCHAR(300)                            NOT NULL,
			CorrelationId       UNIQUEIDENTIFIER                        NULL
	);
END

IF object_id('EventStoreSnapshots', 'U') IS NULL
BEGIN
	CREATE TABLE [EventStoreSnapshots](
		[SnapshotData] [nvarchar](max) NOT NULL,
		[SnapshotType] [varchar](300) NOT NULL,
		[StreamId] [varchar](200) NOT NULL,
		[StreamVersion] int NOT NULL,
		[TenantId] [uniqueidentifier] NOT NULL,
	 CONSTRAINT [PK_EventStoreSnapshots] PRIMARY KEY ([TenantId], [StreamId], [StreamVersion] ASC)
	);
END
