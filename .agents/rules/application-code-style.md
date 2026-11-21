---
trigger: model_decision
description: >-
  Code style and design rules for the Application layer (*.Application, BuildingBlocks.Application,
  TelegramBotService,Application) across all AquaAPI microservices: CQRS commands/queries, sealed handlers,
  FluentValidation validators, pipeline behaviors, cache/tenant safety, and DTOs.
  Apply whenever creating or modifying files in any Application project.
---

# Application Layer Code Style (`*.Application`, `BuildingBlocks.Application`)

1. **CQRS Requests (`Features/<Feature>/Commands|Queries/`):**
   - Declare requests as `public sealed record <Name>Command` or `<Name>Query` with `init`-only properties and `string.Empty` / `[]` defaults.
   - Implement `ICommand`, `ICommand<TResponse>`, `IQuery<TResponse>`, or `ICachedQuery<TResponse>` from `BuildingBlocks.Domain.Abstractions`.
   - User-scoped requests must implement `IUserBoundRequest` (or a service-specific marker like `IEcosystemBoundRequest`) so ownership/security behaviors can enforce tenant isolation.
   - Respect naming conventions enforced by `ApplicationArchitectureTestsBase`: end names with `Command` or `Query`.

2. **Handlers (`*Handler`):**
   - Declare as `public sealed class <Name>Handler(...) : IRequestHandler<TRequest, Result>` (or `Result<TResponse>`) using primary constructors for DI.
   - Name must end with `Handler` and be `sealed`.
   - Generate new entity IDs with `NewId.NextGuid()` (MassTransit sequential GUID) instead of `Guid.NewGuid()` to avoid PostgreSQL B-Tree index fragmentation.
   - Always propagate `CancellationToken` to every async call (`MediatR`, repositories, validators, HTTP/gRPC clients).
   - Never throw exceptions for expected business/validation/not-found errors — propagate `Result.Failure(...)` / `Result<T>.Failure(...)` and return `Result.Success()` / `Result<TResponse>.Success(...)` explicitly.

3. **Validation (`*Validator`):**
   - Declare as `public sealed class <Name>Validator : AbstractValidator<TRequest>` (name must end with `Validator`).
   - For `IUserBoundRequest`, validate `RuleFor(x => x.UserId).NotEmpty()` (guards against `Guid.Empty` from missing/invalid auth claims).
   - Reuse limits and regexes from `BuildingBlocks.Domain.Constants` instead of duplicating magic numbers.
   - `ValidationBehavior` automatically short-circuits invalid requests into `Result` validation failures — do not invoke validators manually inside handlers.

4. **Behaviors, Caching & Tenant Safety:**
   - Do not manage transactions or request logging manually in handlers; rely on `BuildingBlocks.Application.Behaviors` (`LoggingBehavior` → `ValidationBehavior` → `TransactionBehavior` → `CacheBehavior`).
   - Register any service-specific security/ownership behavior **before** `TransactionBehavior` and `CacheBehavior` so unauthorized requests never hit the cache or open DB transactions.
   - **Cross-tenant cache safety:** for any `ICachedQuery<TResponse>` returning user-owned data, `CacheKey` **must** include `UserId` (or tenant scope).
   - Keep `*.Application` free of `*.Infrastructure` and `*.API` references (enforced by `ServiceLayerArchitectureTestsBase`).
