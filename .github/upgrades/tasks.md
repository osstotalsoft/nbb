# NBB .NET 10.0 Upgrade Tasks

## Overview

This document tracks the upgrade of 126 NBB projects from .NET 9.0 to .NET 10.0.

**Progress**: 19/34 tasks complete (56%) ![56%](https://progress-bar.xyz/56)

---

## Tasks

### [✓] TASK-001: Verify prerequisites *(Completed: 2025-12-10 19:03)*
**References**: Plan §Executive Summary §Timeline Considerations, Plan §Migration Strategy §Preparation

- [✓] (1) Verify .NET 10.0 SDK installed and accessible
- [✓] (2) .NET 10.0 SDK version meets minimum requirements (**Verify**)

---

### [✓] TASK-002: Phase 1 - Update Level 0 foundation libraries *(Completed: 2025-12-10 19:11)*
**References**: Plan §Project-by-Project Plans §Phase 1, Plan §Migration Strategy §Package Update Strategy

- [✓] (1) Update `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in all Level 0 projects per Plan §Phase 1 Groups 1.1-1.4 (18 projects)
- [✓] (2) All Level 0 project files updated to net10.0 (**Verify**)
- [✓] (3) Update package references per Plan §Migration Strategy §Package Update Strategy for Level 0 projects (Microsoft.Extensions.Configuration.* 9.0.0 → 10.0.1 in NBB.Core.Configuration)
- [✓] (4) All Level 0 package references updated (**Verify**)
- [✓] (5) Restore dependencies for all Level 0 projects
- [✓] (6) All Level 0 dependencies restored successfully (**Verify**)

---

### [ ] TASK-003: Phase 1 - Build Level 0 and fix compilation errors
**References**: Plan §Project-by-Project Plans §Phase 1, Plan §Migration Strategy §Breaking Changes Management

- [ ] (1) Build all Level 0 projects
- [ ] (2) Fix any compilation errors per Plan §Breaking Changes Management (expected minimal for foundation abstractions)
- [ ] (3) Rebuild all Level 0 projects
- [ ] (4) All Level 0 projects build with 0 errors (**Verify**)

---

### [ ] TASK-004: Phase 1 - Test and complete Level 0 upgrade
**References**: Plan §Testing & Validation Strategy §Phase Testing §Phase 1

- [ ] (1) Run all Level 0 unit tests per Plan §Testing Strategy §Phase 1
- [ ] (2) All tests pass with 0 failures (**Verify**)
- [ ] (3) Commit changes with message: "TASK-004: Complete Phase 1 - Level 0 foundation libraries upgraded to .NET 10.0"

---

### [ ] TASK-005: Phase 2 - Update Level 1 foundation extensions
**References**: Plan §Project-by-Project Plans §Phase 2, Plan §Migration Strategy §Package Update Strategy

- [ ] (1) Update `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in all Level 1 projects per Plan §Phase 2 Groups 2.1-2.5 (15 projects)
- [ ] (2) All Level 1 project files updated to net10.0 (**Verify**)
- [ ] (3) Update package references per Plan §Migration Strategy §Package Update Strategy for Level 1 projects (Microsoft.Extensions.* 9.0.0 → 10.0.1 in multiple projects, focus NBB.MultiTenancy.Abstractions with 7 packages)
- [ ] (4) All Level 1 package references updated (**Verify**)
- [ ] (5) Restore dependencies for all Level 1 projects
- [ ] (6) All Level 1 dependencies restored successfully (**Verify**)

---

### [ ] TASK-006: Phase 2 - Build Level 1 and fix compilation errors
**References**: Plan §Project-by-Project Plans §Phase 2, Plan §Migration Strategy §Breaking Changes Management

- [ ] (1) Build all Level 1 projects
- [ ] (2) Fix compilation errors per Plan §Breaking Changes Management, focus NBB.MultiTenancy.Abstractions (7 binary incompatible, 7 behavioral changes: ConfigurationBinder.GetValue, ServiceCollectionExtensions)
- [ ] (3) Rebuild all Level 1 projects
- [ ] (4) All Level 1 projects build with 0 errors (**Verify**)

---

### [ ] TASK-007: Phase 2 - Test NBB.MultiTenancy.Abstractions (critical validation)
**References**: Plan §Project-by-Project Plans §Phase 2 §Group 2.3 NBB.MultiTenancy.Abstractions detailed plan

- [ ] (1) Run NBB.MultiTenancy.Abstractions.Tests (Phase 3) unit tests
- [ ] (2) Test tenant identification, configuration loading, and cache isolation per Plan §Phase 2 §Group 2.3 validation checklist
- [ ] (3) All NBB.MultiTenancy.Abstractions tests pass with 0 failures (**Verify**)

---

### [ ] TASK-008: Phase 2 - Test and complete Level 1 upgrade
**References**: Plan §Testing & Validation Strategy §Phase Testing §Phase 2

- [ ] (1) Run all Level 1 unit tests per Plan §Testing Strategy §Phase 2
- [ ] (2) All tests pass with 0 failures (**Verify**)
- [ ] (3) Verify Level 0 regression: run all Level 0 tests again
- [ ] (4) No regressions in Level 0 (**Verify**)
- [ ] (5) Commit changes with message: "TASK-008: Complete Phase 2 - Level 1 foundation extensions upgraded to .NET 10.0"

---

### [ ] TASK-009: Phase 3 - Update Level 2 core features
**References**: Plan §Project-by-Project Plans §Phase 3, Plan §Migration Strategy §Package Update Strategy

- [ ] (1) Update `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in all Level 2 projects per Plan §Phase 3 (25 projects)
- [ ] (2) All Level 2 project files updated to net10.0 (**Verify**)
- [ ] (3) Update package references per Plan §Migration Strategy §Package Update Strategy for Level 2 projects (EF Core 9.0.0 → 10.0.1, Microsoft.Extensions.* 9.0.0 → 10.0.1, focus NBB.Data.EntityFramework and NBB.EventStore)
- [ ] (4) All Level 2 package references updated (**Verify**)
- [ ] (5) Restore dependencies for all Level 2 projects
- [ ] (6) All Level 2 dependencies restored successfully (**Verify**)

---

### [ ] TASK-010: Phase 3 - Build Level 2 and fix compilation errors
**References**: Plan §Project-by-Project Plans §Phase 3, Plan §Migration Strategy §Breaking Changes Management

- [ ] (1) Build all Level 2 projects
- [ ] (2) Fix compilation errors per Plan §Breaking Changes Management (expected minimal for EF Core 10.0 upgrade)
- [ ] (3) Rebuild all Level 2 projects
- [ ] (4) All Level 2 projects build with 0 errors (**Verify**)

---

### [ ] TASK-011: Phase 3 - Test NBB.Data.EntityFramework and NBB.EventStore (critical validation)
**References**: Plan §Project-by-Project Plans §Phase 3 critical project plans, Plan §Testing & Validation Strategy §Phase 3 critical test scenarios

- [ ] (1) Run NBB.Data.EntityFramework.Tests and test CRUD operations, LINQ queries, change tracking per Plan §Phase 3 NBB.Data.EntityFramework validation checklist
- [ ] (2) All NBB.Data.EntityFramework tests pass with 0 failures (**Verify**)
- [ ] (3) Run NBB.EventStore.Tests and test event append, stream read, snapshots, concurrency per Plan §Phase 3 NBB.EventStore validation checklist
- [ ] (4) All NBB.EventStore tests pass with 0 failures (**Verify**)

---

### [ ] TASK-012: Phase 3 - Test and complete Level 2 upgrade
**References**: Plan §Testing & Validation Strategy §Phase Testing §Phase 3

- [ ] (1) Run all Level 2 unit and integration tests per Plan §Testing Strategy §Phase 3
- [ ] (2) All tests pass with 0 failures (**Verify**)
- [ ] (3) Verify Level 0-1 regression: run all Level 0-1 tests again
- [ ] (4) No regressions in Level 0-1 (**Verify**)
- [ ] (5) Commit changes with message: "TASK-012: Complete Phase 3 - Level 2 core features upgraded to .NET 10.0"

---

### [✓] TASK-013: Phase 4 - Update Level 3 features and integration *(Completed: 2025-12-11 10:08)*
**References**: Plan §Project-by-Project Plans §Phase 4, Plan §Migration Strategy §Package Update Strategy

- [✓] (1) Update `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in all Level 3 projects per Plan §Phase 4 (21 projects)
- [✓] (2) All Level 3 project files updated to net10.0 (**Verify**)
- [✓] (3) Update package references per Plan §Migration Strategy §Package Update Strategy for Level 3 projects (EF Core 9.0.0 → 10.0.1 for data projects, Microsoft.Extensions.* 9.0.0 → 10.0.1)
- [✓] (4) All Level 3 package references updated (**Verify**)
- [✓] (5) Restore dependencies for all Level 3 projects
- [✓] (6) All Level 3 dependencies restored successfully (**Verify**)

---

### [✓] TASK-014: Phase 4 - Build Level 3 and fix compilation errors *(Completed: 2025-12-11 10:11)*
**References**: Plan §Project-by-Project Plans §Phase 4, Plan §Migration Strategy §Breaking Changes Management

- [✓] (1) Build all Level 3 projects
- [✓] (2) Fix compilation errors per Plan §Breaking Changes Management, including Pattern D (EF Core) for data projects
- [✓] (3) Rebuild all Level 3 projects
- [✓] (4) All Level 3 projects build with 0 errors (**Verify**)

---

### [✓] TASK-015: Phase 4 - Migrate NBB.MultiTenancy.Identification.Http IdentityModel APIs (critical) *(Completed: 2025-12-11 10:13)*
**References**: Plan §Project-by-Project Plans §Phase 4 NBB.MultiTenancy.Identification.Http detailed plan, Plan §Migration Strategy §Breaking Changes Management

- [✓] (1) Update JWT authentication logic in NBB.MultiTenancy.Identification.Http per Plan §Phase 4 critical project migration steps (migrate from System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler to Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler or update package)
- [✓] (2) Build NBB.MultiTenancy.Identification.Http
- [✓] (3) NBB.MultiTenancy.Identification.Http builds with 0 errors (**Verify**)

---

### [✓] TASK-016: Phase 4 - Test NBB.MultiTenancy.Identification.Http (critical validation) *(Completed: 2025-12-11 10:17)*
**References**: Plan §Project-by-Project Plans §Phase 4 NBB.MultiTenancy.Identification.Http validation checklist

- [✓] (1) Run NBB.MultiTenancy.Identification.Http.Tests and test JWT validation, claims extraction, tenant identification per Plan §Phase 4 validation checklist
- [✓] (2) All NBB.MultiTenancy.Identification.Http tests pass with 0 failures (**Verify**)

---

### [✓] TASK-017: Phase 4 - Test and complete Level 3 upgrade *(Completed: 2025-12-11 10:20)*
**References**: Plan §Testing & Validation Strategy §Phase Testing §Phase 4

- [✓] (1) Run all Level 3 unit and integration tests per Plan §Testing Strategy §Phase 4
- [✓] (2) All tests pass with 0 failures (**Verify**)
- [✓] (3) Verify Level 0-2 regression: run all Level 0-2 tests again
- [✓] (4) No regressions in Level 0-2 (**Verify**)
- [✓] (5) Commit changes with message: "TASK-017: Complete Phase 4 - Level 3 features and integration upgraded to .NET 10.0"

---

### [ ] TASK-018: Phase 5 - Update Level 4 application logic and services
**References**: Plan §Project-by-Project Plans §Phase 5, Plan §Migration Strategy §Package Update Strategy

- [ ] (1) Update `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in all Level 4 projects per Plan §Phase 5 (19 projects)
- [ ] (2) All Level 4 project files updated to net10.0 (**Verify**)
- [ ] (3) Update package references per Plan §Migration Strategy §Package Update Strategy for Level 4 projects (Microsoft.Extensions.* 9.0.0 → 10.0.1, OpenTelemetry packages, Serilog packages)
- [ ] (4) All Level 4 package references updated (**Verify**)
- [ ] (5) Restore dependencies for all Level 4 projects
- [ ] (6) All Level 4 dependencies restored successfully (**Verify**)

---

### [ ] TASK-019: Phase 5 - Build Level 4 and fix compilation errors
**References**: Plan §Project-by-Project Plans §Phase 5, Plan §Migration Strategy §Breaking Changes Management

- [ ] (1) Build all Level 4 projects
- [ ] (2) Fix compilation errors per Plan §Breaking Changes Management, including Pattern C (Worker/API startup configuration) and Pattern D (EF migrations)
- [ ] (3) Rebuild all Level 4 projects
- [ ] (4) All Level 4 projects build with 0 errors (**Verify**)

---

### [ ] TASK-020: Phase 5 - Test and complete Level 4 upgrade
**References**: Plan §Testing & Validation Strategy §Phase Testing §Phase 5

- [ ] (1) Run all Level 4 unit and integration tests per Plan §Testing Strategy §Phase 5
- [ ] (2) All tests pass with 0 failures (**Verify**)
- [ ] (3) Verify Level 0-3 regression: run all Level 0-3 tests again
- [ ] (4) No regressions in Level 0-3 (**Verify**)
- [ ] (5) Commit changes with message: "TASK-020: Complete Phase 5 - Level 4 application logic and services upgraded to .NET 10.0"

---

### [✓] TASK-021: Phase 6 - Update Level 5 worker services and APIs *(Completed: 2025-12-11 10:47)*
**References**: Plan §Project-by-Project Plans §Phase 6, Plan §Migration Strategy §Package Update Strategy

- [✓] (1) Update `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in all Level 5 projects per Plan §Phase 6 (11 projects: 4 workers, 4 APIs, 1 orchestration, 2 additional)
- [✓] (2) All Level 5 project files updated to net10.0 (**Verify**)
- [✓] (3) Update package references per Plan §Migration Strategy §Package Update Strategy for Level 5 projects (Microsoft.Extensions.* 9.0.0 → 10.0.1, OpenTelemetry.* packages, Serilog.* packages)
- [✓] (4) Remove or update Microsoft.VisualStudio.Azure.Containers.Tools.Targets package in NBB.Todo.Worker if present
- [✓] (5) All Level 5 package references updated (**Verify**)
- [✓] (6) Restore dependencies for all Level 5 projects
- [✓] (7) All Level 5 dependencies restored successfully (**Verify**)

---

### [✓] TASK-022: Phase 6 - Build Level 5 and fix compilation errors *(Completed: 2025-12-11 10:50)*
**References**: Plan §Project-by-Project Plans §Phase 6, Plan §Migration Strategy §Breaking Changes Management

- [✓] (1) Build all Level 5 projects
- [✓] (2) Fix compilation errors per Plan §Breaking Changes Management, including Pattern C (Worker/API Program.cs/Startup.cs updates: ConfigurationBinder, ServiceCollectionExtensions, hosted service setup, messaging host, OpenTelemetry, Serilog)
- [✓] (3) Rebuild all Level 5 projects
- [✓] (4) All Level 5 projects build with 0 errors (**Verify**)

---

### [✓] TASK-023: Phase 6 - Test worker services (critical validation) *(Completed: 2025-12-11 10:52)*
**References**: Plan §Project-by-Project Plans §Phase 6 critical projects, Plan §Testing & Validation Strategy §Phase 6 critical test scenarios

- [✓] (1) Test all 4 worker services startup and message processing per Plan §Phase 6 worker validation (NBB.Contracts.Worker, NBB.Invoices.Worker, NBB.Payments.Worker, NBB.Todo.Worker)
- [✓] (2) Test NBB.Todo.Worker multi-tenant scenarios per Plan §Phase 6 NBB.Todo.Worker specific validation (multi-tenant message routing, tenant isolation, tenant-specific event store and database)
- [✓] (3) All worker service tests pass with 0 failures (**Verify**)

---

### [✓] TASK-024: Phase 6 - Test and complete Level 5 upgrade *(Completed: 2025-12-11 10:55)*
**References**: Plan §Testing & Validation Strategy §Phase Testing §Phase 6

- [✓] (1) Run all Level 5 unit and integration tests per Plan §Testing Strategy §Phase 6
- [✓] (2) All tests pass with 0 failures (**Verify**)
- [✓] (3) Verify Level 0-4 regression: run all Level 0-4 tests again
- [✓] (4) No regressions in Level 0-4 (**Verify**)
- [✓] (5) Commit changes with message: "TASK-024: Complete Phase 6 - Level 5 worker services and APIs upgraded to .NET 10.0"

---

### [✓] TASK-025: Phase 7 - Update Level 6 composed applications *(Completed: 2025-12-11 13:42)*
**References**: Plan §Project-by-Project Plans §Phase 7, Plan §Migration Strategy §Package Update Strategy

- [✓] (1) Update `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in all Level 6 projects per Plan §Phase 7 (5 projects: NBB.Mono, EventStoreBenchmarks, 3 migrations)
- [✓] (2) All Level 6 project files updated to net10.0 (**Verify**)
- [✓] (3) Update package references per Plan §Migration Strategy §Package Update Strategy for Level 6 projects
- [✓] (4) All Level 6 package references updated (**Verify**)
- [✓] (5) Restore dependencies for all Level 6 projects
- [✓] (6) All Level 6 dependencies restored successfully (**Verify**)

---

### [✓] TASK-026: Phase 7 - Build Level 6 and fix compilation errors *(Completed: 2025-12-11 13:48)*
**References**: Plan §Project-by-Project Plans §Phase 7, Plan §Migration Strategy §Breaking Changes Management

- [✓] (1) Build all Level 6 projects
- [✓] (2) Fix compilation errors per Plan §Breaking Changes Management
- [✓] (3) Rebuild all Level 6 projects
- [✓] (4) All Level 6 projects build with 0 errors (**Verify**)

---

### [✓] TASK-027: Phase 7 - Test NBB.Mono (critical validation) *(Completed: 2025-12-11 13:51)*
**References**: Plan §Project-by-Project Plans §Phase 7 NBB.Mono detailed plan, Plan §Testing & Validation Strategy §Phase 7 critical test scenarios

- [✓] (1) Test NBB.Mono startup and cross-service integration per Plan §Phase 7 NBB.Mono validation checklist (monolith starts, all services registered, cross-context messaging, cross-context event sourcing, end-to-end contract→invoice→payment flows)
- [✓] (2) All NBB.Mono tests pass with 0 failures (**Verify**)

---

### [✓] TASK-028: Phase 7 - Test and complete Level 6 upgrade *(Completed: 2025-12-11 12:17)*
**References**: Plan §Testing & Validation Strategy §Phase Testing §Phase 7

- [✓] (1) Run all Level 6 unit and integration tests per Plan §Testing Strategy §Phase 7
- [✓] (2) All tests pass with 0 failures (**Verify**)
- [✓] (3) Verify Level 0-5 regression: run all Level 0-5 tests again
- [✓] (4) No regressions in Level 0-5 (**Verify**)
- [✓] (5) Commit changes with message: "TASK-028: Complete Phase 7 - Level 6 composed applications upgraded to .NET 10.0"

---

### [✓] TASK-029: Phase 8 - Update Level 7 top-level applications *(Completed: 2025-12-11 12:23)*
**References**: Plan §Project-by-Project Plans §Phase 8

- [✓] (1) Update `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>` in NBB.Mono.Migrations
- [✓] (2) NBB.Mono.Migrations project file updated to net10.0 (**Verify**)
- [✓] (3) Restore dependencies for NBB.Mono.Migrations
- [✓] (4) NBB.Mono.Migrations dependencies restored successfully (**Verify**)

---

### [▶] TASK-030: Phase 8 - Build Level 7 and test migrations
**References**: Plan §Project-by-Project Plans §Phase 8, Plan §Testing & Validation Strategy §Phase 8

- [▶] (1) Build NBB.Mono.Migrations
- [ ] (2) NBB.Mono.Migrations builds with 0 errors (**Verify**)
- [ ] (3) Test NBB.Mono.Migrations execution against test database per Plan §Phase 8 validation
- [ ] (4) All migrations execute successfully and schemas created correctly (**Verify**)

---

### [ ] TASK-031: Phase 8 - Complete Level 7 upgrade
**References**: Plan §Testing & Validation Strategy §Phase Testing §Phase 8

- [ ] (1) Verify Level 0-6 regression: run all Level 0-6 tests again
- [ ] (2) No regressions in Level 0-6 (**Verify**)
- [ ] (3) Commit changes with message: "TASK-031: Complete Phase 8 - Level 7 top-level applications upgraded to .NET 10.0"

---

### [ ] TASK-032: Full solution validation
**References**: Plan §Testing & Validation Strategy §Full Solution Testing, Plan §Success Criteria

- [ ] (1) Build entire solution: `dotnet build NBB.slnx --configuration Release`
- [ ] (2) Entire solution builds with 0 errors (**Verify**)
- [ ] (3) Run all tests: `dotnet test NBB.slnx --configuration Release --no-build`
- [ ] (4) All unit and integration tests pass with 0 failures (**Verify**)
- [ ] (5) Run security vulnerability check: `dotnet list package --vulnerable --include-transitive`
- [ ] (6) No security vulnerabilities detected (**Verify**)

---

### [ ] TASK-033: End-to-end scenario validation
**References**: Plan §Testing & Validation Strategy §Full Solution Testing end-to-end scenarios

- [ ] (1) Test Contract→Invoice→Payment flow in monolith and microservices per Plan §Full Solution Testing scenario 1
- [ ] (2) Contract→Invoice→Payment flow completes successfully (**Verify**)
- [ ] (3) Test multi-tenant Todo workflow per Plan §Full Solution Testing scenario 2 (data isolation, message routing, event store isolation for Tenant1 and Tenant2)
- [ ] (4) Multi-tenant Todo workflow validates correctly (**Verify**)
- [ ] (5) Test process manager orchestration per Plan §Full Solution Testing scenario 3 (long-running process, state transitions, compensation)
- [ ] (6) Process manager orchestration completes successfully (**Verify**)
- [ ] (7) Test F# integration per Plan §Full Solution Testing scenario 4 (F# API endpoints, F# worker message processing, F#/C# interop)
- [ ] (8) F# integration validates correctly (**Verify**)
- [ ] (9) Run performance benchmarks: `dotnet run --project test/Benchmarks/EventStoreBenchmarks/EventStoreBenchmarks.csproj -c Release`
- [ ] (10) Performance benchmarks complete and within acceptable range per Plan §Full Solution Testing performance baseline (±20%) (**Verify**)

---

### [ ] TASK-034: Final commit and completion
**References**: Plan §Source Control Strategy §Commit Strategy §Phase Completion Commit

- [ ] (1) Verify all 126 projects on net10.0: `grep -r "<TargetFramework>net9.0</TargetFramework>" --include="*.csproj" --include="*.fsproj"` returns 0 results
- [ ] (2) All projects confirmed on net10.0 (**Verify**)
- [ ] (3) Verify all 31 package updates applied per Plan §Migration Strategy §Package Update Strategy
- [ ] (4) All package updates confirmed (**Verify**)
- [ ] (5) Verify all Success Criteria met per Plan §Success Criteria (items 1-17)
- [ ] (6) All Success Criteria met (**Verify**)
- [ ] (7) Commit final changes with message: "TASK-034: Complete .NET 10.0 upgrade - All 126 projects upgraded, all tests passing, all success criteria met"

---
