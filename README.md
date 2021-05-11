# NBB
.Net Building Blocks

[![Build status](https://dev.azure.com/totalsoft//NBB/_apis/build/status/NBB-CI-GitHub)](https://dev.azure.com/totalsoft/NBB/_build/latest?definitionId=35)


## Our philosophy
Build cutting-edge, cloud-ready, scalable, maintanable and fun LOB services with .Net Building Blocks

Combining domain-driven design tactical patterns with clean architecture by decoupling the bussiness model and use-cases of the application with the rest of the technology and infrastructure, you get a technology-independent, hand-crafted, stable, encapsulated business model that will evolve over time, regardles the UI, Database, Messaging or other infrastructure or technology.

Applying concepts from EDA, CQRS or ES we decouple furthermore the business domain from the read-side so that the domain would not change when the UI views needs to change.

Applying concepts from the Microservices architectural style, you get a new beggining with every new bounded context (module).

## Architectural considerations
> The goal of software architecture is to minimize the human resources required to build and mantain the required system.
>
> -- <cite>Robert C. Martin</cite>

With NBB you can power a great diversity of architectures from a Monolythical one to a Multi-Container Microserviced based one.
It is important to mention that NBB does not impose any kind of architecture.

This repo contains a sample Microservices application decomposed around three bounded contexts: Contracts, Invoices and Payments. 
They are autonomous and the integration is based on events delivered with NATS.
The sample application contains scripts for building CI / CD pipelines for docker-compose or kubernetes.
## The blocks
- [`NBB.Core`](./Core#readme) - provides core abstractions and functionality that other packages rely upon