---
trigger: model_decision
description: >-
  Testing rules and conventions for the Infrastructure layer (*.Infrastructure.IntegrationTests,
  BuildingBlocks.Infrastructure.IntegrationTests, BuildingBlocks.TestShared): Testcontainers PostgreSQL,
  cached Respawn, and Outbox tests.
  Apply whenever creating, modifying, or reviewing tests in any *.Infrastructure.IntegrationTests project.
---

# Infrastructure Layer Testing Standards (`*.Infrastructure.IntegrationTests`)

1. **Test Host & Database Isolation:**
   - Inherit the service `IntegrationTestWebAppFactory` from `BaseIntegrationTestWebAppFactory<TProgram, TDbContext>` in `BuildingBlocks.TestShared` (starts a `postgres:16-alpine` Testcontainer, replaces MassTransit with InMemory, disables Quartz hosted jobs, wires `TestUserContext`, and runs EF migrations).
   - Inherit test classes from `BaseIntegrationTest` using `[Collection("IntegrationTestCollection")]` and reset database state before each test with `Respawn` (`TablesToIgnore = ["__EFMigrationsHistory"]`).
   - **Performance:** create and cache `Respawner` once per factory/collection (not via `Respawner.CreateAsync(...)` inside every test's `InitializeAsync()`), calling only `ResetAsync(connection)` per test.
   - Reset `UserContext.UserId` and `UserContext.IsAuthenticated` in `InitializeAsync()`.

2. **Test Structure & Assertions:**
   - Use **xUnit**, **FluentAssertions**, `<MethodOrFeature>_<Condition>_<ExpectedOutcome>` naming with `// Arrange`, `// Act`, `// Assert`, and a fresh `IServiceScope` (`Sender`, `DbContext`) per test.
   - Always call `DbContext.ChangeTracker.Clear()` after seeding or acting when verifying persisted database state with `.AsNoTracking()`.

3. **Scope of Outbox & Shared Infrastructure Tests:**
   - Core Outbox behavior (poison messages, invalid JSON, `null` deserialization, retry/backoff, `MaxRetries`, `CleanupAsync`) is tested in `BuildingBlocks.Infrastructure.IntegrationTests`.
   - Service `*.Infrastructure.IntegrationTests` should only keep smoke tests verifying that the service `DbContext` has `OutboxMessages` migrated and `ConvertDomainEventsToOutboxMessagesInterceptor` / `OutboxMessageProcessorService<TDbContext>` wired in DI.
