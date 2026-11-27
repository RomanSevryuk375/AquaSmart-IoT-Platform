---
trigger: model_decision
description: >-
  Testing rules and conventions for the Presentation / API layer (*.API.E2ETests, BuildingBlocks.UnitTests/Presentation)
  across all AquaAPI microservices: HTTP status codes, auth, RBAC permissions, tenant isolation, validation pipeline,
  and response contracts.
  Apply whenever creating, modifying, or reviewing tests in any *.API.E2ETests project.
---

# Presentation / API Layer Testing Standards (`*.API.E2ETests`)

1. **E2E Test Host & Setup:**
   - Inherit from `BaseE2ETest(E2ETestWebAppFactory factory)` using `[Collection("E2ETestCollection")]` so tests run against the full ASP.NET Core pipeline (`WebApplicationFactory` + PostgreSQL Testcontainer + `Respawn`).
   - Authenticate requests via `TestAuthHandler` + `TestUserContext`, and seed prerequisites directly via `DbContext` + `*.TestShared/Builders` before making HTTP calls.
   - Follow `<MethodOrEndpoint>_<Condition>_<ExpectedOutcome>` naming with `// Arrange`, `// Act`, `// Assert`.

2. **Required HTTP Coverage per Endpoint:**
   - **Authentication (`401`):** create a dedicated unauthenticated client via `Factory.CreateClient()` (do not mutate shared `Client.DefaultRequestHeaders`) and assert `HttpStatusCode.Unauthorized`.
   - **RBAC / Missing Permission (`403`):** test an authenticated user with a valid token lacking the required `SubPermissions.*` claim and assert `HttpStatusCode.Forbidden`.
   - **Tenant Isolation (`403` / `404`):** seed a resource owned by `Guid.NewGuid()` and assert the endpoint returns `HttpStatusCode.Forbidden` or `HttpStatusCode.NotFound`.
   - **Validation Pipeline (`400`):** send invalid payloads and assert `HttpStatusCode.BadRequest`.
   - **Happy Path (`200` / `201` / `204`):** assert the expected status code, deserialize JSON with `ReadFromJsonAsync<TDto>()`, and verify persisted state in `DbContext` with `AsNoTracking()` for mutations.

3. **Shared Presentation Unit Tests:**
   - Unit tests for `BuildingBlocks.Presentation` (`GlobalExceptionHandler`, `ResultExtensions`, `ClaimsPrincipalExtensions`) belong in `BuildingBlocks.UnitTests/Presentation/`.
