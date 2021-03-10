# .NET 5 upgrade
* Update TargetFramework in *.csproj files to **net5.0** (for both netcoreapp and netstandard projects)
* Update Microsoft.Extensions.*  references to version 5.0.0
* Microsoft.CSharp package version 4.7.0
* Remove \<LangVersion\>\<LangVersion\> from csproj
* Update Dockerfile to use the following images
  * mcr.microsoft.com/dotnet/aspnet:5.0-buster-slim
  * mcr.microsoft.com/dotnet/sdk:5.0-buster-slim
  * mcr.microsoft.com/dotnet/runtime:5.0-buster-slim

## References update 
* Upgrade NNB packages version 5.x.x (latest)
* Renamed/removed packages
  * NBB.Messaging.DataContracts was renamed to NBB.Messaging.Abstractions
  * NBB.Resiliency was removed 
    * reference can be deleted, messaging policies are included in NBB.Messaging.Abstractions
    * services.UseResiliency() can be deleted
  * NBB.Application.DataContracts is deprecated
    * Transitive references to NBB.Application.DataContracts 4.x should be fixed with direct references to NBB.Application.DataContracts 5.x
    * MediatorUoWDecorator was moved to NBB.Application.MediatR
* Relocated packages - some packages were moved to NBB.Extras repo and have different versioning (v4.x)
  * NBB.Exporter.* uses NBB.Extras versioning
  * NBB.Tools.AutomapperExtensions uses NBB.Extras versioning


## Messaging update
* IMessageBusPublisher signature changed
  * TopicName and EnvelopeCustomizer moved to "MessagingPublisherOptions"
  * CancellationToken parameter moved last
* Messaging Host Middleware signature changed
  * The "Invoke" method handles a **MessagingContext** instead of a MessagingEnvelope
  * The "MessagingEnvelope" is a field in the MessagingContext
* "services.AddNatsMessaging()" will be replaced by "services.AddMessageBus().AddNatsTransport(Configuration)"
  * to support compatibility with NBB4 topics add "UseTopicResolutionBackwardCompatibility()"

## Process Manager update
* Remove "AddProcessManagerDefinition(...)", "AddProcessManagerRuntime(...)", "AddNotificationHandlers", TimeoutOccuredHandler registratin
* Replace all with "AddProcessManager(typeof(SomePM).Assembly)"
* Remove classes for ProcessManagerNotificationHandler, TimeoutOccuredHanlder


## Application update
* Interfaces like ICommand, IEvent no longer exist
* Base classes like Command, Event no longer exist
* For generic command handling code we can use IRequest instead of Command/ICommand
* For generic event handling code we can use INotification or object instead of Event/IEvent

## Third party upgrade
* NewtonsoftJsonPackageVersion 12.0.3
* MediatR 9.0.0
* Scrutor 3.3.0
* Moq 4.15.2
* EntityFramework 5.0.0
* XUnit 2.4.1
* XUnitRunerVS: 2.4.3
* Serilog: 2.10.0
* Serilog.Extensions.Logging: 3.0.1
* Serilog.Sinks.MsSqlServer: 5.6.0
* OpenTracing: 0.12.1
* OpenTracingContrib: 0.7.1
* Jaeger: 0.4.3
* Benchmark.Net: 0.12.1
* FluentAssertions: 5.10.3
* SerilogAspNetCorePackageVersion: 3.4.0 - 4.0.0
* Microsoft.AspNetCore.SignalR.Protocols.NewtonsoftJson: 5.0.0
* Microsoft.VisualStudio.Azure.Containers.Tools.Targets: 1.10.9
* FluentAssertionsPackageVersion: 5.10.3
* MicrosoftNetTestSdkPackageVersion:  16.9.1
* MoqPackageVersion: 4.16.1
* XunitPackageVersion: 2.4.1
* XunitRunnerVisualStudioPackageVersion: 2.4.3
* AutoMapper: 10.1.1
* AutoMapper.Extensions.Microsoft.DependencyInjection: 8.1.0
* FluentValidation: 9.5.2
* System.ComponentModel.Annotations: 5.0.0
* EFCore.BulkExtensions: 3.3.1
* System.Data.SqlClient 4.8.2
* Microsoft.Data.SqlClient 2.0.1
* Hellang.Middleware.ProblemDetails 5.1.1
* Swashbuckle.AspNetCore 6.1.0

## EF upgrade
* https://docs.microsoft.com/en-us/ef/core/what-is-new/ef-core-3.x/breaking-changes
* DBQuery should be replaced with DBSet (use .HasNoKey if required)
* IQueryTypeConfiguration\<TQuery\> should be replaced by IEntityTypeConfiguration\<TEntity\>
* ModelBuilder.Query\<\>() should be replaced with ModelBuilder.Entity\<\>().HasNoKey()
* DBSet.FromSql should be repladed with FromSqlRaw
  * Make sure the entity is configured with HasNoKey()
  * Make sure no linq is used on the query such as FirstOrDefaultAsync(). Replace with ToListAsync().FirstOrDefault()
* ValueGenarator.NextAsync returns ValueTask instead of task
* ExecuteSqlCommand should be replaced with ExecuteSqlRaw (similarly for async version)
* IProperty.Relational().TableName -> IProperty.GetTableName()
* Microsoft.Data.SqlClient is used instead of System.Data.SqlClient
* Concurrent DB operations throw exceptions (cannot have multiple DB tasks in parallel for the same DBContext)
* Remove the call to .AddEntityFrameworkSqlServer() it is not necessary unless using a custom service provider

## Jaeger Update
* add package reference for "Jaeger.Senders.Thrift"
* "using Jaeger.Senders" -> "using Jaeger.Senders.Thrift"

## Automapper upgrade
* https://docs.automapper.org/en/stable/8.0-Upgrade-Guide.html
* Replace linq ProjectTo() with mapper.ProjectTo
* Replace "ResolveUsing" with "MapFrom"
* Replace "UseValue(val)" with "MapFrom(_ => val)"

## FluentValidation upgrade
* ValidationContext should be replaced with ValidationContext\<T\>
* CascadeMode.StopOnFirstFailure should be replaced with CascadeMode.Stop
* Constructor custom message should be replaced with override string GetDefaultMessageTemplate()

## MediatR upgrade
* IRequestHanler / Handle method returns Task\<Unit\> now
  * use Unit.Value or Unit.Task as return value
* Make sure that the package "MediatR.Extensions.Microsoft.DependencyInjection" has the same version as "MediatR"

## Hellang upgrade
* IncludeExceptionDetails signature includes exception now
* ExceptionProblemDetails should be replaced with StatusCodePoblemDetails
* MapStatus code receives HTTPContext now.

## OTHER
* [MyProject].PublishedLanguage
  * remove Newtonsoft.Json package reference
* Replace NBBProcessManagerRuntime build variable with NBBPackagesVersion
* IConfiguration does not contain GetValue\<\>
  * add Microsoft.Extensions.Configuration.Binder package reference
* AddScoppedContravariant should support multiple type arguments like IRequest\<T, Unit\>
* Replace IHostingEnvironment with IWebHostEnvironment 
* Replace packages like "Microsoft.Aspnet*.Abstractions" with \<FrameworkReference Include="Microsoft.AspNetCore.App" /\>
