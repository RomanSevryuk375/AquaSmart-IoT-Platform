# IMPLEMENTATION_PLAN

## Goal

Довести проект до воспроизводимого E2E сценария для первого подключения МК/эмулятора: auth, device onboarding, telemetry ingress, сохранение телеметрии, реакция control rules и alert/notification flow.

## Baseline (29.04.2026)

Уже реализовано:
- gateway routing + JWT validation;
- `IdentityService` с register/login/refresh/logout/profile;
- исправленные register и refresh runtime-баги;
- refresh request DTO и хранение refresh token через hash;
- `DeviceService` с controller/sensor/relay CRUD, ping и telemetry ingress;
- `device-api` опубликован наружу на `localhost:5237` для direct lab mode;
- `X-Device-Token` для ping/telemetry;
- controller registration возвращает `ControllerId` и `DeviceToken` в body;
- `CreatedAtRoute` в controller registration использует корректный route value `{ id = response.ControllerId }`;
- batch telemetry response с `acceptedCount`, `skippedCount`, `validationErrors`;
- telemetry ingress валидирует пустой `Items`;
- telemetry item валидирует `Guid.Empty` для `SensorId`;
- telemetry item имеет явные messages для `ExternalMessageId`, `SensorId`, `RecordedAt`; `ExternalMessageId` ограничен 100 символами;
- `value: 0` больше не блокируется `Value.NotEmpty()` на уровне validator-а;
- unknown/unowned sensor ids добавляются в `validationErrors`;
- telemetry batch-size validator принимает до 50 элементов;
- `DeviceService` содержит device-facing relay command polling/ack endpoints под `X-Device-Token`;
- downstream-сервисы регистрируют общий JWT Bearer setup через `AddCommonAuthentication`;
- `DeviceService` собирается без ошибок после исправления `IRelayCommandQueueService`;
- `RelayCommandQueueService.SetRelayStateAsync` ставит новую command entity через `AddAsync`;
- `TelemetryService` с сохранением telemetry, duplicate-check и sensor no-data job;
- `TelemetryService` содержит raw/aggregate read endpoints и jobs сжатия telemetry;
- `ControlService` с automation rules, relay commands и alert events;
- исправленный `UserId` в `SensorNoDataAlertEvent`;
- `NotificationService` с реальными API/controllers/consumers/jobs;
- отдельная PostgreSQL БД на сервис;
- RabbitMQ/MassTransit integration.

Не завершено:
- MCU endpoints через gateway попадают под `/api/device/*` route policy и требуют JWT, поэтому первый MCU smoke должен идти напрямую в `device-api:5237`;
- `TelemetryService` требует `Ecosystem` по `ControllerId`, publisher `EcosystemCreatedEvent` не найден, а `EcosystemService.CreateEcosystemAsync` сейчас при missing projection возвращает `Success` без создания записи;
- refresh flow требует reuse/family revocation policy;
- relay command polling переводит команды в `Sent` и повторно выдает зависшие `Sent` команды через 1 минуту при `AttemptCount < 3` и до `ExpireAt`;
- alert-to-notification E2E не подтвержден runtime smoke;

## Phase Plan

### Phase 5C: Close Ecosystem Projection Gap

Статус: `pending`.

Задачи:
1. Решить, является ли `Aquarium` текущим источником `Ecosystem` для `TelemetryService`.
2. Если да, добавить публикацию/маппинг `EcosystemCreatedEvent` и `EcosystemDeletedEvent` из control/aquarium lifecycle или перевести telemetry consumers на aquarium events.
3. Исправить `EcosystemService.CreateEcosystemAsync`: duplicate/no-op должен быть при `existingEcosystem is not null`, а при `null` нужно создавать `EcosystemEntity`.
4. Подтвердить порядок событий: aquarium exists before sensor projection and telemetry consumption.
5. Проверить сценарий `create aquarium -> create sensor -> submit telemetry -> telemetry saved`.

Выход фазы:
- `TelemetryDataService` больше не возвращает retryable error `Ecosystem for controller ... not found` в первом hardware smoke.

### Phase 7: Planned Architecture Evolution

Статусы:
- `planned`: `CQRS + MediatR` для `ControlService`, затем `TelemetryService`;
- `planned`: `SignalR` для live dashboard;
- `planned`: `gRPC` для внутренних synchronous query-сценариев;
- `planned`: `gRPC streaming` для эмулятора/контроллера.

Порог входа:
- завершен первый воспроизводимый hardware/E2E smoke.

## Definition of Done

1. `dotnet build` проходит для всех API-проектов согласованным способом.
2. `docker compose up --build` поднимает gateway и все сервисы.
3. Direct MCU lab mode документирован.
4. Controller registration возвращает `ControllerId` и `DeviceToken`, а `Location` корректен.
5. Telemetry ingress принимает batch и возвращает понятную диагностику, включая корректный лимит batch size.
6. Telemetry raw/aggregate persistence не содержит unique `IsAggregated` regression, а соответствующие migrations уже применены.
7. Ecosystem projection в `TelemetryService` создается до обработки telemetry.
8. `value: 0`, unknown sensor ids, future `RecordedAt`, пустой/длинный `ExternalMessageId` и batch `0/1/50/51` проверены на running stack.
9. `TelemetryReportedFromHardwareEvent -> TelemetryReceivedEvent -> ControlService` подтвержден.
10. Relay command flow подтвержден runtime-smoke-ом, включая повторную выдачу `Sent` после потерянного ack.
11. Alert-to-notification flow подтвержден.
12. Оставшиеся roadmap-элементы явно помечены как planned, а не как реализованные.
