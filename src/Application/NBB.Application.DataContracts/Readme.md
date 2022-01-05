# NBB.Application.DataContracts

This package provides legacy base application data contracts, useful when referencing published language assemblies compiled against NBB4.

Starting with version 5, NBB does not formalize application data contracts any more, because there are packages like MediatR, that offer application mediator functionality and they usually formalize application requests and events.

Projects targeting NBB can still formalize application requests, and maybe segregate them into commands and queries using interface and/or base classes. It is their choice.

## NuGet install
```
dotnet add package NBB.Application.DataContracts
```

## Base data contracts
Topically published language assemblies referencing NBB4.x expose commands and events that depend on `NBB.Application.DataContracts.Command` and `NBB.Application.DataContracts.Event`. If you need to reference this kind of assemblies, you will also need to reference this package, so that `Command` and `Event` base classes will be linked to the ones provided by this package.

Example:
- ServiceA published language assembly referencing NBB4.x, contains
```csharp
using NBB.Application.DataContracts;

namespace ServiceA.PublishedLanguage.Commands
{
    public class Shutdown : Command
    {
        public Shutdown(CommandMetadata metadata = null)
            : base(metadata)
        {
        }
    }
}
```
- ServiceB references ServiceA published language so, it needs to also reference this package:
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0</TargetFramework>
  </PropertyGroup>


  <ItemGroup>
    <PackageReference Include="ServiceA.PublishedLanguage" Version="3.0.11" />
    <PackageReference Include="NBB.Application.DataContracts" Version="5.1.1" />
```

