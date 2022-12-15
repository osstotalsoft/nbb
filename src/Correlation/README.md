Correlation
==============

Correlation facilitates the grouping of all requests, messages, logs, and traces belonging to a business flow. They all include a randomly generated unique identifier that is referred to as a "Correlation ID".

Using correlation makes it easy to investigate problems in a business flow because the correlation ID propagates throughout all the components involved, and it can be used to search the logs. This is especially true in distributed systems where data can be scattered across different machines.


## Core package
  - [`NBB.Correlation`](./NBB.Correlation#readme) - provides core correlation functionality such as accessing the current correlation ID

## Serilog integration
  - [`NBB.Correlation.Serilog`](./NBB.Correlation.Serilog#readme) - correlation extensions for Serilog logging infrastructure
  - [`NBB.Correlation.Serilog.SqlServer`](./NBB.Correlation.Serilog.SqlServer#readme) - extensions for log persistence using the Serilog sink for SQL Server

## ASP.NET integration
  - [`NBB.Correlation.AspNet`](./NBB.Correlation.AspNet#readme) - correlation extensions for ASP.NET

## Messaging integration
The correlation ID can be attached to messages and transported across all microservices involved in a business flow.

There is a dedicated messaging header **nbb-correlationID** interpreted by:
* [`message bus publisher`](../Messaging/NBB.Messaging.Abstractions#publish) - the publisher automatically adds the current correlation ID to the envelope headers
* [`messaging host`](../Messaging/NBB.Messaging.Host#readme) - there is a built-in [`correlation middleware `](../Messaging/NBB.Messaging.Host#built-in-correlation-middleware) that reads the messaging header and sets the current correlation ID

### Open Telemetry integration
A tag named **nbb.correlation_id** is added to the messaging publisher and subscriber spans. 

For details see [`NBB.Messaging.OpenTelemetry`](./../Messaging/NBB.Messaging.OpenTelemetry#readme)
