# NBB.Correlation.AspNet

This package correlation support for ASP.NET applications

## NuGet install
```
dotnet add package NBB.Correlation.AspNet
```

### Correlation middleware

This ASP.NET middlware is responsible with creating a correlation scope for the current request.

```csharp
public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
{
    app.UseCorrelation();
    ...
           
}
```

The middeware searches the current request for a correlation ID in the following locations:
* **x-correlation-id** http header 
* **correlationId** query string parameter 

If none is found, a new correlation ID is generated.
 