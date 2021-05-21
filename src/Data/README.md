NBB.Data
===============
This collection of packages focus on formalizing data access abstrations and offer some unambitious implementations.
This packages are designed with respect to clean architecture and evented domain centric applications, although they add value even if your model is rather crud.

Data abstractions
----------------
The package *NBB.Data.Abstractions* contains some very lightweight abstractions over data repositories:
* CRUD repository
* Event sourced repository
* Read-only repository
* Unit of work repository
* Message serialization

For more details see [`NBB.Data.Abstractions`](.//NBB.Data.Abstractions#readme)

Entity Framework
----------------
The package *NBB.Data.EntityFramework* provides some implementations formalized above using Entity Framework:
* `EfCrudRepository`
* `EfReadOnlyRepository`
* `EfUow` - unit of work implementation
* `EfQuery` - implementation of `IQueryable<TEntity>` and `IAsyncEnumerable<TEntity>`

It provides the following core functionalities:
* Raising a background (hosted) service that processes incomming messages
* Configuring the messaging subscriptions (topics, options)
* Building the incomming message pipeline

For more details see [`NBB.Data.EntityFramework`](./NBB.Data.EntityFramework#readme)

Entity Framework multi-tenancy
-----------------
The message bus uses an abstraction over the messaging transport. The following implementations are currently supported:
* **NATS Streaming** (*NBB.Messaging.Nats* package) - https://nats.io
* **In-process** (*NBB.Messaging.InProcessMessaging*) - can be used as *test doubles* in integration tests

Event sourcing
-------------
* *NBB.Messaging.BackwardCompatibility* - used for backward compatibility with messaging policies from previous NBB versions (currently ensures compatiblity with NBB 4.x)
* *NBB.Messaging.DataContracts* - helps us formalize and instrument messaging data contracts
* *NBB.Messaging.Effects* - messaging side effects and handlers for the NBB effects infrastructure
* *NBB.Messaging.MultiTenancy* - support for messaging in multi-tenant environments
* *NBB.Messaging.OpenTracing* - support for *OpenTracing* in messaging publishers and subscribers
