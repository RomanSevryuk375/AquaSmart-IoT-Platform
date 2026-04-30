# PROJECT_ANALYSIS

## Purpose

Документ фиксирует фактическое состояние `AquaAPI` по коду и локальной сборке на 29 апреля 2026 года и обновляет выводы после прошлого ревью.

## Current State

Репозиторий содержит backend-контур из 5 сервисов и gateway:
- `ApiGateway` с YARP, JWT validation и агрегированным Swagger;
- `IdentityService` с регистрацией, логином, refresh/logout flow, профилем пользователя и Quartz jobs;
- `DeviceService` с CRUD по контроллерам, датчикам и реле, `X-Device-Token`, ping и HTTP telemetry ingress для МК;
- `TelemetryService` с хранением телеметрии, duplicate-check по `ExternalMessageId`, публикацией `TelemetryReceivedEvent` и job проверки отсутствия данных;
- `ControlService` с CRUD по аквариумам и правилам автоматизации, обработкой telemetry/sensor/relay events и schedule job;
- `NotificationService` с отдельной БД, API для уведомлений/напоминаний/журнала обслуживания и consumer-ами user/aquarium/alert events.

Главное изменение после прошлого ревью:
- исправлены runtime-дефекты `IdentityService` в регистрации и refresh response;
- `global.json` перенесен в корень репозитория и закрепляет .NET SDK `8.0.100` с `rollForward: latestFeature`;
- `docker-compose.yml` публикует `device-api` наружу на `localhost:5237`, поэтому появился direct lab mode для МК/эмулятора;
- `RabbitMqOptions__*` приведены в compose к фактическим options-классам сервисов;
- refresh endpoint переведен на DTO (`RefreshTokenRequestDto`);
- refresh token больше не хранится raw string: наружу выдается формат `<tokenId>.<secret>`, а в БД сохраняется `TokenHash`;
- добавлена обязательная `TimeZone` у пользователя, миграция `20260429182407_AddTimeZoneForUser` и поле `TimeZone` в `UserCreatedEvent`;
- `SensorNoDataAlertEvent` теперь получает `UserId` через `Aquarium.UserId`, а не через `HttpContext`;
- namespace aquarium events в `NotificationService` приведен к `Contracts.Events.AquariumEvents`;
- controller registration теперь возвращает onboarding DTO с `ControllerId` и `DeviceToken`;
- `CreatedAtRoute` в controller registration исправлен на `{ id = response.ControllerId }`;
- direct MCU lab mode и telemetry payload/response задокументированы в `README.md`;
- telemetry ingress получил `FluentValidation`-валидаторы для `MacAddress`, пустого `Items`, `SensorId == Guid.Empty`, `ExternalMessageId` и `RecordedAt`;
- telemetry validation contract стал ближе к firmware-ready: заданы явные сообщения для `MacAddress`, `Items`, `SensorId`, `ExternalMessageId`, `RecordedAt`, добавлен лимит `ExternalMessageId <= 100`, а `Value` больше не отклоняет `0` на уровне validator-а;
- telemetry batch-size validator исправлен на ожидаемый лимит `1..50`;
- в `DeviceService` добавлен HTTP polling-контур relay-команд для МК через `X-Device-Token`;
- downstream-сервисы получили общий JWT Bearer setup через `AddCommonAuthentication`;
- `DeviceService` снова собирается после добавления `using Contracts.Results` в `IRelayCommandQueueService`;
- `SetRelayStateAsync` уже использует `AddAsync` для новой relay command;
- `TelemetryService` получил raw/aggregate endpoints и jobs сжатия данных;
- unique-index regression по `IsAggregated` исправлен в конфигурациях, `SystemDbContextModelSnapshot` и миграции `20260429103915_AddDataAggregating`; миграции применены, runtime-схема считается обновленной на non-unique `IsAggregated` indexes.

## Services

### ApiGateway

Реализовано:
- reverse proxy маршруты на `identity`, `telemetry`, `device`, `control`, `notification`;
- JWT Bearer validation;
- policy `Default` для `telemetry`, `device`, `control`, `notification`;
- агрегированный Swagger UI.

Ограничения:
- gateway остается фактической trust boundary;
- telemetry ingress через gateway защищен JWT policy, поэтому физический МК должен либо уметь отправлять JWT, либо ходить напрямую в `device-api`;
- direct lab mode теперь доступен через опубликованный `device-api` port `5237`.

### IdentityService

Реализовано:
- `POST /api/identity/v1/auth/register`;
- `POST /api/identity/v1/auth/login`;
- `POST /api/identity/v1/auth/refresh`;
- `POST /api/identity/v1/auth/logout`;
- `GET /api/identity/v1/profile`;
- `PUT /api/identity/v1/profile/me`;
- `POST /api/identity/v1/profile/password`;
- refresh token entity/repository;
- публикация `UserCreatedEvent` и `UserUpdatedEvent`;
- Quartz jobs для refresh-token cleanup и проверки истекших подписок.

Исправлено после прошлого ревью:
- регистрация больше не обращается к `existingUser!.SubscriptionId`, а берет `user.SubscriptionId`;
- refresh flow возвращает новый refresh token (`newRefreshToken`).

Ограничения:
- refresh token hashing внедрен, а reuse detection удаляет все refresh tokens пользователя при повторном использовании `IsUsed` token-а; это уже лучше прежнего состояния, но еще нет явной session-family модели, audit trail и привязки к устройству/клиенту;
- `Secure = false` у auth cookie приемлем только для локального HTTP-стенда;
- `login/register/refresh` выставляют реальные `AccessToken` и `RefreshToken` cookies; Bearer header остается основным API-контрактом, cookie fallback работает через общий `JwtBearer` setup;
- downgrade просроченной подписки выполняется локально, без downstream event;
- `RegisterUserRequestDto` содержит legacy typo `TimaZone`, а profile read/update DTO пока не возвращают и не меняют `TimeZone`;
- JWT validation в самом сервисе настроен через `AddCommonAuthentication`, но user-facing путь все равно должен идти через gateway как основную trust boundary.

### DeviceService

Реализовано:
- CRUD/service-операции для `controllers`, `sensors`, `relays`;
- `POST /api/device/v1/controllers/{id}/ping`;
- публичный endpoint `POST /api/device/v1/sensors/telemetry`;
- batch ingress по `MacAddress + X-Device-Token + Items[]`;
- response telemetry ingress: `acceptedCount`, `skippedCount`, `validationErrors`;
- consumer команд смены состояния реле;
- job проверки offline-контроллеров с публикацией `ControllerNotOnlineEvent`.

Ограничения:
- telemetry batch-size validator принимает до 50 элементов;
- telemetry item имеет явные validation messages для `SensorId`, `ExternalMessageId`, `RecordedAt`; `ExternalMessageId` ограничен 100 символами;
- `Guid.Empty` для `SensorId` выделен отдельной validation error;
- `Value` сейчас не валидируется через `NotEmpty()`, поэтому `0` не должен отклоняться самим `TelemetryItemRequestValidator`;
- неизвестный `SensorId`, который не принадлежит контроллеру, попадает в `skippedCount` и добавляется в `validationErrors`;
- `PingControllerAsync` не публикует событие "controller online", только обновляет `LastSeenAt`;
- `RelayCommandsController` содержит device-facing endpoints для получения pending-команд и acknowledgement через `X-Device-Token`;
- polling pending-команд переводит команды в `Sent`; зависшие `Sent` команды переотдаются через 1 минуту, максимум до 3 попыток и только до `ExpireAt`;
- `RelayCommandQueueService.SetRelayStateAsync` уже создает новую command entity через `AddAsync`;
- `DeviceService` build подтвержден: `dotnet build src\Services\DeviceService\DeviceService\Device.API.csproj -m:1 -v:minimal /p:MSBuildEnableWorkloadResolver=false`.

### TelemetryService

Реализовано:
- `GET /api/telemetry/v1/data/raw`;
- `GET /api/telemetry/v1/data/aggregate`;
- consumer `TelemetryReportedFromHardwareConsumer`;
- duplicate-check через `ExternalMessageId`;
- unique index на `ExternalMessageId`;
- публикация `TelemetryReceivedEvent`;
- sync локального каталога сенсоров через sensor events;
- `CheckSensorStateJob`, публикующий `SensorNoDataEvent`;
- aggregation entities/repositories/services;
- Quartz jobs `CompressRawDataToMinutesJob`, `CompressRawDataToHoursJob`, `CompressRawDataToDaysJob`, `CleanUpOldDataJob`.

Ограничения:
- публичного `POST /api/telemetry/v1/data` нет, canonical ingress идет через `DeviceService`;
- конкурентный duplicate-case явно не обработан на уровне db-exception/neutral contract;
- `TelemetryAggregateEntityConfiguration`, `TelemetryRawEntityConfiguration`, migration `20260429103915_AddDataAggregating`, `SystemDbContextModelSnapshot` и runtime-БД содержат non-unique indexes по `IsAggregated`;
- `TelemetryService` требует существующий `Ecosystem` по `ControllerId`, но publisher `EcosystemCreatedEvent` в текущем коде не найден;
- `EcosystemService.CreateEcosystemAsync` содержит инвертированную проверку: при `existingEcosystem is null` возвращает `Success` без создания projection, а при существующей записи пытается создать дубль;
- JWT validation в сервисе настроен через `AddCommonAuthentication`.

### ControlService

Реализовано:
- `GET/POST/PUT/DELETE /api/control/v1/aquariums`;
- `GET/POST/PUT/DELETE /api/control/v1/automation-rules`;
- consumer-ы telemetry, sensor и relay events;
- публикация `CriticalTelemetryThresholdAlertEvent` и `SensorNoDataAlertEvent`;
- публикация aquarium events;
- `ScheduleProcessJob`.

Исправлено после прошлого ревью:
- `SensorNoDataAlertEvent` теперь заполняет `UserId` через `existingAquarium.UserId`.

Ограничения:
- публичных API для расписаний и vacation mode пока нет;
- `SensorNoDataAlertEvent` публикуется внутри цикла по affected rules, поэтому один sensor-no-data может породить несколько alert events;
- JWT validation в сервисе настроен через `AddCommonAuthentication`.

### NotificationService

Реализовано:
- API для notifications/reminders/maintenance logs;
- отдельная PostgreSQL БД и EF migrations;
- consumer-ы `UserCreatedEvent`, `UserUpdatedEvent`, aquarium events и alert events;
- providers для Telegram/email;
- Quartz jobs `ReminderCheckerJob` и `UnpublishedNoticeProcessorJob`.

Исправлено после прошлого ревью:
- imports aquarium events приведены к `Contracts.Events.AquariumEvents`.

Ограничения:
- сборка сервиса подтверждена через последовательный `dotnet build ... -m:1`;
- обработка уведомлений зависит от тестовых placeholder-настроек Telegram/email;
- JWT validation в сервисе настроен через `AddCommonAuthentication`.

## Cross-Cutting Findings

### Runtime Topology

`docker-compose.yml` поднимает:
- `api-gateway`, `identity-api`, `telemetry-api`, `device-api`, `control-api`, `notification-api`;
- `rabbitmq`;
- `identity-db`, `telemetry-db`, `device-db`, `control-db`, `notification-db`.

Наружу опубликованы gateway (`5055`), `device-api` (`5237`), RabbitMQ и базы. Это закрывает базовый direct HTTP path для МК/эмулятора без JWT, если используется `X-Device-Token`.

### Messaging

Event-driven цепочка архитектурно покрывает:
- user creation/update;
- aquarium lifecycle;
- sensor lifecycle;
- hardware telemetry ingestion;
- telemetry received;
- relay command flow;
- alert dispatch into notification context.

Главные оставшиеся риски:
- нет подтвержденного E2E runtime-smoke;
- нет события о subscription downgrade;
- idempotency telemetry не закрывает конкурентный дубль явно;
- alert-to-notification требует runtime-проверки `NotificationService` в E2E smoke.

### Build/Tooling

Факты на 29 апреля 2026:
- валидный `global.json` теперь лежит в корне и выбирает .NET SDK `8.0.420` через roll-forward от `8.0.100`;
- старый `src/global.json` удален;
- запуск `dotnet` без writable `DOTNET_CLI_HOME` может падать на first-time setup из-за прав;
- воспроизводимая локальная сборка подтверждена для всех 5 API-проектов через последовательный build с `-m:1`;
- `IdentityService` собирается с одним nullable warning в `SubscriptionEntity.Name`;
- параллельный restore/build без `-m:1` может падать без явных compile errors и остается tooling follow-up;
- `docker compose up --build` не подтвержден.
- targeted build `Device.API.csproj` на 29 апреля 2026 подтвержден: сборка успешна без предупреждений;
- targeted build `Telemetry.API.csproj` на 29 апреля 2026 подтвержден: сборка успешна без предупреждений.

### Repository Hygiene

В репозитории присутствуют IDE/build артефакты и временные файлы:
- `.vs`, `.idea`;
- `build.log`, `infra-build*.log`, `restore-diag.log`;
- `*.Backup.tmp`;
- локальные `.dotnet*` директории.

## Main Risks

1. Direct MCU lab mode задокументирован, но не подтвержден E2E smoke-ом.
2. `TelemetryService` требует `Ecosystem` по `ControllerId`, producer `EcosystemCreatedEvent` не найден, а `EcosystemService.CreateEcosystemAsync` сейчас при missing projection возвращает `Success` без создания записи.
3. Gateway route-level policy защищает весь `/api/device/*`, поэтому `[AllowAnonymous]` device endpoints доступны для МК только через direct `device-api:5237`, если МК не отправляет JWT.
4. Telemetry validation стала ближе к firmware-контракту, но еще требует runtime-smoke и документированного формата ошибок: проверить `ExternalMessageId`, `RecordedAt`, batch `0/1/50/51`, unknown sensors и `value: 0`.
5. Auth cookie-flow исправлен на `AccessToken` fallback, но production cookie/security policy еще нужно вынести в настройки окружения.
6. Refresh token hashing внедрен, но reuse/family revocation policy еще не production-grade.
7. Notification/alert chain не подтверждена фактическим E2E smoke.
8. `docker compose up --build` еще не подтвержден.
9. Репозиторий загрязнен временными и IDE/build артефактами.

## Summary

Проект стал ближе к первому hardware smoke: исправлен SDK pinning, опубликован direct `device-api`, telemetry ingress существует, batch-size validator приведен к лимиту `1..50`, device token возвращается в response body, refresh token flow стал безопаснее, alert `UserId` в sensor-no-data ветке исправлен, `DeviceService` снова собирается, а relay command enqueue path уже использует `AddAsync`.

Текущий статус: `backend HTTP ingress уже компилируется`, миграции применены, но `первый MCU smoke еще блокируют ecosystem projection и отсутствие E2E smoke`.

Первый следующий фокус:
- исправить `EcosystemService.CreateEcosystemAsync` и замкнуть publisher/consumer creation path для `TelemetryService`;
- подтвердить telemetry validation contract для МК smoke-набором (`ExternalMessageId`, `RecordedAt`, batch size, `value: 0`, unknown sensors diagnostics);
- пройти `docker compose up --build` и E2E smoke;
- сохранить текущий гибридный auth contract: Bearer для API clients, `AccessToken` cookie fallback для browser clients.
