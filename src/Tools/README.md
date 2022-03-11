# Multi-tenancy

NBB provides building blocks for multi-tenant applications.

The package [`NBB.MultiTenancy.Abstractions`](NBB.MultiTenancy.Abstractions#readme) provides basic abstractions for working with tenants like the tenant data structure, context, repository or configuration options.

The package [`NBB.MultiTenancy.Identification`](NBB.MultiTenancy.Identification#readme) provides abstractions for tenant identification from the current context like the incoming HTTP request or the received messaging envelope.

## Multi-tenant web applications

The package [`NBB.MultiTenancy.Identification.Http`](NBB.MultiTenancy.Identification.Http#readme) provides tenant identification strategies from HTTP requests.

The package [`NBB.MultiTenancy.AspNet`](NBB.MultiTenancy.AspNet#readme) provides a messaging host middleware that identifies the tenant, loads the tenant information and sets it on the current tenancy context.

## Multi-tenant messaging applications

The NBB messaging infrastructure uses by default the "nbb-tenantID" messaging header to transport the tenant ID between publishers and subscribers. This behavior can be customized if needed.

The package [`NBB.MultiTenancy.Identification.Messaging`](NBB.MultiTenancy.Identification.Messaging#readme) provides tenant identification strategies.

The package [`NBB.Messaging.MultiTenancy`](../Messaging/NBB.Messaging.MultiTenancy#readme) provides functionality for message header injection and topic resolution in multi-tenant messaging environments. It also provides a messaging host middleware that identifies the tenant, loads the tenant information and sets it on the current tenancy context.

## Multi-tenant data access

The package [`NBB.Data.EntityFramework.MultiTenancy`](../Data/NBB.Data.EntityFramework.MultiTenancy#readme) helps implementing a multi-tenant data access solution using Entity Framework.

## Multi-tenant data access

The package [`NBB.MultiTenancy.Serilog`](../Data/NBB.MultiTenancy.Serilog#readme) provides a Serilog enricher that puts the tenantId into the LogContext.

## Samples

* [`Todo Tasks`](../../samples/MultiTenancy#readme) is a sample multi-tenant service containing split across two containers one Web API and one worker. It uses Entyty Framework for data access.
