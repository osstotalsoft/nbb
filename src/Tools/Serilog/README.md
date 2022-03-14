# Serilog tools

NBB serilog tools provide enrichers and sinks for NVBB.

The package [`NBB.Tools.Serilog.Enrichers.ServiceIdentifier`](NBB.Tools.Serilog.Enrichers.ServiceIdentifier) provides an enricher for service identifier. It is taken from nbb source / entry assembly name / process name.

The package [`NBB.Tools.Serilog.Enrichers.TenantId`](NBB.Tools.Serilog.Enrichers.TenantId) provides an enricher for tenant id from nbb tenant context.

The package [`NBB.Tools.Serilog.OpenTracingSink`](NBB.Tools.Serilog.OpenTracingSink) provides a sink for serilog and opentracing.
