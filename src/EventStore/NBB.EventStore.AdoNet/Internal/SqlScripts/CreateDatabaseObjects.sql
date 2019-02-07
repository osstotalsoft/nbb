CREATE TABLE [dbo].[EventStoreEvents](
	[EventId] [uniqueidentifier] NOT NULL,
	[EventData] [nvarchar](max) NOT NULL,
	[EventType] [varchar](300) NOT NULL,
	[CorrelationId] [uniqueidentifier] NULL,
	[StreamId] [varchar](200) NOT NULL,
	[StreamVersion] int NOT NULL
 CONSTRAINT [PK_EventStoreEvents] PRIMARY KEY NONCLUSTERED([EventId] ASC)
);

CREATE CLUSTERED INDEX [IX_EventStoreEvents_StreamId] 
	ON [dbo].[EventStoreEvents](StreamId);

CREATE UNIQUE NONCLUSTERED INDEX [IX_EventStoreEvents_StreamId_StreamVersion] 
	ON [dbo].[EventStoreEvents](StreamId, StreamVersion); 


CREATE TYPE dbo.NewEventStoreEvents AS TABLE (
        OrderNo             INT IDENTITY                            NOT NULL,
        EventId             UNIQUEIDENTIFIER                        NOT NULL,
        EventData           NVARCHAR(max)                           NOT NULL,
        EventType           VARCHAR(300)                            NOT NULL,
		CorrelationId       UNIQUEIDENTIFIER                        NULL
);

CREATE TABLE [dbo].[EventStoreSnapshots](
	[SnapshotData] [nvarchar](max) NOT NULL,
	[SnapshotType] [varchar](300) NOT NULL,
	[StreamId] [varchar](200) NOT NULL,
	[StreamVersion] int NOT NULL
 CONSTRAINT [PK_EventStoreSnapshots] PRIMARY KEY ([StreamId], [StreamVersion] ASC)
);