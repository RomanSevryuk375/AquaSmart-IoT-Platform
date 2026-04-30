# Repository Guidelines

## Project Structure & Module Organization

AquaAPI is a .NET 8 microservices backend. Source code lives under `src/`.

- `src/Gateway/ApiGateway`: YARP gateway and JWT boundary.
- `src/Contracts/Contracts`: shared contracts, options, events, enums, and middleware.
- `src/Services/*`: service folders for `IdentityService`, `DeviceService`, `TelemetryService`, `ControlService`, and `NotificationService`.
- Each service generally follows `API`, `Application`, `Domain`, and `Infrastructure` projects.
- `docs/` contains architecture notes, review notes, EF migration commands, and runbooks.
- There are currently no test projects; add future tests under a clear `tests/` tree or `*.Tests` projects beside service folders.

## Build, Test, and Development Commands

Run commands from the repository root.

```powershell
dotnet build src/Services/IdentityService/IdentityService/IdentityService.API.csproj
dotnet build src/Services/DeviceService/DeviceService/Device.API.csproj
dotnet build src/Services/TelemetryService/TelemetryService/Telemetry.API.csproj
dotnet build src/Services/ControlService/ControlService/Control.API.csproj
dotnet build src/Services/NotificationService/NotificationService/Notification.API.csproj
```

Builds individual API services. Use `docker compose up --build` to build and run the full local stack with PostgreSQL and RabbitMQ. EF migration commands are documented in `docs/EF_MIGRATIONS_COMMANDS.md`.

If `dotnet` first-time setup fails locally, set:

```powershell
$env:DOTNET_CLI_HOME="C:\Work Space\AquaAPI\.dotnet-home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1"
```

## Coding Style & Naming Conventions

Use C# with nullable reference types and implicit usings enabled. Prefer 4-space indentation, file-scoped namespaces, `Async` suffixes for asynchronous methods, and dependency injection through constructors. Keep service boundaries strict: shared integration contracts belong in `Contracts`, not in service domain projects.

Avoid adding new names with existing legacy typos such as `Infrastrucrute` or `Registred`; preserve legacy names only when changing them would require a coordinated migration.

## Testing Guidelines

No automated test suite is present yet. For new tests, prefer xUnit and name projects like `DeviceService.Tests` or `ControlService.Tests`. Cover service-level business logic, event consumers, and critical auth/telemetry flows. Until tests exist, verify changes with targeted `dotnet build` plus a local smoke through `docker compose`.

## Commit & Pull Request Guidelines

Git history uses Conventional Commits, often scoped:

- `feat(identity): implement refresh token`
- `feat: move abstractions to a common layer`

Use `feat`, `fix`, `docs`, `refactor`, `test`, or `chore`, with a scope when helpful. PRs should include a short summary, affected services, migration notes, configuration changes, and verification commands. Link issues when available and include request/response examples for API contract changes.

## Security & Configuration Tips

Do not commit secrets, real device tokens, Telegram tokens, email credentials, or local database dumps. Keep local-only artifacts such as `.vs`, `.idea`, `.dotnet*`, logs, `bin`, and `obj` out of commits.
