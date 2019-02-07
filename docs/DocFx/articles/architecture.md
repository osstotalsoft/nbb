
Architecture considerations
===============

> The goal of software architecture is to minimize the human resources required to build and mantain the required system.
>
> -- <cite>Robert C. Martin</cite>

![external - internal - architecture](/images/external-internal-architecture.png)
 
 External Architecture per application
----------------

With NBB you can power a great diversity of architectures from a Monolythical one to a Multi-Container Microserviced based one.
It is important to mention that Ch-BB does not impose any kind of architecture.


NBB solution contains a sample Microservices application decomposed around three bounded contexts: Contracts, Invoices and Payments. 
They are autonomous and the integration is based on events delivered with Kafka.
The sample application contains scripts for building CI / CD pipelines for docker-compose or kubernetes.

![samples](/images/samples.png)



Internal Architecture per microservice
----------------
In each bounded context there will typically be 2 processes:
1. The API - witch is exposing the Queries and the Commands (REST, RPC, GraphQL, etc.)
2. The Worker - active service that handles topic / queue subscriptions for integration purposes and async offload (Queue-Based Load Leveling)

All microservices in the samples expose the following architecture:
![layers](/images/layers.png)

### Business Model
contains all the fine grained business details : Aggregates, Entities, Value Objects, Domain Events, Domain Services, Repository Interfaces
### Application Services
contains the use cases of the application, divided in Queries and Commands (CQS). 
All the EventHandlers and CommandHandlers, including integration ones, including read model generators, are housed here.
### Interface adapters
This is the more technical layer, you will find here repository implementations, messaging subscribers, event store clients, etc.
### Frameworks & Drivers
Here we will have external systems like: DB, EventStore, Messaging, Monitoring, Loggers, etc.


According to Uncle Bob, this architecture produces systems that are:
1. Independent of Frameworks. The architecture does not depend on the existence of some library of feature laden software. This allows you to use such frameworks as tools, rather than having to cram your system into their limited constraints.
2. Testable. The business rules can be tested without the UI, Database, Web Server, or any other external element.
3. Independent of UI. The UI can change easily, without changing the rest of the system. A Web UI could be replaced with a console UI, for example, without changing the business rules.
4. Independent of Database. You can swap out Oracle or SQL Server, for Mongo, BigTable, CouchDB, or something else. Your business rules are not bound to the database.
5. Independent of any external agency. In fact your business rules simply don’t know anything at all about the outside world.





