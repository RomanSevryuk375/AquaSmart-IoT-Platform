# AquaAPI

AquaAPI is a .NET 8 microservices backend for smart aquarium and terrarium automation. It is a portfolio project focused on service boundaries, event-driven integration, JWT-based gateway auth, PostgreSQL per service, RabbitMQ, Quartz jobs, and Docker-based local runtime.

## Architecture

Current services:

- `ApiGateway`: YARP reverse proxy, JWT validation boundary, aggregated Swagger.
- `IdentityService`: registration, login, refresh/logout, profile, subscriptions, user events.
- `DeviceService`: controllers, sensors, relays, device token validation, hardware telemetry ingress.
- `TelemetryService`: telemetry history, idempotency by `ExternalMessageId`, telemetry events, no-data checks.
- `ControlService`: aquariums, automation rules, schedule processing, relay commands, alert events.
- `NotificationService`: notifications, reminders, maintenance logs, user/aquarium/alert consumers.

Shared contracts live in `src/Contracts/Contracts`.

## High-Level Flow

1. User authenticates through `IdentityService`.
2. User-facing clients call downstream APIs through `ApiGateway`.
3. Devices, sensors, and relays are managed in `DeviceService`.
4. Aquariums and automation rules are managed in `ControlService`.
5. Hardware sends telemetry to `DeviceService`.
6. `DeviceService` publishes `TelemetryReportedFromHardwareEvent`.
7. `TelemetryService` stores data and publishes `TelemetryReceivedEvent`.
8. `ControlService` evaluates rules and publishes relay commands.
9. `DeviceService` applies relay state changes.
10. Alert events are consumed by `NotificationService`.

## Repository Layout

```text
src/
  Contracts/
  Gateway/ApiGateway/
  Services/
    IdentityService/
    DeviceService/
    TelemetryService/
    ControlService/
    NotificationService/
docs/
docker-compose.yml
global.json
```

## Build Verification

Run commands from the repository root. The stable local build mode is sequential MSBuild with `-m:1`.

```powershell
$env:DOTNET_CLI_HOME="C:\Work Space\AquaAPI\.dotnet-home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1"

dotnet build src\Services\IdentityService\IdentityService\IdentityService.API.csproj -m:1 -v:minimal /p:MSBuildEnableWorkloadResolver=false
dotnet build src\Services\DeviceService\DeviceService\Device.API.csproj -m:1 -v:minimal /p:MSBuildEnableWorkloadResolver=false
dotnet build src\Services\TelemetryService\TelemetryService\Telemetry.API.csproj -m:1 -v:minimal /p:MSBuildEnableWorkloadResolver=false
dotnet build src\Services\ControlService\ControlService\Control.API.csproj -m:1 -v:minimal /p:MSBuildEnableWorkloadResolver=false
dotnet build src\Services\NotificationService\NotificationService\Notification.API.csproj -m:1 -v:minimal /p:MSBuildEnableWorkloadResolver=false
```

All five API projects are expected to build this way. Parallel restore/build without `-m:1` can fail in the current local environment and is not the documented build path.

## Local Run

```bash
docker compose up --build
```

Main exposed ports:

- `5055` -> `ApiGateway`
- `5237` -> `DeviceService` direct lab mode
- `5438` -> telemetry database
- `5439` -> device database
- `5440` -> control database
- `5441` -> identity database
- `5442` -> notification database
- `5672` -> RabbitMQ
- `15672` -> RabbitMQ Management UI

`docker compose up --build` is still part of the E2E verification phase and has not been treated as the completed smoke proof yet.

## API Examples

Authentication:

```http
POST /api/identity/v1/auth/register
POST /api/identity/v1/auth/login
POST /api/identity/v1/auth/refresh
POST /api/identity/v1/auth/logout
```

Refresh token format:

```text
<tokenId>.<secret>
```

The raw refresh token is returned to the client. The database stores `TokenHash`, not the raw token. Current reuse/family revocation policy is still a security follow-up.

Controller onboarding:

```http
POST /api/device/v1/controllers
```

Expected response body:

```json
{
  "controllerId": "00000000-0000-0000-0000-000000000000",
  "deviceToken": "<secret-device-token>"
}
```

Known gap: `CreatedAtRoute` still needs to use `{ id = response.ControllerId }` so the `Location` header is correct.

## MCU / Emulator Telemetry

Use direct lab mode for the first hardware smoke:

```http
POST http://localhost:5237/api/device/v1/sensors/telemetry
Content-Type: application/json
X-Device-Token: <controller-device-token>
```

Payload:

```json
{
  "macAddress": "AA:BB:CC:DD:EE:FF",
  "items": [
    {
      "sensorId": "11111111-2222-3333-4444-555555555555",
      "value": 25.4,
      "externalMessageId": "esp32-1744621200-001:temp1",
      "recordedAt": "2026-04-26T10:00:00Z"
    }
  ]
}
```

Response:

```json
{
  "acceptedCount": 1,
  "skippedCount": 0,
  "validationErrors": []
}
```

Gateway mode remains the protected client path: call gateway port `5055` with JWT plus `X-Device-Token`. Direct `device-api:5237` is the lab path for MCU/emulator work.

Known validation gaps:

- empty `items` is not explicitly rejected yet;
- `Guid.Empty` `sensorId` should become a validation error.

## Smoke Flow

Target E2E scenario:

1. `register/login` in `IdentityService`.
2. Create controller in `DeviceService` and save `deviceToken`.
3. Create sensor and relay.
4. Create aquarium and automation rule in `ControlService`.
5. Submit telemetry batch through `http://localhost:5237/api/device/v1/sensors/telemetry`.
6. Verify telemetry is stored by `TelemetryService`.
7. Verify `ControlService` reacts and emits relay command.
8. Verify `DeviceService` applies relay state.
9. Verify alert notification record when an alert scenario is triggered.

This full runtime smoke is not yet confirmed.

## Documentation

Additional project notes live in `docs/`:

- `ARCHITECTURE.md`
- `PROJECT_ANALYSIS.md`
- `IMPLEMENTATION_PLAN.md`
- `PRIORITY_FILES_TO_FIX.md`
- `EF_MIGRATIONS_COMMANDS.md`
- `TZ_microservices_aquarium.md`

## Planned Evolution

The following are roadmap items, not implemented runtime features:

- `CQRS + MediatR`
- `SignalR` live dashboard
- `gRPC` internal synchronous queries
- `gRPC streaming` for emulator/controller channels

## Current Known Gaps

- `CreatedAtRoute` in controller onboarding has an incorrect route value name.
- Telemetry ingress validation still needs hardening for empty `items` and `Guid.Empty` sensor ids.
- `docker compose up --build` and E2E smoke still need confirmation.
- Alert-to-notification runtime path is not proven by smoke test.
- Refresh token reuse/family revocation policy is not production-grade yet.
- Production cookie settings need HTTPS-safe `Secure = true`.
