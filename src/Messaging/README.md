Messaging
===============

When it comes to messaging systems we embrace the Microservices philosophy:
> "smart endpoints dumb pipes"


Althought NBB targets distributed systems, the messaging system is not a core building block, it is treated as an infrastructure detail.
It is your choice whether to use it or choose another messaging library. (Note. You may also choose to integrate services via Http or Event Store Streams).


Messaging abstractions
----------------

The package *NBB.Messaging.Abstractions* contains some very lightweight abstractions over messaging concepts:
* Message bus (Pubilsher and Subscriber)
* Messaging transport
* Topic resolution
* Message envelope
* Message serialization

For more details see [`NBB.Messaging.Abstractions`](.//NBB.Messaging.Abstractions)

Messaging host
----------------
The package *NBB.Messaging.Host* provides an infrastructure for event-driven stream processing microservices.

It provides the following core functionalities:
* Raising a background (hosted) service that processes incomming messages
* Configuring the messaging subscriptions (topics, options)
* Building the incomming message pipeline

For more details see [`NBB.Messaging.Host`](./NBB.Messaging.Host)

Messaging transports
-----------------
The message bus uses an abstraction over the messaging transport. The following implementations are currently supported:
* **NATS Streaming** (*NBB.Messaging.Nats* package) - https://nats.io
* **In-process** (*NBB.Messaging.InProcessMessaging*) - can be used as *test doubles* in integration tests

Other packages
-------------
* *NBB.Messaging.BackwardCompatibility* - used for backward compatibility with messaging policies from previous NBB versions (currently ensures compatiblity with NBB 4.x)
* *NBB.Messaging.Effects* - messaging side effects and handlers for the NBB effects infrastructure
* *NBB.Messaging.MultiTenancy* - support for messaging in multi-tenant environments
* *NBB.Messaging.OpenTracing* - support for *OpenTracing* in messaging publishers and subscribers
