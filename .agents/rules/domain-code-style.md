---
trigger: model_decision
description: >-
  Code style and design rules for the Domain layer (*.Domain, BuildingBlocks.Domain) across all AquaAPI microservices:
  entities, DomainGuard list-based validation, Value Objects, and Domain Events.
  Apply whenever creating or modifying files in any *.Domain or BuildingBlocks.Domain project.
---

# Domain Layer Code Style (`*.Domain`, `BuildingBlocks.Domain`)

1. **Entities (`Entities/`):**
   - Declare as `public sealed class <Entity> : AggregateRoot, IEntity` (or `IdentityUser<Guid>, IEntity, IHasDomainEvents`).
   - Provide a `private` primary constructor and a `private` parameterless constructor for EF Core wrapped in `#pragma warning disable CS8618` / `#pragma warning restore CS8618`.
   - Properties use `{ get; private set; }`. Collections are backed by `private readonly List<T> _items = [];` and exposed as `IReadOnlyCollection<T>` / `IReadOnlyList<T>` via `.AsReadOnly()`.
   - Define length/range limits as `public const int` inside the entity.
   - Create entities only via `public static Result<TEntity> Create(...)` and mutate via domain methods returning `Result` / `Result<T>` (calling `IncrementVersion()` and `RaiseEvent(...)`; ensure `Version` is mapped with `.IsConcurrencyToken()` in Infrastructure). Never throw exceptions for domain validation.

2. **Validation & Error Aggregation (`DomainGuard`):**
   - In `Create(...)` and multi-field methods, collect errors into `var errors = new List<string>();`.
   - Validate primitives with `DomainGuard.CheckNotEmpty`, `CheckText`, `CheckOptionalText`, `CheckEnum`, `CheckPositive`, `CheckNonNegative`, `CheckInRange`.
   - Validate nested Value Objects with `DomainGuard.Collect(Vo.Create(...), errors)`.
   - When `errors.Count != 0`, return `DomainGuard.ValidationFailure<TEntity>(errors)` (for `Result<TEntity>`) or `Result.Failure(Error.Validation<TEntity>(string.Join(", ", errors)))` (for `Result`).
   - For single-check update methods, `DomainGuard.Require*` or `Result.Failure(Error.Validation<TEntity>(...))` is acceptable.

3. **Value Objects (`ValueObjects/`):**
   - Declare as `public sealed record` (or `public sealed partial record` with `[GeneratedRegex]`), immutable (`get`-only properties), with a `private`/`internal` constructor.
   - Provide `Parse(...)` (no validation, **strictly for EF Core converters**) and `Create(...)` returning `Result<TVo>`. Never call `Parse(...)` on external/untrusted input (DTOs, query params, integration events, bot input).
   - Override `ToString()`. Shared cross-service Value Objects (e.g., `EmailAddress`, `Money`, `DateRange`) live in `BuildingBlocks.Domain.ValueObjects`.

4. **Domain Events (`Events/`):**
   - Declare as `public sealed record <Action>DomainEvent : IDomainEvent` with `init`-only properties.
   - Include `public DateTime OccurredOn { get; init; } = DateTime.UtcNow;` and initialize string properties to `string.Empty`.

5. **Time & Normalization:**
   - Always use `DateTime.UtcNow`.
   - Normalize strings (`Trim()`, `ToLowerInvariant()` / `ToUpperInvariant()`, `CultureInfo.InvariantCulture`) inside `Create` / `DomainGuard.CheckText`.
