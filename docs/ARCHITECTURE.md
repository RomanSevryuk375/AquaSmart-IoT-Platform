# ARCHITECTURE

## Topology

- `ApiGateway`: единая точка входа, JWT validation, reverse proxy, агрегированный Swagger UI.
- `IdentityService`: регистрация, логин, refresh/logout, профиль пользователя, выпуск JWT, refresh tokens, Quartz jobs, user events.
- `DeviceService`: контроллеры, датчики, реле, heartbeat, HTTP ingress телеметрии от железа, обработка relay-команд.
- `TelemetryService`: хранение телеметрии, duplicate-check по `ExternalMessageId`, публикация `TelemetryReceivedEvent`, контроль задержки данных датчиков.
- `ControlService`: аквариумы, правила автоматизации, schedule processing, реакция на telemetry/device events.
- `NotificationService`: уведомления, напоминания, maintenance log, sync пользовательских и aquarium-данных из событий.

## Current Runtime (As Is)

### Gateway + Auth

- `ApiGateway` проксирует `identity`, `telemetry`, `device`, `control`, `notification`.
- JWT проверяется на gateway и также зарегистрирован в downstream-сервисах через `AddCommonAuthentication`.
- Маршруты `/api/telemetry/*`, `/api/device/*`, `/api/control/*`, `/api/notification/*` защищены policy `Default`.
- `/api/identity/*` доступен для auth flow; profile/logout требуют авторизацию на controller level.
- `IdentityService` выставляет `AccessToken` и `RefreshToken` cookies; общий `JwtBearer` setup читает `Authorization: Bearer <accessToken>` и fallback `AccessToken` cookie.

Практический вывод для МК:
- gateway path требует JWT на route-level и `X-Device-Token` внутри device-facing handler-а;
- direct lab path уже опубликован через `device-api` на `localhost:5237`;
- первый hardware smoke проще вести напрямую в `device-api`, а gateway path оставить для user-facing clients.

### Identity Flow

Endpoint-ы:
- `POST /api/identity/v1/auth/register`
- `POST /api/identity/v1/auth/login`
- `POST /api/identity/v1/auth/refresh`
- `POST /api/identity/v1/auth/logout`
- `GET /api/identity/v1/profile`
- `PUT /api/identity/v1/profile/me`
- `POST /api/identity/v1/profile/password`

Фактический runtime:
- после регистрации публикуется `UserCreatedEvent`;
- после изменения профиля публикуется `UserUpdatedEvent`;
- access token генерируется через `JwtProvider`;
- refresh token хранится в таблице `refresh_tokens`;
- cleanup и subscription-check выполняются Quartz jobs.

Текущий статус:
- register и refresh response regression исправлены;
- refresh endpoint принимает DTO `RefreshTokenRequestDto`;
- refresh token выдается как `<tokenId>.<secret>`, а в БД хранится `TokenHash`;
- `login/register/refresh` выставляют `AccessToken` и `RefreshToken` cookies с `HttpOnly`; `Secure = false` подходит только для локального HTTP;
- `UserEntity` получил обязательный `TimeZone`, `UserCreatedEvent` переносит `TimeZone`, но request DTO использует legacy typo `TimaZone`, а profile read/update пока не возвращают и не меняют time zone;
- subscription downgrade не публикует downstream event.

### Device + Hardware Ingress Flow

Canonical hardware ingress находится в `DeviceService`.

Endpoint:
- `POST /api/device/v1/sensors/telemetry`
- Header: `X-Device-Token: <controller-device-token>`

Payload:
```json
{
  "macAddress": "AA:BB:CC:DD:EE:FF",
  "items": [
    {
      "sensorId": "00000000-0000-0000-0000-000000000000",
      "value": 25.4,
      "externalMessageId": "controller-001-42",
      "recordedAt": "2026-04-26T09:00:00Z"
    }
  ]
}
```

Flow:
1. `SensorService.ProcessTelemetryBatchAsync` находит controller по `MacAddress`.
2. `X-Device-Token` сверяется с `DeviceTokenHash`.
3. Каждый item сверяется с датчиками контроллера.
4. Для валидных items публикуется `TelemetryReportedFromHardwareEvent`.
5. Response: `202 Accepted` с `acceptedCount`, `skippedCount`, `validationErrors`.

Ограничения:
- пустой `Items` валидируется через `TelemetryBatchRequestValidator`;
- `MacAddress`, `Items`, `SensorId`, `ExternalMessageId` и `RecordedAt` имеют явные validation messages;
- `ExternalMessageId` обязателен и ограничен 100 символами;
- `Guid.Empty` для `SensorId` выделен отдельной validation error;
- `Value` не валидируется через `NotEmpty()`, поэтому `0` не должен отклоняться самим validator-ом;
- `SensorId`, который не принадлежит controller-у, попадает в `skippedCount` и возвращается в `validationErrors`;
- правило максимального размера batch исправлено на лимит до 50 элементов;
- registration action возвращает `DeviceToken` в body, а `CreatedAtRoute` уже передает `{ id = response.ControllerId }`.

Device-facing relay command endpoints:
- `GET /api/device/v1/commands/pending/{controllerId}`
- `POST /api/device/v1/commands/{commandId}/complete`
- `POST /api/device/v1/commands/{commandId}/fail`

Все три endpoint-а принимают `X-Device-Token` и предназначены для direct `device-api` lab path. Через gateway они также попадают под `/api/device/*` route policy и требуют JWT.

Текущий relay-command contract: `GET pending` помечает найденные команды как `Sent`; если МК не успел вызвать `complete/fail`, команда повторно возвращается через 1 минуту при `AttemptCount < 3` и до наступления `ExpireAt`. `Completed` и `Failed` команды повторно не выдаются.

### Telemetry Flow

- Публичный API `TelemetryService` read-only:
  - `GET /api/telemetry/v1/data/raw`
  - `GET /api/telemetry/v1/data/aggregate`
- Ingress идет через RabbitMQ event `TelemetryReportedFromHardwareEvent`.
- Идемпотентность: pre-check по `ExternalMessageId` + unique index.
- После сохранения публикуется `TelemetryReceivedEvent`.
- `CheckSensorStateJob` публикует `SensorNoDataEvent`.
- Raw telemetry хранится в `telemetry_raw_data`.
- Агрегаты хранятся в `telemetry_aggregate_data`.
- Сжатие raw -> minute/hour/day aggregate выполняется Quartz jobs.

Ограничение:
- конкурентный duplicate-case требует явной обработки unique-index violation.
- `TelemetryService` ищет `Ecosystem` по `ControllerId` перед сохранением telemetry; publisher `EcosystemCreatedEvent` в текущем коде не найден.
- `EcosystemService.CreateEcosystemAsync` сейчас не создает missing ecosystem: при `existingEcosystem is null` возвращает `Success` без записи, а при существующей записи пытается создать дубль.
- `IsAggregated` indexes исправлены в code config, migration `20260429103915_AddDataAggregating`, snapshot и примененной runtime-БД; текущий baseline - non-unique indexes.

### Control Flow

- `ControlService` consume-ит telemetry, sensor и relay events.
- API покрывает `aquariums` и `automation-rules`.
- Публикует relay commands и alert events.
- `CriticalTelemetryThresholdAlertEvent` получает `UserId` через `aquarium.UserId`.
- `SensorNoDataAlertEvent` также получает `UserId` через `existingAquarium.UserId`.

Ограничения:
- публичных API для schedule/vacation mode пока нет;
- sensor no-data alert может публиковаться по каждому affected rule.

### Notification Flow

- API:
  - `/api/notification/v1/notifications`
  - `/api/notification/v1/reminders`
  - `/api/notification/v1/maintenance-logs`
- Consumers:
  - `UserCreatedEvent`
  - `UserUpdatedEvent`
  - aquarium lifecycle events
  - `CriticalTelemetryThresholdAlertEvent`
  - `SensorNoDataAlertEvent`
  - `ControllerNotOnlineEvent`
- Providers:
  - Telegram
  - Email

Текущий статус:
- namespace mismatch по aquarium events в исходниках уже исправлен;
- build подтвержден через последовательный `dotnet build ... -m:1`;
- runtime smoke для alert-to-notification еще не выполнен;
- внешние provider-настройки остаются тестовыми.

## Cross-Cutting

### Databases

Отдельная PostgreSQL БД на сервис:
- `identity_db`
- `telemetry_db`
- `device_db`
- `control_db`
- `notification_db`

### Messaging

RabbitMQ + MassTransit используются для:
- user sync;
- aquarium sync;
- sensor sync;
- telemetry ingestion;
- telemetry received;
- relay command flow;
- alerts.

### Security Boundary

- Реальная user-facing trust boundary остается на gateway.
- Downstream-сервисы также регистрируют JWT Bearer через `AddCommonAuthentication`, поэтому direct service calls к protected actions требуют валидный bearer token.
- JWT извлекается из `Authorization: Bearer` или `AccessToken` cookie; Bearer header имеет приоритет для API clients.
- `[AllowAnonymous]` в `DeviceService` работает только после прохождения маршрутизации до сервиса. Gateway route-level policy для `/api/device/*` блокирует эти endpoints без JWT.
- Для direct MCU access к `device-api` нужно отдельно учитывать, что endpoint обходит gateway JWT layer и опирается на `X-Device-Token`.

### Build Boundary

- Корневой `global.json` валиден и выбирает .NET 8 SDK через `8.0.100` + `rollForward: latestFeature`.
- В текущей среде `dotnet --info` выбирает SDK `8.0.420`.
- `dotnet build` подтвержден для всех 5 API-проектов через последовательный режим `-m:1`.
- Параллельный restore/build без `-m:1` может падать без явных compile errors и не считается рабочим режимом сборки.

## Planned Architecture Extensions

### `planned`: CQRS + MediatR

Цель:
- разнести write/read use cases по handler-ам;
- добавить pipeline behaviors для валидации, логирования и idempotency.

Первая зона:
- `ControlService`, затем `TelemetryService`.

### `planned`: SignalR

Цель:
- live dashboard без polling;
- push telemetry/alert/offline events в UI.

Рекомендуемая точка:
- `ApiGateway` или отдельный realtime edge-сервис.

### `planned`: gRPC

Цель:
- быстрые synchronous internal query-сценарии.

Use case-ы:
- проверка лимитов подписки;
- синхронная проверка конфигурации контроллера.

### `planned`: gRPC streaming

Цель:
- долгоживущий канал с эмулятором/контроллером;
- push команд, конфигурации и потенциально OTA.

## Architecture Gaps

1. Direct MCU lab mode задокументирован, но E2E smoke еще не пройден.
2. Auth cookie-flow унифицирован на `AccessToken`, но production cookie/security policy еще нужно вынести в настройки окружения.
3. Gateway-level `/api/device/*` policy не дает MCU пройти к `[AllowAnonymous]` endpoints без JWT, поэтому первый smoke должен идти напрямую в `device-api:5237`.
4. `TelemetryService` зависит от `Ecosystem` projection, но producer `EcosystemCreatedEvent` не найден, а create-handler при missing projection возвращает `Success` без создания записи.
5. Telemetry validation contract для МК требует runtime-подтверждения и фиксации в README/API examples: `ExternalMessageId`, `RecordedAt`, batch size, `value: 0`, item-level диагностика unknown sensors.
6. Telemetry idempotency не закрывает конкурентный дубль явно.
7. Alert-to-notification E2E не подтвержден.
8. `docker compose up --build` не подтвержден.
9. Refresh token flow требует reuse/family revocation policy.

## Current End-to-End Scenario

1. Клиент получает JWT через `IdentityService`.
2. Клиент работает через `ApiGateway`.
3. Создаются controller/sensor/relay сущности в `DeviceService`.
4. Создаются aquarium/rule сущности в `ControlService`.
5. МК отправляет batch телеметрию в `POST /api/device/v1/sensors/telemetry`.
6. `DeviceService` публикует `TelemetryReportedFromHardwareEvent`.
7. `TelemetryService` сохраняет данные и публикует `TelemetryReceivedEvent`.
8. `ControlService` вычисляет правило и отправляет relay-команду.
9. `DeviceService` применяет команду.
10. `NotificationService` должен обработать alert-событие и создать/отправить уведомление.

## MCU Test Readiness

Оценка на 29 апреля 2026: `DeviceService` и `TelemetryService` собираются, direct HTTP ingress доступен по контракту, telemetry validation в коде уже покрывает основные firmware-поля, а migrations применены. Первый MCU smoke еще требует исправления ecosystem projection и runtime-проверки validation/relay diagnostics.

Готово:
- есть HTTP telemetry endpoint;
- есть `X-Device-Token`;
- есть batch payload;
- `device-api` опубликован на `localhost:5237`;
- registration response возвращает `DeviceToken` в body;
- `CreatedAtRoute` onboarding endpoint-а исправлен;
- пустой `Items`, `Guid.Empty` sensor id, пустой/длинный `ExternalMessageId`, future `RecordedAt` и unknown sensor ownership валидируются диагностически;
- есть RabbitMQ flow в сторону telemetry/control;
- исправлена часть старых event/user ownership дефектов.
- `SetRelayStateAsync` уже ставит backend relay command через `AddAsync`.
- relay command polling имеет retry для `Sent` команд: повторная выдача через 1 минуту, максимум 3 попытки, до `ExpireAt`.
- `DeviceService` build подтвержден без предупреждений.

Не готово:
- gateway route требует JWT;
- compose/E2E smoke не подтверждены;
- ecosystem sync для `TelemetryService` не замкнут найденным publisher-ом;
- `EcosystemService.CreateEcosystemAsync` содержит existing/null bug;
- telemetry validation не подтверждена smoke-набором как firmware contract;
- E2E alert flow не проверен.
