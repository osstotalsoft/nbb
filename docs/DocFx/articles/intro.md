
Build cutting-edge mobile-first, cloud-ready, scalable, maintanable and fun LOB services with Charisma Building Blocks
===============


Line Of Bussiness apps usually fall in one of the two categories:
* Succesfull mature solutions with an old technology stack a.k.a legacies
* Cutting-edge technology startups witch, in some years, will fall in the first category, if they are succesfull


When starting a new project, after some research the newest and the more promissing technology stack is choosen, but the problem is that technology changes extremely fast and once a project has been started, 
it is an imense effort to switch or rewrite the existent solution, and when there is a budget for that, the whole application is rewritten from ground up.

What is actually a LOB application selling? It is not about fancy UI's, databases, patterns or technologies, it's about business capabilities and business use cases. 
Ok, but you need technology to power the business capabilities, so you cannot have one without the other.

We think that this problem can be solved when:
1. combining *DDD tactical patterns with Clean Architecture* by decoupling the bussiness model and use-cases of the application with the rest of the technology and infrastructure.
This way you get a technology-independent, hand-crafted, stable, encapsulated business model that will evolve over time, regardles the UI, Database, Messaging or other infrastructure or technology.
2. applying concepts from *EDA, CQRS or ES* we decouple furthermore the business domain from the read-side so that the domain would not change when the UI views needs to change
3. applying concepts from the *Microservices* architectural style, you get a new beggining with every new bounded context (module).

