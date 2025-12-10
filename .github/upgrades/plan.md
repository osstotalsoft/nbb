# .NET 10.0 Upgrade Plan

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Project-by-Project Plans](#project-by-project-plans)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

## Executive Summary

### Overview

This plan details the upgrade of the NBB solution from **.NET 9.0 to .NET 10.0**. The solution consists of **126 projects** organized in a **7-level dependency hierarchy**, including core libraries, microservices samples, and test projects.

### Discovered Metrics

| Metric | Value | Status |
|--------|-------|--------|
| **Total Projects** | 126 | All require upgrade |
| **Dependency Depth** | 7 levels | Deep, well-structured hierarchy |
| **Total Issues** | 438 | 187 mandatory, 248 potential, 3 optional |
| **Affected Files** | 175 / 553 | 31.6% of codebase |
| **Package Updates** | 31 / 85 | 36.5% require upgrade |
| **Security Vulnerabilities** | 0 | ? None found |
| **Circular Dependencies** | 0 | ? Clean dependency graph |
| **Estimated LOC to Modify** | 98+ | Approximately 0.3% of codebase |

### Complexity Classification: **Complex**

**Rationale:**
- ? 126 projects (significantly >15 threshold)
- ? 7 levels of dependencies (>4 threshold)
- ? Mixed language support (C# and F# projects)
- ? Multiple application types (microservices, monolith, test projects)
- ?? But: No high-risk projects, no security vulnerabilities, clean dependency graph
- ?? Most issues are low-impact API compatibility changes (28713 compatible vs 98 breaking)

### Critical Success Factors

- **No Security Vulnerabilities**: Excellent baseline security posture
- **Clean Dependency Graph**: Well-architected solution with clear separation of concerns
- **Low Breaking Change Impact**: Only 98 APIs require code changes out of 28,713 analyzed
- **No Deprecated Packages**: Only 2 deprecated packages (SqlStreamStore, STAN.Client) with known replacements

### Selected Migration Strategy: **Bottom-Up (Dependency-First)**

**Why Bottom-Up:**
- Clear 7-level dependency hierarchy enables clean tier-by-tier migration
- No circular dependencies to complicate ordering
- Each level can be upgraded, tested, and validated before moving up
- Foundation libraries upgraded first ensure stable base for dependent projects
- Minimize multi-targeting complexity (dependencies always on same or newer framework)

### Iteration Strategy: **Batched by Dependency Level**

Given the well-structured dependency graph, projects will be upgraded in **batches per level**:
- **Level 0** (18 projects): Foundation libraries with no dependencies - batch upgrade
- **Level 1** (15 projects): First-tier dependencies - batch upgrade
- **Level 2-7** (93 projects): Progressive tier upgrades with validation at each level

### Expected Remaining Iterations

Based on complexity classification:
1. ? Phase 1: Discovery & Classification (Complete)
2. ? Phase 2: Foundation - Strategy, Dependency Analysis, Stubs
3. ?? Phase 3: Detail Generation - 8 iterations (one per dependency level)
4. Final Iteration: Success Criteria and Source Control Strategy

**Total Expected Iterations**: ~12

### Key Technologies Requiring Attention

| Technology | Impact | Migration Path |
|------------|--------|----------------|
| **Entity Framework Core** | 28 packages | 9.0.0 ? 10.0.1 |
| **Microsoft.Extensions.*** | Multiple packages | 9.0.0 ? 10.0.1 |
| **IdentityModel APIs** | 5 issues | Migrate to Microsoft.IdentityModel.* packages |
| **SqlStreamStore** | Deprecated | Maintain on 1.2.0 or plan replacement |
| **STAN.Client** | Deprecated | Maintain on 0.3.0 or migrate to JetStream |

### Timeline Considerations

**Relative Complexity by Level:**
- **Level 0-1**: Low - Foundation libraries, minimal API surface changes
- **Level 2-3**: Low-Medium - Core features, some breaking changes expected
- **Level 4-5**: Medium - Integration projects, worker services
- **Level 6-7**: Medium - Sample applications and migration tools

This plan uses **relative complexity ratings** (Low/Medium/High) rather than time estimates, as actual duration depends on team capacity, testing requirements, and deployment cadence.

## Migration Strategy

### Approach Selection: **Bottom-Up (Dependency-First)**

#### Rationale

The NBB solution's well-structured 7-level dependency hierarchy makes it an ideal candidate for a bottom-up migration approach:

**Why Bottom-Up is Optimal:**

1. **Clean Dependency Graph**: Zero circular dependencies enable strict tier-by-tier progression
2. **Depth of Hierarchy**: 7 levels require careful ordering to avoid multi-targeting complexity
3. **Stable Foundation Pattern**: Foundation libraries (Level 0) are consumed by 100+ downstream projects
4. **Risk Mitigation**: Early validation of core libraries catches breaking changes before they propagate
5. **Learning Curve**: Lessons from upgrading foundation libraries apply to all dependent projects
6. **No Multi-Targeting Needed**: Dependencies always on same or newer framework than consumers

#### Bottom-Up Strategy Principles Applied

**From Strategy Guidance:**

? **Dependency-First Ordering**: Projects upgraded only after all their dependencies are complete
? **Tier-Based Batching**: Projects within the same level upgraded together (no cross-dependencies)
? **Cumulative Testing**: Each tier validated with all lower tiers before proceeding
? **Stable Build Requirement**: Solution remains buildable after each tier completes
? **Clear Validation Points**: Explicit success criteria for each phase before advancing

**Adaptations for This Solution:**
- **8 Phases Matching 8 Levels**: Each dependency level becomes a distinct migration phase
- **Within-Tier Parallelization**: Projects in the same level can be upgraded simultaneously
- **Application Path Separation**: Sample apps (Contracts, Invoices, Payments, Todo) can progress independently after shared foundation is ready
- **Test Project Co-Migration**: Test projects upgraded alongside their corresponding source projects within the same phase

### Migration Phases

#### Phase Structure

Each phase follows this structure:

1. **Preparation**
   - Review all projects in the phase
   - Verify all dependencies (lower tiers) are stable and complete
   - Identify phase-specific risks
   - **Duration**: Single planning task

2. **Framework & Package Updates**
   - Update `<TargetFramework>` from `net9.0` to `net10.0` in all project files
   - Update all package references requiring new versions
   - **Batching**: Single operation covering all projects in the phase
   - **Duration**: Single execution task

3. **Compilation & Error Resolution**
   - Build all projects in the phase
   - Resolve compilation errors from breaking API changes
   - Address warnings that block clean build
   - **Batching**: Per-phase, not per-project (common patterns apply across phase)
   - **Duration**: 1-2 tasks depending on error volume

4. **Testing & Validation**
   - Run unit tests for all projects in phase
   - Run integration tests between current phase and lower phases
   - Verify consumers (higher tiers still on net9.0) remain compatible
   - **Duration**: Single validation task per phase

5. **Stabilization Checkpoint**
   - Address remaining issues
   - Document lessons learned
   - Mark phase complete
   - **Duration**: Single checkpoint task

#### Phase Execution Order

**Phase 1 ? Phase 2 ? ... ? Phase 8** (strict sequential order)

- ? **Cannot skip phases**: Each phase depends on previous phase being stable
- ? **Within-phase parallelization allowed**: No intra-phase dependencies
- ?? **Between-phase validation required**: Must verify tier completion before advancing
- ?? **Higher tiers remain on net9.0**: Until their turn (no forward compatibility concerns)

### Dependency-Based Ordering Principles

#### Tier Determination (Already Applied)

From the dependency graph analysis:

**Tier 0 (Level 0)**: Projects with zero internal project references
- 18 foundation libraries
- No dependencies on other solution projects
- May depend on external NuGet packages only

**Tier N+1 (Levels 1-7)**: Projects depending only on Tiers 0 through N
- Level 1: 15 projects depending only on Level 0
- Level 2: 25 projects depending only on Levels 0-1
- ...and so on through Level 7

**Verification**: Dependency graph analysis confirmed no circular dependencies exist

#### Upgrade Flow Per Phase

For each phase (corresponding to a dependency level):

**1. Preparation** (One Task)
- Review all projects in the phase
- Check dependencies are stable (lower phases complete)
- Identify phase-specific risks
- Confirm no blocking issues

**2. Update** (Typically 1-2 Tasks)
- **Task 1**: Update project files + packages for entire phase
  - Change `<TargetFramework>net9.0</TargetFramework>` to `<TargetFramework>net10.0</TargetFramework>`
  - Update package versions per package update table (see section below)
  - Single operation for all projects in phase
  
- **Task 2** (if needed): Fix compilation errors
  - Address breaking API changes
  - Resolve namespace changes
  - Update obsolete API usage
  - Batched by phase, not per-project (common patterns)

**3. Testing** (Phase-Scoped)
- Run unit tests for all phase projects
- Run integration tests with lower phases
- Validate higher phases (still on net9.0) still work with upgraded dependencies
- Regression testing for lower phases

**4. Stabilization** (One Checkpoint)
- Address issues found during testing
- Document lessons learned
- Mark phase complete
- Confirm solution builds and all tests pass

**5. Proceed to Next Phase**
- Only after current phase fully validated
- No partial phase completions

### Package Update Strategy

#### Package Update Principles

? **Update All Recommended Packages**: All 31 packages flagged for upgrade will be updated
? **No Deferrals**: Package updates happen in the same phase as the project using them
? **Version Consistency**: Same package updated to same version across all projects
? **Security First**: No security vulnerabilities identified, but deprecated packages noted

#### Key Package Updates

**Entity Framework Core**: 9.0.0 ? 10.0.1 (5 packages)
- `Microsoft.EntityFrameworkCore`
- `Microsoft.EntityFrameworkCore.Design`
- `Microsoft.EntityFrameworkCore.InMemory`
- `Microsoft.EntityFrameworkCore.Relational`
- `Microsoft.EntityFrameworkCore.SqlServer`

**Microsoft.Extensions.***: 9.0.0 ? 10.0.1 (23 packages)
- All `Microsoft.Extensions.Configuration.*`
- All `Microsoft.Extensions.DependencyInjection.*`
- All `Microsoft.Extensions.Logging.*`
- All `Microsoft.Extensions.Hosting.*`
- All `Microsoft.Extensions.Options.*`
- All `Microsoft.Extensions.Caching.*`

**System.Net.Http**: 4.3.4 ? Remove (functionality included in framework)

**Newtonsoft.Json**: 13.0.3 ? 13.0.4

#### Deprecated Packages (Maintain Current Versions)

?? **SqlStreamStore.MsSql**: 1.2.0 (deprecated, no replacement available)
- **Action**: Keep current version, plan future replacement if needed
- **Impact**: Used by `NBB.SQLStreamStore` and benchmarks

?? **STAN.Client**: 0.3.0 (deprecated, replaced by JetStream)
- **Action**: Keep current version, or migrate to `NBB.Messaging.JetStream` which is already available
- **Impact**: Used by `NBB.Messaging.Nats`

?? **Microsoft.VisualStudio.Azure.Containers.Tools.Targets**: 1.21.0 (incompatible)
- **Action**: Remove or update to compatible version
- **Impact**: Only used by `NBB.Todo.Worker`, Docker tooling only

### Breaking Changes Management

#### Expected Breaking Changes

From assessment analysis:

**API Compatibility Summary:**
- ?? **Binary Incompatible**: 58 APIs (require code changes)
- ?? **Source Incompatible**: 20 APIs (re-compilation needed)
- ?? **Behavioral Change**: 20 APIs (testing required)
- ? **Compatible**: 28,615 APIs

**Top Breaking Change Categories:**

1. **Microsoft.Extensions.DependencyInjection.ServiceCollectionExtensions** (28 occurrences)
   - Extension method signature changes
   - **Mitigation**: Update method calls, compiler will guide

2. **Microsoft.Extensions.Configuration.ConfigurationBinder.GetValue** (12 occurrences)
   - Generic method constraints changed
   - **Mitigation**: Explicit type parameters may be needed

3. **System.TimeSpan factory methods** (15 occurrences)
   - Parameter type changes (Int64 overloads)
   - **Mitigation**: Explicit casts or use Double overloads

4. **IdentityModel APIs** (5 issues)
   - JwtSecurityTokenHandler and related types
   - **Mitigation**: Migrate to `Microsoft.IdentityModel.*` packages

5. **Microsoft.Extensions.Logging.ConsoleLoggerExtensions.AddConsole** (6 occurrences)
   - Behavioral changes
   - **Mitigation**: Review console logging configuration

#### Breaking Change Strategy

**Per-Phase Approach:**

1. **Compile First**: Let compiler identify actual breaking changes
2. **Pattern Recognition**: Identify common patterns in errors
3. **Batch Fixes**: Apply same fix pattern across multiple projects in phase
4. **Test Immediately**: Validate fixes don't introduce regressions
5. **Document Patterns**: Record solutions for application to later phases

**Common Fix Patterns Expected:**

```csharp
// Pattern 1: TimeSpan factory methods
// Before (net9.0)
TimeSpan.FromSeconds(10)  // Int32 parameter

// After (net10.0) - may need explicit cast
TimeSpan.FromSeconds((double)10)  // or TimeSpan.FromSeconds(10.0)

// Pattern 2: Configuration binding
// Before (net9.0)
var value = config.GetValue<int>("Key");

// After (net10.0) - may need explicit type parameter
var value = config.GetValue<int>("Key");  // Usually unchanged, but verify

// Pattern 3: DependencyInjection
// Most changes are binary-only, recompilation sufficient
services.AddSingleton<IMyService, MyService>();  // Usually unchanged
```

### Validation & Testing Strategy Per Phase

#### Phase Completion Criteria

Before proceeding to the next phase, verify:

? **All projects in phase build successfully** (no errors)
? **All projects in phase build cleanly** (no warnings)
? **All unit tests in phase pass** (100% pass rate)
? **Integration tests with lower phases pass** (regression check)
? **Higher phases on net9.0 still build** (forward compatibility)
? **No new security vulnerabilities introduced** (package audit)

#### Testing Levels

**Per-Project Testing:**
- Unit tests for the project pass
- No dependency conflicts
- Project builds without errors or warnings

**Phase Testing:**
- All projects in phase build together
- Integration between phase projects works
- Phase tests (if any) all pass

**Cross-Phase Testing:**
- Projects from current phase integrate correctly with lower phases (already on net10.0)
- Projects from higher phases (still on net9.0) can still consume current phase (rare, but validate if public libraries)

**Full Solution Testing** (After Phase 8):
- Complete solution builds
- All tests pass (unit + integration)
- End-to-end scenarios work
- Performance acceptable
- No security vulnerabilities

### Risk Mitigation Through Bottom-Up

**How Bottom-Up Reduces Risk:**

1. **Foundation Stability**: Core libraries upgraded first, validated thoroughly before dependent code touches them
2. **Early Breaking Change Detection**: API incompatibilities discovered in isolated libraries, not deep in application stack
3. **Incremental Validation**: Each phase provides a checkpoint; issues caught early
4. **Learning Applied Upward**: Patterns discovered in Level 0 guide fixes in Levels 1-7
5. **Rollback Clarity**: If a phase fails, only that phase needs rollback; lower phases remain stable
6. **Parallel Safety**: Within-phase parallel work has no cross-dependencies, reducing merge conflicts

### Source Control Alignment

**Branch Strategy** (detailed in Source Control Strategy section):
- Single upgrade branch: `upgrade/net10.0`
- Commits per phase completion
- PR for final review after all phases complete

**Commit Strategy**:
- Phase Preparation: Planning commit
- Phase Updates: Project file + package updates commit
- Phase Fixes: Compilation error fixes commit(s)
- Phase Validation: Test fixes commit (if needed)
- Phase Completion: Checkpoint commit with summary

This strategy ensures each phase is a reviewable, revertible unit of work.

## Project-by-Project Plans

### Organization Approach

Given the 126 projects in this solution, this section organizes projects by:
1. **Phase/Level**: Projects grouped by dependency level
2. **Criticality**: Detailed plans for high-impact projects, grouped plans for similar low-impact projects
3. **Pattern**: Common upgrade patterns identified and documented once, referenced by multiple projects

### Common Upgrade Patterns

These patterns apply across multiple projects and will be referenced throughout:

#### **Pattern A: Simple Class Library**
**Applies to**: ~60 projects with no package updates, 0-2 API issues
**Steps**:
1. Update `<TargetFramework>net9.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`
2. Rebuild project
3. Fix any compilation errors (typically none or minimal)
4. Run unit tests (if present)
5. Validate no warnings

#### **Pattern B: Test Project**
**Applies to**: ~35 test projects
**Steps**:
1. Apply Pattern A (framework update)
2. Run test suite
3. Address test failures (typically related to tested code changes, not test framework)
4. Validate 100% test pass rate

#### **Pattern C: Worker Service / API**
**Applies to**: ~10 worker/API projects
**Steps**:
1. Update framework target
2. Update Microsoft.Extensions.* packages (typically 5-10 packages)
3. Update startup configuration if needed (Program.cs, Startup.cs)
4. Address ConfigurationBinder and DependencyInjection breaking changes
5. Test application startup
6. Run integration tests
7. Validate health checks

#### **Pattern D: Data Access (Entity Framework)**
**Applies to**: ~7 projects using EF Core
**Steps**:
1. Update framework target
2. Update EF Core packages: 9.0.0 ? 10.0.1
3. Update database provider packages (e.g., Microsoft.EntityFrameworkCore.SqlServer)
4. Rebuild and address compilation errors
5. **Critical**: Test database queries (especially complex LINQ)
6. **Critical**: Verify migrations still work
7. Run data access integration tests

#### **Pattern E: F# Project**
**Applies to**: ~12 F# projects
**Steps**:
1. Update `<TargetFramework>net9.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`
2. Verify FSharp.Core version compatibility (typically auto-updated)
3. Rebuild project
4. Validate F#/C# interop if present
5. Test computation expressions and effect-based code
6. Run unit tests

---

### Phase 1 (Level 0): Foundation Libraries

**Common Characteristics**: No internal dependencies, pure libraries, Pattern A or Pattern E applies

#### Group 1.1: Core Abstractions (Pattern A)

**Projects** (5):
- `NBB.Core.Abstractions`
- `NBB.Core.Configuration`
- `NBB.Core.DependencyInjection`
- `NBB.Core.Pipeline`
- `NBB.Correlation`

**Upgrade Approach**: Batch upgrade using Pattern A
**Expected Issues**: Minimal - these are abstract contracts
**Validation**: Ensure all interface contracts remain binary compatible

**Special Notes**:
- `NBB.Core.Configuration`: Has 4 package updates (Microsoft.Extensions.Configuration.*)
  - Update: `Microsoft.Extensions.Configuration*` 9.0.0 ? 10.0.1

#### Group 1.2: Core Effects (Pattern A + E)

**Projects** (3):
- `NBB.Core.Effects` (C#, Pattern A)
- `NBB.Core.FSharp` (F#, Pattern E)
- `NBB.Core.Evented.FSharp` (F#, Pattern E)

**Upgrade Approach**: C# first, then F# projects
**Expected Issues**: Verify effect-based patterns work correctly
**Validation**: Test effect composition, monadic operations

#### Group 1.3: Published Language Contracts (Pattern A)

**Projects** (4):
- `NBB.Contracts.PublishedLanguage`
- `NBB.Invoices.PublishedLanguage`
- `NBB.Payments.PublishedLanguage`
- `NBB.Todo.PublishedLanguage`

**Upgrade Approach**: Batch upgrade using Pattern A
**Expected Issues**: None - these are simple contract classes
**Validation**: Ensure serialization compatibility (no breaking changes to message contracts)

#### Group 1.4: Supporting Libraries (Pattern A)

**Projects** (6):
- `NBB.Application.DataContracts`
- `NBB.Contracts.ReadModel`
- `NBB.Messaging.DataContracts`
- `NBB.SQLStreamStore.Migrations`
- `NBB.Tools.Serilog.Enrichers.ServiceIdentifier`
- `NBB.Tools.Serilog.OpenTelemetryTracingSink`

**Upgrade Approach**: Batch upgrade using Pattern A
**Expected Issues**: Minimal
**Validation**: Standard build and test

**Special Notes**:
- `NBB.SQLStreamStore.Migrations`: Uses deprecated `SqlStreamStore.MsSql` package - keep at 1.2.0, document in tech debt

---

### Phase 2 (Level 1): Foundation Extensions

#### Group 2.1: MediatR Integration (Pattern A)

**Projects** (2):
- `NBB.Application.MediatR`
- `NBB.Application.MediatR.Effects`

**Upgrade Approach**: Pattern A, sequential (Effects depends on base)
**Expected Issues**: None - MediatR 12.4.1 is compatible
**Validation**: Verify request/response pipelines

#### Group 2.2: Abstractions (Pattern A)

**Projects** (4):
- `NBB.Data.Abstractions`
- `NBB.Domain.Abstractions`
- `NBB.EventStore.Abstractions`
- `NBB.Messaging.Abstractions`

**Upgrade Approach**: Batch upgrade using Pattern A
**Expected Issues**: 
- `NBB.Messaging.Abstractions`: 7 API issues (ConfigurationBinder, DependencyInjection)
**Validation**: Critical - these abstractions used by 50+ projects

**Special Attention**:
- `NBB.Messaging.Abstractions`: 4 package updates:
  - `Microsoft.Extensions.Configuration.Abstractions` 9.0.0 ? 10.0.1
  - Update after fixing compilation errors

#### Group 2.3: Correlation & Multi-Tenancy (Pattern A)

**Projects** (5):
- `NBB.Correlation.AspNet`
- `NBB.Correlation.Serilog`
- `NBB.Correlation.Serilog.SqlServer`
- `NBB.MultiTenancy.Abstractions` ??

**Upgrade Approach**: Pattern A with special attention to `NBB.MultiTenancy.Abstractions`
**Expected Issues**: 
- `NBB.MultiTenancy.Abstractions`: **15 issues (7 mandatory)** - highest in Phase 2

**Detailed Plan for `NBB.MultiTenancy.Abstractions`** (Medium Complexity):

**Current State**:
- Target Framework: net9.0
- 7 Package updates required (Microsoft.Extensions.Caching.*, Configuration.Binder, Options.*, Hosting.Abstractions, DependencyInjection)
- 15 API issues: 7 binary incompatible, 7 behavioral changes
- Key issues: ConfigurationBinder.GetValue, ServiceCollectionExtensions

**Migration Steps**:
1. Update target framework to net10.0
2. Update packages:
   - `Microsoft.Extensions.Caching.Abstractions` 9.0.0 ? 10.0.1
   - `Microsoft.Extensions.Caching.Memory` 9.0.0 ? 10.0.1
   - `Microsoft.Extensions.Configuration.Binder` 9.0.0 ? 10.0.1
   - `Microsoft.Extensions.Options` 9.0.0 ? 10.0.1
   - `Microsoft.Extensions.Options.ConfigurationExtensions` 9.0.0 ? 10.0.1
   - `Microsoft.Extensions.Hosting.Abstractions` 9.0.0 ? 10.0.1
   - `Microsoft.Extensions.DependencyInjection.Abstractions` 9.0.0 ? 10.0.1
3. Rebuild and address compilation errors:
   - **Pattern**: ConfigurationBinder.GetValue calls may need explicit type parameters
   - **Pattern**: ServiceCollectionExtensions binary compatibility (likely just recompile)
4. Review tenant caching behavior (behavioral changes in caching abstractions)
5. Test multi-tenant scenarios:
   - Tenant identification
   - Tenant configuration loading
   - Tenant cache isolation
6. Run unit tests: `NBB.MultiTenancy.Abstractions.Tests` (Phase 3)

**Validation Checklist**:
- [ ] Builds without errors
- [ ] No new warnings
- [ ] Tenant configuration loads correctly
- [ ] Tenant caching works as expected
- [ ] No performance degradation in tenant resolution

#### Group 2.4: F# Integration (Pattern E)

**Projects** (2):
- `NBB.Core.Effects.FSharp`
- `NBB.Application.Mediator.FSharp`

**Upgrade Approach**: Pattern E
**Expected Issues**: Minimal - F# projects typically smooth upgrade
**Validation**: Test F# computation expressions, effect composition

#### Group 2.5: Foundation Tests (Pattern B)

**Projects** (3):
- `NBB.Core.Configuration.Tests`
- `NBB.Core.Pipeline.Tests`
- `NBB.Tools.Serilog.Enrichers.ServiceIdentifier.Tests`

**Upgrade Approach**: Pattern B
**Expected Issues**: Test failures related to tested code changes only
**Validation**: 100% test pass rate

---

### Phase 3 (Level 2): Core Features

**Key Focus**: Entity Framework Core 10.0 validation

#### **Critical Project: `NBB.Data.EntityFramework`** (High Impact)

**Why Critical**: Foundation for all data access in sample applications

**Current State**:
- Target Framework: net9.0
- Package updates: `Microsoft.EntityFrameworkCore` 9.0.0 ? 10.0.1
- Used by: 7 data access projects (Contracts, Invoices, Payments, Todo, Tests)

**Migration Steps**:
1. Update target framework to net10.0
2. Update packages:
   - `Microsoft.EntityFrameworkCore` 9.0.0 ? 10.0.1
3. Rebuild and address compilation errors (likely minimal)
4. **Critical Testing**:
   - Test basic CRUD operations
   - Test complex LINQ queries (watch for query translation changes)
   - Test change tracking behavior
   - Test connection resilience
   - Run all unit tests in `NBB.Data.EntityFramework.Tests` (Phase 3)
5. Document any EF 10.0 behavioral changes discovered
6. Create fix patterns for application to dependent projects (Phase 4+)

**Validation Checklist**:
- [ ] All CRUD operations work
- [ ] LINQ queries translate correctly
- [ ] Change tracking behaves as expected
- [ ] Concurrency handling works
- [ ] All unit tests pass
- [ ] No performance degradation (>20% slower)

**Breaking Change Monitoring**:
- Query translation changes (GROUP BY, DISTINCT, subqueries)
- Connection string handling
- Migration generation

#### **Critical Project: `NBB.EventStore`** (High Impact)

**Why Critical**: Central to event sourcing pattern used across solution

**Current State**:
- Target Framework: net9.0
- 6 Package updates (Microsoft.Extensions.*)
- 7 API issues
- Used by: Worker services, EventStore implementations, applications

**Migration Steps**:
1. Update target framework to net10.0
2. Update packages (Microsoft.Extensions.Configuration.*, Options.*)
3. Rebuild and address API compatibility issues
4. Test event store operations:
   - Append events
   - Read event streams
   - Snapshot handling
   - Concurrency conflict resolution
5. Run `NBB.EventStore.Tests` (Phase 3)
6. Validate with `NBB.EventStore.InMemory` and `NBB.EventStore.AdoNet` (Phase 3)

**Validation Checklist**:
- [ ] Event append works
- [ ] Stream read works
- [ ] Snapshots work
- [ ] Concurrency conflicts detected
- [ ] All tests pass

#### Group 3.1: Data Access (Pattern D)

**Projects** (3):
- `NBB.Data.EventSourcing` (depends on NBB.EventStore)
- `NBB.Domain` (depends on NBB.Domain.Abstractions)

**Upgrade Approach**: After `NBB.EventStore` is validated, apply Pattern D
**Expected Issues**: Coordination between EventStore and EventSourcing
**Validation**: Test event-sourced aggregate persistence and retrieval

#### Group 3.2: Messaging Implementations (Pattern A)

**Projects** (7):
- `NBB.Messaging.InProcessMessaging`
- `NBB.Messaging.Nats` ??
- `NBB.Messaging.JetStream`
- `NBB.Messaging.Rusi`
- `NBB.Messaging.Noop`
- `NBB.Messaging.OpenTelemetry`
- `NBB.Messaging.Host` (depends on NBB.Core.Effects)

**Upgrade Approach**: Pattern A, test each messaging provider

**Special Attention**:
- `NBB.Messaging.Nats`: Uses deprecated `STAN.Client` - keep at 0.3.0
- `NBB.Messaging.Rusi`: 15 API issues (1 mandatory) - test gRPC communication
- `NBB.Messaging.Host`: 12 issues (3 mandatory) - critical messaging infrastructure

**Validation**: Test message send/receive with each provider

#### Group 3.3: Effects & Supporting (Pattern A)

**Projects** (5):
- `NBB.Http.Effects`
- `NBB.Messaging.Effects`
- `NBB.Messaging.BackwardCompatibility`
- `NBB.Core.Effects.FSharp` (F#)
- `NBB.Application.Mediator.FSharp` (F#)

**Upgrade Approach**: C# first, then F#
**Expected Issues**: HTTP client factory behavioral changes
**Validation**: Test effect composition patterns

#### Group 3.4: Multi-Tenancy Implementation (Pattern A)

**Projects** (2):
- `NBB.MultiTenancy.Identification`
- `NBB.Messaging.MultiTenancy`

**Upgrade Approach**: Pattern A
**Expected Issues**: Messaging + multi-tenancy integration
**Validation**: Test tenant message routing

#### Group 3.5: Phase 2 Tests (Pattern B)

**Projects** (8 test projects from Phase 2 libraries)

**Upgrade Approach**: Pattern B after corresponding source projects complete
**Expected Issues**: Test failures related to source changes
**Validation**: 100% pass rate

---

### Phase 4 (Level 3): Features & Integration

#### **Critical Project: `NBB.MultiTenancy.Identification.Http`** (High Complexity)

**Why Critical**: JWT authentication + multi-tenancy, IdentityModel migration required

**Current State**:
- Target Framework: net9.0
- **8 API issues (6 mandatory)** - highest breaking change impact in solution
- **Key Issue**: `System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler` API changes

**Migration Steps**:
1. Update target framework to net10.0
2. **Address IdentityModel Breaking Changes**:
   - Old API: `System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler`
   - New API: Use `Microsoft.IdentityModel.JsonWebTokens.JsonWebTokenHandler` (recommended)
   - Or: Update to newer `System.IdentityModel.Tokens.Jwt` package version compatible with .NET 10
3. Update JWT validation logic:
   ```csharp
   // Before (net9.0)
   var handler = new JwtSecurityTokenHandler();
   if (handler.CanReadToken(token))
   {
       var jwt = handler.ReadToken(token) as JwtSecurityToken;
       var claims = jwt.Claims;
   }

   // After (net10.0) - Option 1: New API
   var handler = new JsonWebTokenHandler();
   var result = handler.ValidateToken(token, validationParameters);
   
   // After (net10.0) - Option 2: Updated package
   // (Check latest System.IdentityModel.Tokens.Jwt compatibility)
   ```
4. **Critical Testing**:
   - Test JWT token validation
   - Test claims extraction
   - Test tenant identification from JWT
   - Test authentication middleware pipeline
   - Run `NBB.MultiTenancy.Identification.Http.Tests` (Phase 4)

**Validation Checklist**:
- [ ] JWT validation works
- [ ] Claims correctly extracted
- [ ] Tenant ID identified from JWT
- [ ] Authentication middleware flows correctly
- [ ] All tests pass

#### Group 4.1: Domain Models (Pattern A)

**Projects** (3):
- `NBB.Contracts.Domain`
- `NBB.Invoices.Domain`
- `NBB.Payments.Domain`

**Upgrade Approach**: Batch upgrade using Pattern A
**Expected Issues**: None - simple domain models
**Validation**: Domain logic tests

#### Group 4.2: Data Access (Pattern D)

**Projects** (4):
- `NBB.Contracts.ReadModel.Data` (EF Core)
- `NBB.Contracts.WriteModel.Data` (Event Sourcing)
- `NBB.Invoices.Data` (EF Core + Event Sourcing)
- `NBB.Payments.Data` (EF Core)
- `NBB.Todo.Data` (EF Core + Multi-tenancy)

**Upgrade Approach**: Pattern D
**Expected Issues**: Apply EF Core 10.0 patterns from Phase 3
**Validation**: Database integration tests for each

**Special Attention**:
- `NBB.Todo.Data`: Multi-tenant data isolation must be validated

#### Group 4.3: EventStore Implementations (Pattern A)

**Projects** (3):
- `NBB.EventStore.AdoNet`
- `NBB.EventStore.AdoNet.MultiTenancy`
- `NBB.EventStore.InMemory`

**Upgrade Approach**: Pattern A
**Expected Issues**: SQL connection handling, multi-tenant isolation
**Validation**: Event persistence and retrieval tests

#### Group 4.4: F# Samples (Pattern E)

**Projects** (1):
- `NBB.Invoices.FSharp` (uses Core.Evented.FSharp, Core.Effects.FSharp, Mediator.FSharp, Messaging.Effects)

**Upgrade Approach**: Pattern E
**Expected Issues**: F#/C# interop, effect composition
**Validation**: F# computation expressions, message handling

#### Group 4.5: Orchestration (Pattern A)

**Projects** (2):
- `NBB.ProcessManager.Definition`
- `NBB.ProjectR`

**Upgrade Approach**: Pattern A
**Expected Issues**: HTTP effects, messaging effects
**Validation**: Process manager state transitions, projections

#### Group 4.6: Multi-Tenancy Extensions (Pattern A)

**Projects** (2):
- `NBB.MultiTenancy.AspNet`
- `NBB.MultiTenancy.Identification` (base)

**Upgrade Approach**: After `NBB.MultiTenancy.Identification.Http` complete
**Expected Issues**: ASP.NET Core middleware pipeline
**Validation**: Tenant identification middleware

#### Group 4.7: Phase 3 Tests (Pattern B)

**Projects** (~10 test projects from Phase 3 libraries)

**Upgrade Approach**: Pattern B
**Expected Issues**: Test failures related to source changes
**Validation**: 100% pass rate

---

### Phase 5 (Level 4): Application Logic & Services

#### Group 5.1: Application Logic (Pattern A)

**Projects** (4):
- `NBB.Contracts.Application`
- `NBB.Invoices.Application`
- `NBB.Payments.Application`
- (Note: Todo app doesn't have separate Application layer)

**Upgrade Approach**: Pattern A
**Expected Issues**: MediatR pipelines, business logic
**Validation**: Application service tests, command/query handling

#### Group 5.2: APIs (Pattern C)

**Projects** (3):
- `NBB.Contracts.Api`
- `NBB.Invoices.FSharp.Api` (F#)

**Upgrade Approach**: Pattern C (Pattern E for F# project)
**Expected Issues**: Startup configuration, middleware pipeline
**Validation**: API integration tests, Swagger/OpenAPI

**Special Attention**:
- `NBB.Contracts.Api`: 5 mandatory API issues (ConfigurationBinder, ServiceCollectionExtensions)
  - Test: API startup, health checks, OpenTelemetry, Swagger

#### Group 5.3: Worker Services (Pattern C)

**Projects** (2):
- `NBB.Invoices.FSharp.Worker` (F#)
- `ProcessManagerSample`

**Upgrade Approach**: Pattern C (Pattern E for F# project)
**Expected Issues**: Hosted service configuration, messaging setup
**Validation**: Worker starts correctly, message processing

**Special Attention**:
- `ProcessManagerSample`: 14 issues (3 mandatory) - complex orchestration sample
  - Test: Process manager state machine, compensation flows

#### Group 5.4: Orchestration Runtime (Pattern A)

**Projects** (1):
- `NBB.ProcessManager.Runtime`

**Upgrade Approach**: Pattern A
**Expected Issues**: Process state persistence, HTTP/messaging effects
**Validation**: State machine transitions, long-running processes

#### Group 5.5: Migrations (Pattern A)

**Projects** (2):
- `NBB.EventStore.AdoNet.Migrations`
- `NBB.Todo.Migrations`

**Upgrade Approach**: Pattern A + Pattern D (EF migrations)
**Expected Issues**: Database migration execution
**Validation**: Migrations apply cleanly to test database

**Special Attention**:
- `NBB.EventStore.AdoNet.Migrations`: Critical - event store schema
- `NBB.Todo.Migrations`: Multi-tenant database migrations

#### Group 5.6: Phase 4 Tests (Pattern B)

**Projects** (~8 test projects from Phase 4 libraries)

**Upgrade Approach**: Pattern B
**Expected Issues**: Test failures related to source changes
**Validation**: 100% pass rate

---

### Phase 6 (Level 5): Worker Services & APIs

#### **Critical Projects: Worker Services** (Pattern C - High Complexity)

**Projects** (4):
- `NBB.Contracts.Worker`
- `NBB.Invoices.Worker`
- `NBB.Payments.Worker`
- `NBB.Todo.Worker` ??

**Why Critical**: Full application stack, multi-tenancy, messaging, event store integration

**Common Migration Approach** (all 4 workers):
1. Update target framework to net10.0
2. Update packages (Microsoft.Extensions.*, OpenTelemetry.*, Serilog.*)
3. Update Program.cs/Startup.cs:
   - Configuration loading (ConfigurationBinder changes)
   - Service registration (ServiceCollectionExtensions changes)
   - Hosted service setup
4. Update messaging host configuration
5. Update OpenTelemetry configuration
6. Update Serilog configuration
7. Test worker startup
8. Test message processing end-to-end
9. Test event store integration
10. Run integration tests

**Specific Attention Per Worker**:

**`NBB.Contracts.Worker`**:
- 12 issues (5 mandatory)
- OpenTelemetry + Messaging.Rusi integration
- **Validate**: Contract events published correctly

**`NBB.Invoices.Worker`**:
- 13 issues (5 mandatory + 1 behavioral)
- Console logger behavioral change
- **Validate**: Invoice events processed, cross-context integration (contracts, payments)

**`NBB.Payments.Worker`**:
- 13 issues (5 mandatory + 1 behavioral)
- Similar to Invoices.Worker
- **Validate**: Payment events processed, cross-context integration

**`NBB.Todo.Worker`** (Highest Complexity):
- **11 issues (7 mandatory)** - highest mandatory count in Phase 6
- **Multi-tenancy validation required**
- Uses: OpenTelemetry, Serilog.Enrichers.TenantId, MultiTenancy.Identification.Http, MultiTenancy.Identification.Messaging
- **Validate**:
  - Multi-tenant message routing
  - Tenant isolation in message processing
  - Tenant-specific event store
  - Tenant-specific database access

**Special Note**:
- `NBB.Todo.Worker`: May need to remove/update `Microsoft.VisualStudio.Azure.Containers.Tools.Targets` package (incompatible)

#### Group 6.2: Additional APIs (Pattern C)

**Projects** (3):
- `NBB.Invoices.Api`
- `NBB.Payments.Api`
- `NBB.Todo.Api`

**Upgrade Approach**: Pattern C
**Expected Issues**: Similar to Contracts.Api
**Validation**: API startup, health checks, Swagger

**Special Attention**:
- `NBB.Todo.Api`: 5 mandatory issues, multi-tenant API
  - **Validate**: Tenant identification from JWT, tenant-scoped data access

#### Group 6.3: Orchestration Sample (Pattern C)

**Projects** (1):
- `NBB.MicroServicesOrchestration`

**Upgrade Approach**: Pattern C
**Expected Issues**: Process manager runtime, cross-context messaging
**Validation**: Orchestration flows, saga patterns

---

### Phase 7 (Level 6): Composed Applications

#### **Critical Project: `NBB.Mono`** (High Complexity - Monolith)

**Why Critical**: Composes all sample microservices into single monolith

**Current State**:
- Target Framework: net9.0
- 2 API issues (2 mandatory)
- Depends on: Contracts.Api, Invoices.Api/Application, Payments.Api/Application, MicroServicesOrchestration, all data access, EventStore, Messaging
- **Integration complexity**: Multiple microservice patterns in single app

**Migration Steps**:
1. Update target framework to net10.0
2. Update Program.cs/Startup.cs (similar to individual APIs/Workers)
3. Address 2 mandatory API issues
4. Test monolith startup (all services initialize correctly)
5. Test cross-service integration within monolith
6. Test event sourcing across contexts
7. Test messaging (in-process) across contexts
8. Run end-to-end scenarios
9. Performance test (ensure no degradation from microservice composition)

**Validation Checklist**:
- [ ] Monolith starts successfully
- [ ] All services registered correctly
- [ ] Cross-context messaging works (in-process)
- [ ] Cross-context event sourcing works
- [ ] End-to-end scenarios pass (contract ? invoice ? payment flows)
- [ ] No performance degradation

#### Group 7.1: Benchmarks (Pattern A/C)

**Projects** (1):
- `EventStoreBenchmarks`

**Upgrade Approach**: Mix of Pattern A and C (executable benchmark project)
**Expected Issues**: Benchmark framework compatibility
**Validation**: Benchmarks run successfully, establish .NET 10 baseline

#### Group 7.2: Migrations (Pattern A)

**Projects** (3):
- `NBB.Contracts.Migrations`
- `NBB.Invoices.Migrations`
- `NBB.Payments.Migrations`

**Upgrade Approach**: Pattern A + Pattern D (EF migrations)
**Expected Issues**: Migration execution
**Validation**: Migrations apply to test databases

---

### Phase 8 (Level 7): Top-Level Applications

#### Group 8.1: Aggregation (Pattern A)

**Projects** (1):
- `NBB.Mono.Migrations`

**Upgrade Approach**: Pattern A
**Expected Issues**: None - simple aggregation of Phase 7 migrations
**Validation**: All migrations execute successfully

**Migration Steps**:
1. Update target framework to net10.0
2. Rebuild (depends on Phase 7 migration projects, already upgraded)
3. Test migration execution against test database
4. Validate all schemas created correctly

---

### Package Update Reference Table

**Note**: Detailed package updates will be applied per phase, per project. Key packages:

| Package | Current | Target | Projects Affected | Phase(s) |
|---------|---------|--------|-------------------|----------|
| **Microsoft.EntityFrameworkCore** | 9.0.0 | 10.0.1 | 2 | 2, 3 |
| **Microsoft.EntityFrameworkCore.Design** | 9.0.0 | 10.0.1 | 4 | 4, 5 |
| **Microsoft.EntityFrameworkCore.InMemory** | 9.0.0 | 10.0.1 | 1 | 3 |
| **Microsoft.EntityFrameworkCore.Relational** | 9.0.0 | 10.0.1 | 1 | 3 |
| **Microsoft.EntityFrameworkCore.SqlServer** | 9.0.0 | 10.0.1 | 4 | 4 |
| **Microsoft.Extensions.Configuration.*** | 9.0.0 | 10.0.1 | 20+ | 1-6 |
| **Microsoft.Extensions.DependencyInjection.*** | 9.0.0 | 10.0.1 | 15+ | 2-6 |
| **Microsoft.Extensions.Hosting.*** | 9.0.0 | 10.0.1 | 10+ | 2, 5-6 |
| **Microsoft.Extensions.Logging.*** | 9.0.0 | 10.0.1 | 12+ | 3-6 |
| **Microsoft.Extensions.Options.*** | 9.0.0 | 10.0.1 | 5+ | 2-3 |
| **Newtonsoft.Json** | 13.0.3 | 13.0.4 | 7 | 2-3 |
| **System.Net.Http** | 4.3.4 | *Remove* | 2 | 2 |
| **SqlStreamStore.MsSql** | 1.2.0 | 1.2.0 | 2 | 2 (no change - deprecated) |
| **STAN.Client** | 0.3.0 | 0.3.0 | 1 | 2 (no change - deprecated) |

(Full table available in assessment.md)

## Testing & Validation Strategy

### Multi-Level Testing Approach

Testing occurs at three levels throughout the upgrade:

1. **Per-Project Testing** - During each project upgrade
2. **Phase Testing** - After completing each dependency level
3. **Full Solution Testing** - After Phase 8 completes

### Per-Project Testing

**When**: Immediately after each project upgrade (framework + packages + code fixes)

**What to Test**:
- ? **Build Success**: Project compiles without errors
- ? **No Warnings**: No new warnings introduced (or justified if unavoidable)
- ? **Unit Tests**: All project unit tests pass (if present)
- ? **No Dependency Conflicts**: Package restore succeeds, no version conflicts
- ? **Public API Compatibility**: If library project, public API surface unchanged (unless intentional breaking change)

**How**:
```bash
# Per project after upgrade
dotnet build src/Core/NBB.Core.Abstractions/NBB.Core.Abstractions.csproj --configuration Release
dotnet test src/Core/NBB.Core.Abstractions/NBB.Core.Abstractions.csproj --configuration Release
```

**Pass Criteria**:
- Exit code 0 for build
- Exit code 0 for test (if tests present)
- 100% test pass rate

### Phase Testing

**When**: After all projects in a phase are upgraded and individually validated

**What to Test**:
- ? **Phase Build**: All phase projects build together
- ? **Integration Tests**: Tests that span multiple projects in phase
- ? **Cross-Phase Tests**: Validate phase integrates with lower phases (already on net10.0)
- ? **Forward Compatibility**: Higher phases (still on net9.0) still work with upgraded dependencies
- ? **Regression Tests**: Lower phase tests still pass (no regressions introduced)

**Phase-Specific Testing**:

#### Phase 1 (L0) - Foundation
- Validate all foundation libraries build together
- Test configuration loading (NBB.Core.Configuration)
- Test pipeline construction (NBB.Core.Pipeline)
- Test effect composition (NBB.Core.Effects)
- No integration tests (no dependencies)

#### Phase 2 (L1) - Foundation Extensions
- Test MediatR request/response flows
- Test correlation context propagation
- Test messaging abstractions (topic subscription, message dispatch)
- Test multi-tenancy abstractions (tenant resolution, configuration)
- Run all Phase 1 tests again (regression check)

#### Phase 3 (L2) - Core Features
- **Critical**: Entity Framework Core 10.0 validation
  - CRUD operations
  - Complex LINQ queries
  - Connection resilience
  - Migration generation
- Test EventStore operations (append, read streams, snapshots)
- Test messaging implementations (Nats, JetStream, Rusi, InProcess)
- Test HTTP effects (client factory, retry policies)
- Run all Phase 1-2 tests again (regression check)

**Phase 3 Critical Test Scenarios**:
```csharp
// EF Core 10.0 - Query Translation
var complexQuery = context.Invoices
    .Where(i => i.Status == Status.Pending)
    .GroupBy(i => i.CustomerId)
    .Select(g => new { CustomerId = g.Key, Count = g.Count(), Total = g.Sum(i => i.Amount) })
    .ToList();

// EventStore - Concurrency
var stream1 = await eventStore.LoadStreamAsync("invoice-123", expectedVersion: 5);
var stream2 = await eventStore.LoadStreamAsync("invoice-123", expectedVersion: 5);
// ... modify both, append should conflict

// Messaging - Round Trip
await publisher.PublishAsync(new ContractCreatedEvent { ContractId = "123" });
// Assert subscriber receives event
```

#### Phase 4 (L3) - Features & Integration
- Test domain aggregate behavior
- Test event-sourced persistence (write and read)
- Test multi-tenancy data isolation
- Test JWT authentication and tenant identification
- Test F# interop (C# ? F# ? C# round trip)
- Run all Phase 1-3 tests again (regression check)

**Phase 4 Critical Test Scenarios**:
```csharp
// Multi-Tenancy - Data Isolation
using (tenantContext.SetTenant("tenant1")) {
    var data1 = await repository.GetAllAsync();
    // Assert data is tenant1 only
}
using (tenantContext.SetTenant("tenant2")) {
    var data2 = await repository.GetAllAsync();
    // Assert data is tenant2 only, different from data1
}

// JWT Tenant Identification
var token = GenerateJwtWithTenant("tenant1");
httpContext.Request.Headers.Authorization = $"Bearer {token}";
var tenant = await tenantIdentifier.IdentifyTenantAsync(httpContext);
// Assert tenant == "tenant1"
```

#### Phase 5 (L4) - Application Logic
- Test application commands and queries (MediatR handlers)
- Test cross-context integration (Contracts ? Invoices ? Payments)
- Test application APIs (HTTP endpoints)
- Test worker services (message processing)
- Run all Phase 1-4 tests again (regression check)

**Phase 5 Critical Test Scenarios**:
```csharp
// Application Command
var command = new CreateInvoiceCommand { ContractId = "123", Amount = 1000 };
var result = await mediator.Send(command);
// Assert invoice created, events published

// API Integration Test
var response = await httpClient.PostAsJsonAsync("/api/invoices", createRequest);
response.EnsureSuccessStatusCode();
var invoice = await response.Content.ReadFromJsonAsync<InvoiceDto>();
// Assert invoice created correctly
```

#### Phase 6 (L5) - Worker Services & APIs
- Test worker startup and configuration
- Test end-to-end message processing (publish ? subscribe ? process)
- Test API health checks and startup
- Test OpenTelemetry tracing (spans recorded correctly)
- Test multi-tenant worker scenarios
- Run all Phase 1-5 tests again (regression check)

**Phase 6 Critical Test Scenarios**:
```csharp
// Worker Startup
var worker = host.Services.GetRequiredService<IHostedService>();
await worker.StartAsync(CancellationToken.None);
// Assert worker started, health check passes

// End-to-End Message Processing
await publisher.PublishAsync(new ContractCreatedEvent { ContractId = "123" });
await Task.Delay(1000); // Allow processing
var invoice = await invoiceRepository.GetByContractId("123");
// Assert invoice created from contract event

// Multi-Tenant Worker
await publisher.PublishAsync(new TodoCreatedEvent { TodoId = "1", TenantId = "tenant1" });
await publisher.PublishAsync(new TodoCreatedEvent { TodoId = "2", TenantId = "tenant2" });
// Assert events routed to correct tenant contexts
```

#### Phase 7 (L6) - Composed Applications
- Test monolith startup (all services initialize)
- Test monolith end-to-end scenarios
- Test benchmark execution
- Test migration aggregation
- Run all Phase 1-6 tests again (regression check)

**Phase 7 Critical Test Scenarios**:
```csharp
// Monolith - Cross-Service Integration
var contract = await contractService.CreateContractAsync(createRequest);
var invoice = await invoiceService.CreateInvoiceForContractAsync(contract.Id);
var payment = await paymentService.CreatePaymentForInvoiceAsync(invoice.Id);
// Assert entire flow works within monolith

// Benchmarks
BenchmarkRunner.Run<EventStoreBenchmarks>();
// Assert benchmarks complete, establish baseline
```

#### Phase 8 (L7) - Top-Level
- Test aggregated migrations (all schemas created)
- Run all Phase 1-7 tests again (final regression check)

### Full Solution Testing

**When**: After Phase 8 completes, before marking upgrade complete

**What to Test**:
- ? **Complete Build**: Entire solution builds (`dotnet build NBB.slnx`)
- ? **All Tests Pass**: All unit + integration tests (`dotnet test NBB.slnx`)
- ? **No Warnings**: Build produces minimal warnings
- ? **No Conflicts**: No package version conflicts
- ? **End-to-End Scenarios**: Complete business flows work
- ? **Performance**: No significant performance degradation
- ? **Security**: No new security vulnerabilities

**End-to-End Scenarios to Validate**:

1. **Contract ? Invoice ? Payment Flow** (Monolith and Microservices):
   - Create contract
   - Generate invoice from contract
   - Process payment for invoice
   - Verify events published at each step
   - Verify event sourcing captured state transitions
   - Verify cross-context messaging worked

2. **Multi-Tenant Todo Workflow**:
   - Create todos for Tenant1
   - Create todos for Tenant2
   - Verify data isolation (Tenant1 can't see Tenant2 data)
   - Verify message routing works per tenant
   - Verify event store isolation per tenant

3. **Process Manager Orchestration**:
   - Start long-running process
   - Verify state transitions
   - Trigger compensation scenario
   - Verify saga completes correctly

4. **F# Integration**:
   - Test F# API endpoints
   - Test F# worker message processing
   - Verify F#/C# interop bidirectional

**Performance Baseline**:
- Establish baseline with benchmarks
- Compare .NET 9 vs .NET 10 performance
- Acceptable range: ±20% (investigate if outside)

**Security Validation**:
```bash
# Check for security vulnerabilities
dotnet list package --vulnerable --include-transitive

# Expected: No vulnerabilities
```

### Testing Tools and Commands

**Build All**:
```bash
dotnet build NBB.slnx --configuration Release
```

**Test All**:
```bash
dotnet test NBB.slnx --configuration Release --no-build
```

**Test Specific Phase** (example: Phase 3):
```bash
dotnet test src/Data/NBB.Data.EntityFramework.Tests/NBB.Data.EntityFramework.Tests.csproj
dotnet test src/EventStore/NBB.EventStore.Tests/NBB.EventStore.Tests.csproj
# ... (all Phase 3 test projects)
```

**Package Vulnerability Check**:
```bash
dotnet list package --vulnerable --include-transitive
```

**Performance Benchmarks**:
```bash
dotnet run --project test/Benchmarks/EventStoreBenchmarks/EventStoreBenchmarks.csproj -c Release
```

### Test Pass Criteria Summary

| Test Level | Pass Criteria |
|------------|---------------|
| **Per-Project** | Build succeeds, unit tests 100% pass, no new warnings |
| **Phase** | All phase projects build together, integration tests pass, no regressions in lower phases |
| **Full Solution** | Entire solution builds, all tests pass (unit + integration), end-to-end scenarios work, performance acceptable, no security vulnerabilities |

### Handling Test Failures

**If Tests Fail**:

1. **Isolate Failure**:
   - Which test(s) failed?
   - Which project/phase?
   - Is it a code issue or test issue?

2. **Categorize**:
   - **Test needs update**: Test assumptions invalid for .NET 10 (update test)
   - **Code issue**: Actual bug introduced (fix code)
   - **Behavior change**: Expected .NET 10 behavior change (update test + document)

3. **Fix Priority**:
   - Mandatory breaking changes: Fix immediately
   - Behavioral changes: Validate behavior change is acceptable
   - Optional warnings: Can defer to later cleanup

4. **Retest**:
   - Run failed test again
   - Run related tests
   - Run full phase tests

## Source Control Strategy

### Branch Strategy

**Single Upgrade Branch**: `upgrade/net10.0`

**Rationale**:
- Bottom-up approach maintains buildable solution after each phase
- No need for multiple feature branches (sequential phase execution)
- Clear linear history of upgrade progress
- Single PR for final review reduces overhead

**Branch Structure**:
```
main (or master) [net9.0]
  ?
upgrade/net10.0 [net9.0 ? net10.0, phase by phase]
  ?
[After all phases complete and validated]
  ?
main (or master) [net10.0]
```

### Branch Creation

**Initial Setup**:
1. Ensure current branch is clean (no pending changes)
2. Create upgrade branch from main:
   ```bash
   git checkout main
   git pull origin main
   git checkout -b upgrade/net10.0
   git push -u origin upgrade/net10.0
   ```

### Commit Strategy

**Commit Frequency**: **One commit per logical checkpoint**

**Phase Commit Structure** (typical 5 commits per phase):

1. **Phase Preparation Commit** (optional, if significant planning work):
   ```
   Phase X: Preparation - Plan X projects for upgrade
   
   - Review project dependencies
   - Identify phase-specific risks
   - Document upgrade approach
   ```

2. **Framework & Package Update Commit**:
   ```
   Phase X: Update target framework and packages
   
   - Update <TargetFramework> net9.0 ? net10.0 for all phase projects
   - Update Microsoft.Extensions.* packages 9.0.0 ? 10.0.1
   - Update Entity Framework packages 9.0.0 ? 10.0.1
   - Update other packages per package update table
   
   Projects updated:
   - src/Core/NBB.Core.Abstractions/NBB.Core.Abstractions.csproj
   - src/Core/NBB.Core.Configuration/NBB.Core.Configuration.csproj
   - ...
   ```

3. **Compilation Fix Commit(s)** (may be multiple if complex):
   ```
   Phase X: Fix compilation errors
   
   - Fix ConfigurationBinder.GetValue calls (explicit type parameters)
   - Fix TimeSpan factory method calls (explicit double cast)
   - Fix ServiceCollectionExtensions binary incompatibility (recompile)
   
   Files modified:
   - src/MultiTenancy/NBB.MultiTenancy.Abstractions/TenantConfiguration.cs
   - src/Messaging/NBB.Messaging.Host/HostConfiguration.cs
   - ...
   ```

4. **Test Fix Commit** (if tests failed and needed updates):
   ```
   Phase X: Fix test failures
   
   - Update test expectations for .NET 10 behavioral changes
   - Fix test data setup for new EF Core query translation
   - Update mocks for changed service signatures
   
   Test projects updated:
   - test/UnitTests/Core/NBB.Core.Configuration.Tests/...
   - test/UnitTests/Data/NBB.Data.EntityFramework.Tests/...
   ```

5. **Phase Completion Commit**:
   ```
   Phase X: Complete - Level X projects upgraded to .NET 10
   
   ? All phase projects build successfully
   ? All phase tests pass (100% pass rate)
   ? No regressions in lower phases
   ? Forward compatibility validated
   
   Summary:
   - X projects upgraded
   - Y packages updated
   - Z API compatibility issues resolved
   - All tests passing
   
   Next: Phase X+1
   ```

### Commit Message Format

**Standard Format**:
```
Phase X: <Action> - <Summary>

<Detailed description>

<Optional metadata: files changed, issues resolved, etc.>
```

**Examples**:

```
Phase 3: Update Entity Framework Core to 10.0.1

- Update Microsoft.EntityFrameworkCore packages 9.0.0 ? 10.0.1
- Update EF Core providers (SqlServer, InMemory)
- No compilation errors introduced

Projects affected:
- src/Data/NBB.Data.EntityFramework/NBB.Data.EntityFramework.csproj
- src/Data/NBB.Data.EntityFramework.MultiTenancy/NBB.Data.EntityFramework.MultiTenancy.csproj
```

```
Phase 4: Fix IdentityModel breaking changes

Migrated from System.IdentityModel.Tokens.Jwt to modern Microsoft.IdentityModel.* APIs

- Replace JwtSecurityTokenHandler with JsonWebTokenHandler
- Update JWT validation logic
- Update claims extraction

Fixes:
- NBB.MultiTenancy.Identification.Http: JWT authentication flow
- All multi-tenant authentication tests pass

Files modified:
- src/MultiTenancy/NBB.MultiTenancy.Identification.Http/JwtTenantIdentifier.cs
```

### Pull Request Strategy

**Single PR After All Phases Complete**

**Rationale**:
- Bottom-up ensures solution buildable at every checkpoint (no WIP states)
- Easier review of complete upgrade vs. 8 separate PRs
- Clear diff: net9.0 ? net10.0
- All phases validated together

**PR Creation**:
```bash
# After Phase 8 completes and full solution testing passes
git push origin upgrade/net10.0

# Create PR: upgrade/net10.0 ? main
# Title: "Upgrade solution to .NET 10.0"
```

**PR Description Template**:
```markdown
## Upgrade to .NET 10.0

This PR upgrades the entire NBB solution from .NET 9.0 to .NET 10.0.

### Summary

- **Projects Upgraded**: 126 (all projects)
- **Packages Updated**: 31 packages
- **API Compatibility Issues Resolved**: 98 breaking changes
- **Phases Completed**: 8 (Level 0 ? Level 7)

### Upgrade Approach

**Strategy**: Bottom-Up (Dependency-First)
- Upgraded projects in 8 phases matching dependency levels
- Each phase validated before proceeding to next
- No circular dependencies, clean tier-by-tier progression

### Key Changes

#### Framework
- All projects: `<TargetFramework>net9.0</TargetFramework>` ? `<TargetFramework>net10.0</TargetFramework>`

#### Packages
- Entity Framework Core: 9.0.0 ? 10.0.1
- Microsoft.Extensions.*: 9.0.0 ? 10.0.1 (23 packages)
- Newtonsoft.Json: 13.0.3 ? 13.0.4
- (See full list in commit history)

#### Breaking Changes Addressed
- **ConfigurationBinder.GetValue**: Updated calls with explicit type parameters
- **TimeSpan factory methods**: Added explicit type casts (int ? double)
- **IdentityModel APIs**: Migrated to Microsoft.IdentityModel.* packages
- **ServiceCollectionExtensions**: Binary compatibility (recompilation)
- **ConsoleLogger**: Behavioral changes validated (6 occurrences)

### Testing

? **All Tests Pass**:
- Unit tests: 100% pass rate
- Integration tests: 100% pass rate
- End-to-end scenarios: All validated

? **Performance**:
- Benchmarks run successfully
- Performance within acceptable range (±20%)

? **Security**:
- No security vulnerabilities detected (`dotnet list package --vulnerable`)

### Validation Checklist

- [x] All 126 projects build successfully
- [x] No package dependency conflicts
- [x] All unit tests pass (100% pass rate)
- [x] All integration tests pass
- [x] End-to-end scenarios validated:
  - [x] Contract ? Invoice ? Payment flow
  - [x] Multi-tenant Todo workflow
  - [x] Process manager orchestration
  - [x] F# integration
- [x] No new warnings introduced
- [x] No security vulnerabilities
- [x] Performance acceptable
- [x] Deprecated packages documented

### Phases Completed

1. ? Phase 1 (L0): Foundation - 18 projects
2. ? Phase 2 (L1): Foundation Extensions - 15 projects
3. ? Phase 3 (L2): Core Features - 25 projects
4. ? Phase 4 (L3): Features & Integration - 21 projects
5. ? Phase 5 (L4): Application Logic - 19 projects
6. ? Phase 6 (L5): Worker Services & APIs - 11 projects
7. ? Phase 7 (L6): Composed Applications - 5 projects
8. ? Phase 8 (L7): Top-Level - 1 project

### Known Issues / Technical Debt

- **SqlStreamStore.MsSql**: Remains at 1.2.0 (deprecated, no replacement) - Plan future migration
- **STAN.Client**: Remains at 0.3.0 (deprecated) - Consider migrating to NBB.Messaging.JetStream

### Deployment Notes

- **Recommended**: Deploy after thorough QA testing
- **Rollback Plan**: Revert to main branch (net9.0) if issues discovered
- **Monitoring**: Watch for .NET 10 runtime-specific issues post-deployment

### Review Focus Areas

1. **Entity Framework Core 10.0**: Data access patterns (Phase 3-4 commits)
2. **Multi-Tenancy**: Tenant identification and data isolation (Phase 4-6 commits)
3. **IdentityModel Migration**: JWT authentication (Phase 4 commits)
4. **Worker Services**: Startup and configuration (Phase 6 commits)

---

Closes #[issue-number] (if tracked in issue)
```

### Review Process

**Recommended Review Approach**:

1. **Phase-by-Phase Review**:
   - Review commits per phase (grouped logically)
   - Validate phase checkpoint commits show phase complete

2. **Focus Areas**:
   - Critical project upgrades (NBB.Data.EntityFramework, NBB.EventStore, NBB.MultiTenancy.Identification.Http)
   - Breaking change resolutions
   - Test coverage (no tests removed without justification)

3. **Approval Criteria**:
   - All checklist items completed
   - CI/CD pipeline passes (build + test)
   - Manual QA validation complete
   - Documentation updated (if needed)

### Merge Strategy

**Recommended**: **Squash and Merge** OR **Merge Commit**

**Squash and Merge**:
- **Pros**: Clean main branch history (single commit for upgrade)
- **Cons**: Lose phase-by-phase history in main branch
- **Use when**: Team prefers clean linear history

**Merge Commit**:
- **Pros**: Preserve all phase commits and checkpoint history
- **Cons**: More commits in main branch
- **Use when**: Team values detailed history

**Command** (after PR approval):
```bash
# Squash and merge (via GitHub/Azure DevOps UI)
# OR
# Merge commit
git checkout main
git merge upgrade/net10.0 --no-ff
git push origin main
```

### Post-Merge

**Tag Release**:
```bash
git tag -a v10.0.0 -m "Upgraded to .NET 10.0"
git push origin v10.0.0
```

**Delete Upgrade Branch** (optional, after successful deployment):
```bash
git branch -d upgrade/net10.0
git push origin --delete upgrade/net10.0
```

## Success Criteria

### Technical Success Criteria

The .NET 10.0 upgrade is considered **technically successful** when **ALL** of the following criteria are met:

#### 1. Framework Migration Complete

- ? **All 126 projects** target `<TargetFramework>net10.0</TargetFramework>`
- ? **No projects remaining on net9.0** (except intentionally excluded, if any - currently none)
- ? **Solution file (NBB.slnx)** references all net10.0 projects

**Verification**:
```bash
# Should return 0 projects
grep -r "<TargetFramework>net9.0</TargetFramework>" --include="*.csproj" --include="*.fsproj"
```

#### 2. Package Updates Applied

- ? **All 31 recommended package updates applied**
- ? **Entity Framework Core**: All projects using EF Core upgraded to 10.0.1
- ? **Microsoft.Extensions.***: All projects using Extensions packages upgraded to 10.0.1
- ? **No package dependency conflicts** (no version mismatch errors)

**Verification**:
```bash
dotnet restore NBB.slnx
# Should complete without errors or warnings
```

#### 3. Build Success

- ? **Entire solution builds**: `dotnet build NBB.slnx --configuration Release` succeeds (exit code 0)
- ? **No compilation errors** (0 errors)
- ? **Minimal warnings** (no new warnings introduced, existing warnings acceptable if documented)
- ? **All projects compile** individually

**Verification**:
```bash
dotnet build NBB.slnx --configuration Release
# Exit code: 0
# Errors: 0
```

#### 4. Test Success

- ? **All unit tests pass**: 100% pass rate
- ? **All integration tests pass**: 100% pass rate
- ? **Test coverage maintained**: No reduction in test coverage percentage
- ? **No skipped tests**: All tests execute (unless explicitly skipped with justification)

**Verification**:
```bash
dotnet test NBB.slnx --configuration Release --no-build
# Overall test result: Passed
# Failed: 0, Passed: [total-count], Skipped: 0
```

#### 5. Breaking Changes Resolved

- ? **All 98 mandatory breaking API changes addressed**
- ? **ConfigurationBinder calls** updated (12 occurrences)
- ? **TimeSpan factory methods** fixed (15 occurrences)
- ? **IdentityModel APIs** migrated (5 occurrences)
- ? **ServiceCollectionExtensions** recompiled (28 occurrences)
- ? **Console logger** behavioral changes validated (6 occurrences)

**Verification**: All compilation errors resolved, all tests pass

#### 6. Security

- ? **No security vulnerabilities** in package dependencies
- ? **Deprecated packages documented**: SqlStreamStore, STAN.Client kept at current versions with tech debt documented

**Verification**:
```bash
dotnet list package --vulnerable --include-transitive
# Result: No vulnerable packages found
```

#### 7. Performance

- ? **No significant performance degradation**: Performance within ±20% of .NET 9.0 baseline
- ? **Benchmarks establish new baseline**: EventStoreBenchmarks run successfully on .NET 10.0

### Quality Success Criteria

#### 8. Code Quality

- ? **No new code smells introduced**: Code changes limited to framework/package compatibility
- ? **Architecture preserved**: No architectural changes required for upgrade
- ? **Design patterns maintained**: Existing patterns (event sourcing, CQRS, messaging) work correctly

#### 9. Documentation

- ? **Upgrade plan documented**: This plan.md exists and is complete
- ? **Breaking changes documented**: All breaking change resolutions documented in commit messages
- ? **Technical debt documented**: Deprecated packages noted in tech debt backlog
- ? **Lessons learned captured**: Patterns and solutions recorded for future reference

#### 10. Source Control

- ? **Branch strategy followed**: Single `upgrade/net10.0` branch used
- ? **Commit strategy followed**: Phase-by-phase commits with clear messages
- ? **PR created**: Single PR for entire upgrade with complete description
- ? **Review completed**: Code review approved by designated reviewers

### Process Success Criteria

#### 11. Phase Completion

- ? **All 8 phases completed successfully**:
  1. Phase 1 (L0): Foundation - ?
  2. Phase 2 (L1): Foundation Extensions - ?
  3. Phase 3 (L2): Core Features - ?
  4. Phase 4 (L3): Features & Integration - ?
  5. Phase 5 (L4): Application Logic - ?
  6. Phase 6 (L5): Worker Services & APIs - ?
  7. Phase 7 (L6): Composed Applications - ?
  8. Phase 8 (L7): Top-Level - ?

- ? **Each phase validated before next**: No skipped validations

#### 12. Bottom-Up Strategy Applied

- ? **Dependency order respected**: No project upgraded before its dependencies
- ? **Tier batching used**: Projects within levels upgraded together
- ? **No multi-targeting needed**: All projects on net10.0, no mixed framework solution
- ? **Incremental validation**: Each phase checkpoint reached

### Functional Success Criteria

#### 13. End-to-End Scenarios Validated

- ? **Contract ? Invoice ? Payment Flow**: Complete business flow works in monolith and microservices
- ? **Multi-Tenant Todo Workflow**: Tenant isolation validated, data and messaging separated correctly
- ? **Process Manager Orchestration**: Long-running processes and sagas complete successfully
- ? **F# Integration**: F# Integration**: F# projects integrate correctly with C# projects bidirectionally

**Verification**: Execute end-to-end test scenarios (manual or automated)

#### 14. Application Functionality Preserved

- ? **All sample applications start**: APIs, worker services, monolith start without errors
- ? **Configuration loading works**: appsettings.json, environment variables, user secrets load correctly
- ? **Dependency injection works**: All services resolve correctly
- ? **Middleware pipelines work**: ASP.NET Core middleware executes correctly
- ? **Messaging works**: Pub/sub patterns function across all messaging providers
- ? **Event sourcing works**: Aggregate persistence and retrieval via event store
- ? **Multi-tenancy works**: Tenant identification, isolation, and routing

#### 15. Data Access Validated

- ? **Entity Framework Core 10.0**: CRUD operations, LINQ queries, migrations all work
- ? **Event Store**: Append events, read streams, snapshots, concurrency handling work
- ? **Multi-tenant data**: Tenant isolation maintained in database and event store

### Deployment Readiness Criteria

#### 16. Production Readiness

- ? **Monitoring configured**: Application insights, logging, tracing work on .NET 10.0
- ? **Health checks pass**: All health check endpoints return healthy
- ? **Deployment tested**: Upgrade validated in dev/staging environment
- ? **Rollback plan ready**: Clear rollback procedure documented

#### 17. Team Readiness

- ? **Team trained**: Development team aware of .NET 10.0 changes
- ? **Documentation updated**: README, wiki, and developer guides reflect .NET 10.0
- ? **DevOps updated**: CI/CD pipelines updated for .NET 10.0 (if not using SDK-based)

### Final Acceptance

**The upgrade is considered COMPLETE and ready for merge/deployment when**:

? **All Technical Criteria met** (items 1-7)
? **All Quality Criteria met** (items 8-10)
? **All Process Criteria met** (items 11-12)
? **All Functional Criteria met** (items 13-15)
? **All Deployment Readiness Criteria met** (items 16-17)

**Sign-Off Required From**:
- Technical Lead (verify technical criteria)
- QA Lead (verify functional criteria)
- DevOps Lead (verify deployment readiness)

**Final Checklist**:
```markdown
- [ ] All 126 projects on net10.0
- [ ] All 31 packages updated
- [ ] Solution builds (0 errors)
- [ ] All tests pass (100% pass rate)
- [ ] All breaking changes resolved
- [ ] No security vulnerabilities
- [ ] Performance acceptable
- [ ] All 8 phases complete
- [ ] Bottom-up strategy applied correctly
- [ ] End-to-end scenarios validated
- [ ] All applications start and function
- [ ] Data access validated
- [ ] Monitoring/health checks work
- [ ] Team ready for deployment
- [ ] Code review approved
- [ ] Documentation updated
```

**Once all criteria met**: Merge PR to main, tag release, deploy to production.

---

**End of Plan**
