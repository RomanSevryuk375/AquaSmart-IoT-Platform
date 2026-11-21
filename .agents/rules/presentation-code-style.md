---
trigger: model_decision
description: >-
  Code style and design rules for the Presentation / API layer (*.API, BuildingBlocks.Presentation,
  ApiGateway, TelegramBotService) across all AquaAPI microservices: Minimal API IEndpoint classes,
  Result mapping, IDOR/authorization safety, webhook verification, and exception handling.
  Apply whenever creating or modifying files in any API, Gateway, or Presentation project.
---

# Presentation / API Layer Code Style (`*.API`, `BuildingBlocks.Presentation`)

1. **Minimal API Endpoints (`Endpoints/<Feature>/`):**
   - Implement `IEndpoint` from `BuildingBlocks.Presentation.Endpoints` as `public sealed class <Action>Endpoint : IEndpoint`. Do not create new MVC controllers (`ToActionResult()` exists for legacy compatibility only).
   - Register routes in `MapEndpoint(IEndpointRouteBuilder app)` using constants from `BuildingBlocks.Presentation.Constants.ApiConstants.Routes`.
   - Keep endpoints thin: bind parameters + `ISender` + `IUserContext` + `CancellationToken`, construct the MediatR command/query, call `await sender.Send(request, cancellationToken)`, and return `result.ToIResult()`.
   - Never inject `DbContext` or repositories directly into endpoints.

2. **Security, Authorization & OpenAPI Metadata:**
   - **Prevent IDOR:** always populate `UserId` strictly from `IUserContext.UserId` (or `ClaimsPrincipalExtensions.GetUserId()`), never from request body, route, or query DTOs.
   - Enforce authorization via `.RequireAuthorization(SubPermissions.<Permission>)`.
   - Use `.AllowAnonymous()` only for public auth/health/webhook endpoints. Every anonymous webhook endpoint (e.g., payment callbacks) **must** verify the provider's cryptographic signature (HMAC / webhook secret) and protect against replay attacks before processing.
   - Annotate every endpoint with `.WithTags(...)` and `.Produces<T>(StatusCodes.Status...)` / `.Produces(StatusCodes.Status...)` for expected status codes (`400`, `401`, `403`, `404`, `409`).

3. **Error Handling:**
   - Do not wrap endpoint handlers in `try/catch` for standard exceptions; `GlobalExceptionHandler` in `BuildingBlocks.Presentation` maps `ConcurrencyException`, `DbUpdateException`, `KeyNotFoundException`, `ArgumentException`, and `UnauthorizedAccessException` to RFC 7807 `ProblemDetails` without leaking internal exception details.
