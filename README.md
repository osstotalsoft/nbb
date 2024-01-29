NBB [![CI](https://github.com/osstotalsoft/nbb/actions/workflows/ci.yml/badge.svg?branch=master)](https://github.com/osstotalsoft/nbb/actions/workflows/ci.yml)
===============
>.Net Building Blocks

![Building blocks](assets/img/icon.png)

Our philosophy
----------------
Build cutting-edge, cloud-ready, scalable, maintainable and fun LOB services with .Net Building Blocks

Combining domain-driven design tactical patterns with clean architecture by decoupling the business model and use-cases of the application with the rest of the technology and infrastructure, you get a technology-independent, hand-crafted, stable, encapsulated business model that will evolve over time, regardless the UI, Database, Messaging or other infrastructure or technology.

Applying concepts from EDA, CQRS or ES we decouple furthermore the business domain from the read-side so that the domain would not change when the UI views needs to change.

Applying concepts from the Microservices architectural style, you get a new beginning with every new bounded context (module).

Architectural considerations
----------------
> The goal of software architecture is to minimize the human resources required to build and maintain the required system. 
>
> -- <cite>Robert C. Martin</cite>

With NBB you can power a great diversity of architectures from a Monolithic one to a Multi-Container Microservices based one.
It is important to mention that NBB does not impose any kind of architecture.

This repo contains a sample Microservices application decomposed around three bounded contexts: Contracts, Invoices and Payments. 
They are autonomous and the integration is based on events delivered with NATS.
The sample application contains scripts for building CI / CD pipelines for docker-compose or kubernetes.

The blocks
----------------
* [`NBB.Core`](./src/Core#readme) - core abstractions and functionality that other packages rely upon
* [`NBB.Application`](./src/Application#readme) - application layer specific functionality
* [`NBB.Domain`](./src/Domain#readme) - building blocks for domain modelling in DDD 
* [`NBB.Data`](./src/Data#readme) - data access abstractions and implementations
* [`NBB.Messaging`](./src/Messaging#readme) - distributed application infrastructure that enables loosely-coupled, message-based asynchronous communication
* [`NBB.EventStore`](./src/EventStore#readme) - event store functionality
* [`NBB.Correlation`](./src/Correlation#readme) - facilitates the grouping of all requests, messages, logs, and traces belonging to a business flow
* [`NBB.ProcessManager`](./src/Orchestration#readme) - a way of orchestrating your events
* [`NBB.MultiTenancy`](./src/MultiTenancy#readme) -  building blocks for multi-tenant applications
* [`NBB.ProjectR`](./src/Projections/NBB.ProjectR#readme) - functional style read-model projections inspired by [Elm](https://guide.elm-lang.org/)

The samples
----------------
Please see our [`samples`](./samples#README.md) folder to get you started with various architectures built with NBB.

The templates
-------------

[`NBB templates`](https://github.com/osstotalsoft/nbb-templates#readme)
is a collection of custom templates to be used by the dotnet cli (dotnet new command).

## License

NBB is licensed under the [MIT](LICENSE) license.
