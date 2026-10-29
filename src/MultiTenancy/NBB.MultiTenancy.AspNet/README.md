# NBB.MultiTenancy.AspNet

This project provides an ASP.NET middleware that is responsible with: 
* identifying the tenant identifier from the incoming request
* loading tenant information from the repository
* building the tenancy context that holds the current tenant
* tagging the current request span with **nbb.tenant_id** and **nbb.tenant_code** (see `TenantTracingTags`)

## NuGet install
```
dotnet add package NBB.MultiTenancy.AspNet
```
# Registration
The tenancy middleware can be registered in `Startup.cs` using the following application builder extension:

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    ...
    app.UseTenantMiddleware();
    ...
}
```
