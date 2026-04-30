# PRIORITY_FILES_TO_FIX

## Tier 1: Immediate Blockers

### 1. Ecosystem Projection For TelemetryService

Файлы:
- `src/Contracts/Contracts/Events/EcosystemEvents/EcosystemCreatedEvent.cs`
- `src/Services/TelemetryService/Telemetry.Infrastructure/Messaging/EcosystemConsumers/*`
- `src/Services/TelemetryService/Telemetry.Application/Services/EcosystemService.cs`
- `src/Services/TelemetryService/Telemetry.Application/Services/TelemetryDataService.cs`
- `src/Services/TelemetryService/Telemetry.Application/Services/SensorService.cs`
- `src/Services/ControlService/ControlService.Application/Services/AquariumService.cs`

Что сделать:
- выбрать источник truth для telemetry ecosystem projection;
- исправить `EcosystemService.CreateEcosystemAsync`: при `existingEcosystem is null` нужно создавать projection, а duplicate считать при `is not null`; текущий код при `null` возвращает `Success` без записи, а при существующей записи пытается создать дубль;
- либо публиковать `EcosystemCreatedEvent` при создании aquarium/controller binding;
- либо перевести `TelemetryService` на consume aquarium events, если aquarium является ecosystem;
- пройти smoke: create aquarium -> create sensor -> telemetry batch -> telemetry saved.

Почему это blocker:
- `TelemetryService` возвращает retryable error, если не нашел `Ecosystem` по `ControllerId`;
- sensor projection тоже зависит от существующего ecosystem;
- даже при добавлении publisher-а текущий `CreateEcosystemAsync` не создаст запись из-за инвертированной проверки.

### 2. Telemetry Validation Contract Runtime Smoke For MCU

Файлы:
- `src/Services/DeviceService/Device.Application/DTOs/Validators/TelemtryValidators/TelemetryItemRequestValidator.cs`
- `src/Services/DeviceService/Device.Application/DTOs/Validators/TelemtryValidators/TelemetryBatchRequestValidator.cs`
- `src/Services/DeviceService/Device.Application/Services/TelemetryBatchService.cs`
- `README.md`
- `docs/ESP32_FIRMWARE_TZ_AND_IMPLEMENTATION_PLAN.md`

Что сделать:
- подтвердить фактические messages для `MacAddress`, `Items`, `SensorId`, `ExternalMessageId`, `RecordedAt`, batch size;
- подтвердить, что `value: 0` проходит validator, потому что `Value.NotEmpty()` в текущем `TelemetryItemRequestValidator` отсутствует;
- подтвердить item-level диагностику для sensor ids, которые не найдены у controller-а;
- smoke cases: `externalMessageId: ""`, `value: 0`, future `recordedAt`, unknown `sensorId`, batch `0/1/50/51`.

Почему это blocker:
- прошивка должна отображать `E006` и логировать причину по стабильному списку backend-полей/messages;
- нулевые показания должны быть подтверждены smoke-ом, потому что они валидны для части датчиков;
- item-level диагностика unknown sensor ids уже добавлена в `validationErrors`, но ее нужно проверить на running stack.

### 3. E2E Smoke and Runtime Chain Verification

Файлы:
- `docker-compose.yml`
- `src/Services/TelemetryService/Telemetry.Infrastructure/Messaging/TelemetryReportedFromHardwareConsumer.cs`
- `src/Services/ControlService/ControlService.Application/Services/TelemetryServiceFromEvent.cs`
- `src/Services/NotificationService/Notification.Infrastructure/Messaging/*`

Что сделать:
- поднять `docker compose up --build`;
- пройти сценарий register/login -> create controller/sensor/relay -> telemetry -> telemetry saved -> control reaction -> notification record;
- задокументировать обнаруженные runtime gaps после smoke.

Почему это высокий приоритет:
- без runtime smoke готовность к МК пока подтверждена только inspection-ом, а не рабочим стендом.

### 4. Auth Contract and Gateway/Device Path

Файлы:
- `src/Contracts/Contracts/Authorization/Extensions.cs`
- `src/Gateway/ApiGateway/ApiAuthentication.cs`
- `src/Gateway/ApiGateway/appsettings.json`
- `src/Services/IdentityService/IdentityService/Controllers/AuthController.cs`
- `docs/ARCHITECTURE.md`

Статус:
- API auth contract задокументирован: `Authorization: Bearer <accessToken>` для API clients и `AccessToken` cookie fallback для browser clients;
- первый MCU smoke должен идти через direct `device-api:5237`, потому что gateway route-level `/api/device/*` policy требует JWT до попадания в `[AllowAnonymous]`.

Почему это blocker:
- МК с одним `X-Device-Token` не пройдет через gateway.

## Tier 2: Reliability and Security

### 5. Controller Registration Response Contract

Файлы:
- `src/Services/DeviceService/DeviceService/Controllers/ControllersController.cs`
- `src/Services/DeviceService/Device.Application/DTOs/Controller/ControllerRegistredResponseDto.cs`
- `README.md`
- `docs/ARCHITECTURE.md`

Статус:
- исправлено: `CreatedAtRoute` использует `{ id = response.ControllerId }`;
- body содержит `ControllerId` и `DeviceToken`.

Оставить в наблюдении:
- проверить `Location` в E2E smoke.

### 6. Refresh Token Security Hardening Follow-Up

Файлы:
- `src/Services/IdentityService/IdentityService.Domain/Entities/RefreshTokenEntity.cs`
- `src/Services/IdentityService/IdentityService.Infrastructure/Repositories/RefreshTokenRepository.cs`
- `src/Services/IdentityService/IdentityService/Controllers/AuthController.cs`
- `src/Services/IdentityService/IdentityService.Application/DTOs/*`

Что сделать:
- определить reuse/family revocation policy;
- проверить длину и индексы `TokenHash`;
- решить, какие cookie settings нужны для production HTTPS.

Статус:
- runtime-баги регистрации и refresh response уже исправлены;
- raw storage и raw string request уже заменены на hash + DTO;
- формат refresh token-а `<tokenId>.<secret>` задокументирован;
- production-grade session security еще не достигнут.

### 7. Telemetry Idempotency Under Concurrency

Файлы:
- `src/Services/TelemetryService/Telemetry.Application/Services/TelemetryDataService.cs`
- `src/Services/TelemetryService/Telemetry.Infrastructure/Repositories/TelemetryRawDataRepository.cs`
- `src/Services/TelemetryService/Telemetry.Infrastructure/Configurations/TelemetryRawEntityConfiguration.cs`

Что сделать:
- оставить pre-check по `ExternalMessageId`;
- добавить явную обработку unique-index violation как neutral duplicate result;
- описать expected behavior при повторной доставке одного hardware message.

### 8. Alert Chain Runtime Verification

Файлы:
- `src/Services/ControlService/ControlService.Application/Services/TelemetryServiceFromEvent.cs`
- `src/Services/ControlService/ControlService.Application/Services/SensorServiceFromEvent.cs`
- `src/Services/NotificationService/Notification.Infrastructure/Messaging/*`
- `src/Services/NotificationService/Notification.Application/Services/*AlertSender.cs`

Что сделать:
- подтвердить `CriticalTelemetryThresholdAlertEvent -> notification`;
- подтвердить `SensorNoDataEvent -> SensorNoDataAlertEvent -> notification`;
- проверить, не создается ли избыточный дубль alert-ов при нескольких affected rules.

Статус:
- `UserId` в sensor-no-data ветке уже исправлен через `existingAquarium.UserId`;
- runtime-smoke еще не пройден.

### 9. Subscription Downgrade Event Contract

Файлы:
- `src/Services/IdentityService/IdentityService.Application/Services/SubscriptionExpiredChecker.cs`
- `src/Contracts/Contracts/Events/*`

Что сделать:
- определить event о downgrade/изменении подписки;
- решить, какие сервисы должны реагировать;
- сформулировать продуктовый эффект downgrade.

## Tier 3: Engineering Hygiene

### 10. Build/SDK Tooling Follow-Up

Файлы:
- `global.json`
- `src/Directory.Build.props`
- сервисные `.csproj`/`.slnx`
- `README.md`
- `docs/IMPLEMENTATION_PLAN.md`

Что сделать:
- сохранить корневой `global.json` с валидным SDK feature band;
- использовать подтвержденную команду `dotnet build <API.csproj> -m:1 -v:minimal /p:MSBuildEnableWorkloadResolver=false`;
- отдельно расследовать параллельный restore/build без `-m:1`, который может падать без явных compile errors;
- подтвердить `docker compose up --build`.

Статус:
- `dotnet build` подтвержден для всех 5 API-проектов через `-m:1`;
- `Device.API.csproj` на 29 апреля 2026 собирается успешно без предупреждений;
- `Telemetry.API.csproj` на 29 апреля 2026 собирается успешно без предупреждений;
- `IdentityService` имеет один nullable warning по `SubscriptionEntity.Name`.

### 11. Repository Cleanliness

Файлы/артефакты:
- `.gitignore`
- `.vs`
- `.idea`
- `.dotnet`
- `.dotnet-home`
- `build.log`
- `infra-build*.log`
- `restore-diag.log`
- `src/**/*.Backup.tmp`

Что сделать:
- исключить IDE/build/temp артефакты из отслеживания;
- оставить только воспроизводимые исходники, миграции и документацию;
- не коммитить локальные SDK/home директории.

### 12. Naming and Contract Cleanup

Файлы и артефакты:
- `AquariumCreatedEvend`
- `AquarimUdatedEvent`
- `AquriumId`
- `Device.Infrastrucrute`
- `ControllerRegistredResponseDto`

Что сделать:
- решить, что считается legacy-contract и что можно переименовать сейчас;
- если переименовывать события, делать это синхронно во всех producers/consumers;
- не добавлять новые сущности с опечатками.

## Done Since Previous Review

- Исправлен register bug в `AuthService`.
- Исправлен refresh response: возвращается новый refresh token.
- Добавлен `RefreshTokenRequestDto`.
- Refresh token storage переведен на `TokenHash`.
- Корневой `global.json` исправлен на валидный SDK feature band.
- `device-api` опубликован в compose на `5237`.
- Direct MCU lab mode и telemetry payload/response описаны в `README.md`.
- `ExternalMessageId` валидируется на null/empty/whitespace.
- `ExternalMessageId` имеет явное сообщение и ограничение длины `<= 100`.
- `NotificationService` imports переведены на `Contracts.Events.AquariumEvents`.
- `SensorNoDataAlertEvent.UserId` больше не берется из `HttpContext`-dependent `IUserContext`.
- `ControllersController` теперь возвращает onboarding DTO с `DeviceToken` в body.
- `CreatedAtRoute` в `ControllersController` исправлен на `{ id = response.ControllerId }`.
- Добавлены `FluentValidation` validators для telemetry batch/items.
- Пустой `Items` и `Guid.Empty` sensor id теперь валидируются.
- Unknown/unowned sensor ids добавляются в `validationErrors`, а не только в `skippedCount`.
- Batch-size validator принимает до 50 telemetry items.
- Добавлены device-facing relay command polling/ack endpoints.
- Downstream-сервисы получили общий JWT Bearer setup через `AddCommonAuthentication`.
- `IRelayCommandQueueService` импортирует `Contracts.Results`, и `DeviceService` снова собирается.
- `RelayCommandQueueService.SetRelayStateAsync` использует `AddAsync` для новой relay command.
- Relay command polling переотдает зависшие `Sent` команды через 1 минуту при `AttemptCount < 3` и до `ExpireAt`.
- `TelemetryRawEntityConfiguration` больше не делает index `IsAggregated` unique.
- `TelemetryAggregateEntityConfiguration` больше не делает index `IsAggregated` unique.
- Добавлена migration `20260429103915_AddDataAggregating`, которая пересоздает raw/aggregate `is_aggregated` indexes как non-unique.
- `SystemDbContextModelSnapshot` синхронизирован с non-unique `IsAggregated`.
- Migration `20260429103915_AddDataAggregating` применена на runtime-БД; non-unique `IsAggregated` является текущим baseline.

## Not A Priority Now

- Перенос ingress в `TelemetryService`: текущий boundary через `DeviceService` правильнее.
- CQRS/MediatR/SignalR/gRPC: это roadmap после стабилизации first hardware smoke.
- Расширение внешних notification providers: сначала нужен E2E alert chain.
