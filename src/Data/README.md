NBB.Data
===============

In Martin Fowler’s PoEAA, the Repository pattern is described as:
> Mediates between the domain and data mapping layers using a collection-like interface for accessing domain objects.


This collection of packages focus on formalizing data access abstrations and offer some unambitious implementations.
This packages are designed with respect to clean architecture and evented domain centric applications, although they add value even if your model is rather crud.

Data abstractions
----------------
The package *NBB.Data.Abstractions* contains some very lightweight abstractions over data repositories:
* CRUD repository
* Event sourced repository
* Read-only repository
* Unit of work repository

For more details see [`NBB.Data.Abstractions`](.//NBB.Data.Abstractions#readme)

Entity Framework
----------------
The package *NBB.Data.EntityFramework* provides some implementations for abstractions formalized above using Entity Framework:
* `EfCrudRepository`
* `EfReadOnlyRepository`
* `EfUow` - unit of work implementation
* `EfQuery` - implementation of `IQueryable<TEntity>` and `IAsyncEnumerable<TEntity>`

For more details see [`NBB.Data.EntityFramework`](./NBB.Data.EntityFramework#readme)

Entity Framework multi-tenancy
-----------------
The package *NBB.Data.EntityFramework.MultiTenancy* helps implementing a multi-tenant data access solution using Entity Framework

For more details see [`NBB.Data.EntityFramework.MultiTenancy`](./NBB.Data.EntityFramework.MultiTenancy#readme)

Event sourcing
-------------
This package aims to help you deal with data access when working with event sourced domain models.

It offers an `EventSourcedRepository` that:
* reads/persists events from/into an `IEventStore`
* manages snapshots using an `ISnapshotStore`
* dispatches events using `MediatR`

For more details see [`NBB.Data.EventSourcing`](./NBB.Data.EventSourcing#readme)
