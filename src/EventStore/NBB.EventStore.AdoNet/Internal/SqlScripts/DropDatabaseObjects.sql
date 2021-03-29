IF object_id('EventStoreEvents', 'U') IS NOT NULL
BEGIN
	DROP TABLE [EventStoreEvents];
END

IF EXISTS(
    SELECT * 
    FROM sys.table_types tt JOIN sys.schemas s ON tt.schema_id = s.schema_id
    WHERE tt.name='NewEventStoreEvents')
BEGIN
	DROP TYPE NewEventStoreEvents;
END

IF object_id('EventStoreSnapshots', 'U') IS NOT NULL
BEGIN
	DROP TABLE [EventStoreSnapshots];
END