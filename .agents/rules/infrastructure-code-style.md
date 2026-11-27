---
trigger: model_decision
description: >-
  Code style and design rules for the Infrastructure layer (*.Infrastructure, BuildingBlocks.Infrastructure)
  across all AquaAPI microservices: EF Core DbContext, concurrency tokens, ValueConverters, Outbox,
  repositories, Dapper SQL safety, and background jobs.
  Apply whenever creating or modifying files in any *.Infrastructure or BuildingBlocks.Infrastructure project.
---

# Infrastructure Layer Code Style (`*.Infrastructure`, `BuildingBlocks.Infrastructure`)

1. **EF Core `DbContext` & Entity Configurations (`Persistence/`):**
   - Use primary constructor `(DbContextOptions<TDbContext> options) : DbContext(options)`.
   - Expose `public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();` alongside service `DbSet<T>` properties.
   - In `OnModelCreating`, call `modelBuilder.ApplyConfigurationsFromAssembly(typeof(TDbContext).Assembly);` and `modelBuilder.ConfigureOutbox();`.
   - In `ConfigureConventions`, call `configurationBuilder.AddValueConverters();`.
   - In every `IEntityTypeConfiguration<T>` for entities implementing `IHasVersion` / `AggregateRoot`, configure `builder.Property(x => x.Version).IsConcurrencyToken();` so `IncrementVersion()` actually prevents lost updates.
   - Register PostgreSQL via `EntityFrameworkExtensions.AddPostgresDbContext<TDbContext>(configuration)` from `BuildingBlocks.Infrastructure`.

2. **Value Object Converters (`Persistence/Converters/`):**
   - Implement `internal class <Vo>Converter : ValueConverter<TVo, TPrimitive>` mapping `vo => vo.Value` (or `vo.ToString()`) and `dbVal => TVo.Parse(dbVal)`.
   - Use `Parse(...)` strictly in EF Core converters (never `Create(...)` here, and never `Parse(...)` on untrusted external input).
   - Register all converters and column limits (`HaveMaxLength`, `HavePrecision`) in `Extensions.AddValueConverters(this ModelConfigurationBuilder)`.

3. **Repositories, Queries & SQL Safety:**
   - Inherit `BaseRepository<TDbContext, TEntity>` (note parameter order: `TDbContext` first, `TEntity` second).
   - **Write vs. Read separation:** `BaseRepository.GetByIdAsync` (`FindAsync`) tracks entities for writes; read-only CQRS queries must use `AsNoTracking()` with DTO projections (`.Select(...)`) or Dapper, and always propagate `CancellationToken`.
   - **Caution with `BaseRepository.DeleteAsync(Guid id)`:** it runs `ExecuteDeleteAsync` directly by `Id`, bypassing EF `ChangeTracker`, optimistic concurrency (`Version`), domain events (`Outbox`), and `UserId` checks. Use it only after ownership verification when no domain events/concurrency checks are needed; otherwise load the aggregate and delete via `DbContext.Remove(...)` or filter explicitly by `UserId`.
   - **Dapper / raw SQL (`ISqlConnectionFactory`):** always use parameterized queries (`@Param`) and never interpolate user input into SQL strings.
   - Keep `*.Infrastructure` referenced to `BuildingBlocks.Infrastructure` (enforced by `ServiceLayerArchitectureTestsBase`) and reuse Outbox/Quartz/Redis/MassTransit extensions from `BuildingBlocks`.
