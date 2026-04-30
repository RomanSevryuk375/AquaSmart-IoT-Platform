# Учебное ТЗ: API для управления МК, аквариумами и террариумами

## Статус относительно текущего кода AquaAPI

- Этот файл описывает учебную/целевую архитектуру и сверяет ее с текущим кодом.
- По состоянию на 29 апреля 2026 года ingress телеметрии реализован через `DeviceService`, что соответствует правильной границе ответственности.
- Текущий endpoint: `POST /api/device/v1/sensors/telemetry`.
- `NotificationService` уже не является пустым шаблоном: есть API, consumers, jobs и отдельная БД.
- Старые blockers по register/refresh, refresh DTO/hash, aquarium namespace imports, `SensorNoDataAlertEvent.UserId` и публикации `device-api` исправлены в коде.
- `CreatedAtRoute` controller onboarding исправлен, пустой batch и `Guid.Empty` sensor id валидируются.
- Batch-size validator приведен к лимиту `1..50`.
- Telemetry validator теперь имеет явные messages для `MacAddress`, `Items`, `SensorId`, `ExternalMessageId`, `RecordedAt`; `ExternalMessageId <= 100`, `value: 0` не блокируется validator-ом.
- Unknown/unowned sensor ids добавляются в `validationErrors`.
- `DeviceService` и `TelemetryService` собираются через последовательный `dotnet build ... -m:1`.
- `SetRelayStateAsync` уже ставит backend relay command в очередь через `AddAsync`.
- Relay command polling переводит команды в `Sent`; зависшие `Sent` команды переотдаются через 1 минуту, максимум 3 попытки, до `ExpireAt`.
- Code-level и runtime blocker по unique `IsAggregated` закрыт: конфигурации, snapshot, migration `20260429103915_AddDataAggregating` и примененная БД используют non-unique indexes.
- `IdentityService` получил обязательный `TimeZone` и миграцию `20260429182407_AddTimeZoneForUser`, но register DTO пока использует legacy typo `TimaZone`, а profile DTO не возвращает/не меняет timezone.
- Оставшиеся blockers для МК: замкнуть `Ecosystem` projection для `TelemetryService`, подтвердить telemetry validation contract smoke-набором, подтвердить compose и пройти E2E smoke.

## 1. Цель проекта

Сделать учебную микросервисную систему для управления климатом, светом, фильтрацией и обслуживанием аквариумов/террариумов через МК.

Практическая цель:
- принять телеметрию от контроллера;
- сохранить измерения;
- проверить правила автоматизации;
- изменить состояние реле;
- уведомить пользователя при проблемах.

## 2. Минимальный состав сервисов

- `DeviceService` - МК, контроллеры, датчики, реле, ingress от железа.
- `TelemetryService` - история измерений и telemetry events.
- `ControlService` - аквариумы, правила, расписания, реакции.
- `NotificationService` - уведомления, напоминания, журнал обслуживания.
- `IdentityService` - пользователи, JWT, refresh tokens, подписки.
- `ApiGateway` - единая точка входа и JWT boundary.

## 3. Ключевой сценарий

1. МК отправляет telemetry batch в `DeviceService`.
2. `DeviceService` валидирует `MacAddress`, `X-Device-Token` и принадлежность датчиков.
3. `DeviceService` публикует `TelemetryReportedFromHardwareEvent`.
4. `TelemetryService` сохраняет данные и публикует `TelemetryReceivedEvent`.
5. `ControlService` проверяет правила и публикует relay-команду.
6. `DeviceService` меняет состояние реле.
7. `NotificationService` обрабатывает alert events и создает/отправляет уведомления.

## 4. Стек

- `.NET 8`
- `ASP.NET Core Web API`
- `EF Core`
- `PostgreSQL` отдельно на сервис
- `RabbitMQ`
- `MassTransit`
- `Quartz`
- `YARP`
- `Docker Compose`

## 5. Что обязательно в учебной части

- Один внешний ingress для железа через `DeviceService`.
- Одна асинхронная цепочка через RabbitMQ.
- Idempotency для обработки телеметрии.
- Сквозной маршрут через gateway.
- Отдельная БД на сервис.
- Документированный hardware/emulator smoke.

## 6. Что не входит в первый этап

- Kubernetes.
- High-load tuning.
- OTA firmware updates.
- Сложная RBAC-модель.
- Реальное железо как обязательная зависимость: первый smoke можно пройти эмулятором.
- Семисегментный индикатор `5641AS`: в актуальной аппаратной конфигурации не используется.
- Навигация через потенциометры: заменена на энкодер `KY-040` и обычную кнопку.

## 6A. Аппаратный минимум первого МК

Первый ESP32-контроллер должен быть описан в прошивке и документации как следующий набор устройств:
- `LCD1602` для локального статуса;
- `DS1307` для RTC;
- 8-канальный модуль реле;
- `XKC-Y25-V` как бесконтактный датчик уровня воды;
- `I2C soil moisture sensor` как датчик влажности грунта/субстрата;
- `AHT20 I2C` как датчик влажности и температуры поверхности/окружения;
- `DS18B20` как погружной датчик температуры воды;
- второй `DS18B20` или отдельный адрес на OneWire-шине для температуры поверхности, если установлен;
- энкодер `KY-040` и обычная кнопка-замыкалка для локальной навигации.

Практический вывод для API:
- каждый физический канал датчика должен иметь отдельный backend `sensorId`;
- AHT20 дает минимум два telemetry item-а: влажность и температура;
- два DS18B20 должны маппиться по OneWire ROM address, а не по порядку обнаружения;
- `XKC-Y25-V` должен отправляться как числовое boolean-состояние, например `0/1`, пока backend contract не выделит отдельный boolean type.

## 7. Критерии готовности

- `dotnet build` проходит воспроизводимо.
- `docker compose up --build` поднимает весь проект.
- МК access mode выбран и описан.
- Registration контроллера возвращает `ControllerId` и `DeviceToken`.
- Direct lab endpoint `http://localhost:5237` описан для МК/эмулятора.
- Сквозной сценарий "телеметрия -> решение -> команда -> уведомление" работает.
- README содержит API-примеры и smoke-сценарий.

## 8. Рекомендуемый telemetry ingress

Для AquaAPI прием телеметрии правильно строить через `DeviceService`, потому что этот сервис владеет контроллерами, токенами устройств, датчиками и реле.

Текущий вариант:

```http
POST http://localhost:5237/api/device/v1/sensors/telemetry
Header: X-Device-Token: <controller-device-token>
```

Текущий payload:

```json
{
  "macAddress": "AA:BB:CC:DD:EE:FF",
  "items": [
    {
      "sensorId": "a3c2f9c1-1111-2222-3333-444444444444",
      "value": 25.4,
      "externalMessageId": "esp32-1744621200-001:a3c2f9c1-1111-2222-3333-444444444444",
      "recordedAt": "2026-04-26T09:00:00Z"
    }
  ]
}
```

Что нужно довести:
- подтвердить runtime smoke для batch size `1`, `50` и `51`;
- подтвердить явные тексты ошибок для пустого/длинного `externalMessageId`, future `recordedAt`, пустого batch и `Guid.Empty`;
- подтвердить, что `value: 0` проходит validator и сохраняется как валидная телеметрия;
- подтвердить диагностику для sensor ids, которые не принадлежат controller-у;
- описать response ошибки/частичного принятия;
- подтвердить direct `device-api` runtime-smoke-ом.

Device-facing relay endpoints для прошивки:

```http
GET http://localhost:5237/api/device/v1/commands/pending/{controllerId}
Header: X-Device-Token: <controller-device-token>

POST http://localhost:5237/api/device/v1/commands/{commandId}/complete
Header: X-Device-Token: <controller-device-token>

POST http://localhost:5237/api/device/v1/commands/{commandId}/fail
Header: X-Device-Token: <controller-device-token>
Content-Type: application/json
```

Через gateway эти endpoints требуют JWT из-за route-level policy `/api/device/*`, поэтому первая прошивка должна использовать direct `device-api` path.

## 9. Что должен делать `DeviceService`

Ingress endpoint должен:
- проверить `MacAddress`;
- проверить `X-Device-Token`;
- проверить принадлежность сенсоров контроллеру;
- отклонять некорректные элементы диагностически;
- публиковать нормализованные telemetry events;
- по возможности обновлять `LastSeenAt` контроллера при валидном обмене.

## 10. Что должен делать `TelemetryService`

`TelemetryService` должен:
- проверить локальную проекцию сенсора;
- проверить `ExternalMessageId`;
- сохранить telemetry data;
- обработать duplicate delivery neutral success-ом;
- опубликовать `TelemetryReceivedEvent`;
- отслеживать no-data состояния.

Текущий gap:
- конкурентный duplicate-case требует явной обработки unique-index violation.
- telemetry aggregate migration/model и примененная БД уже используют non-unique `IsAggregated` indexes;
- `TelemetryService` требует `Ecosystem` по `ControllerId`, publisher `EcosystemCreatedEvent` в текущем коде не найден, а `CreateEcosystemAsync` сейчас при missing projection возвращает `Success` без создания записи.

## 11. Что должен делать `ControlService`

`ControlService` должен:
- consume-ить `TelemetryReceivedEvent`;
- находить правила по сенсору;
- сравнивать значение с порогами;
- публиковать `ChangeRelayStateCommand`;
- публиковать alert events с корректным `UserId`.

Текущий статус:
- `UserId` в `SensorNoDataAlertEvent` исправлен через `Aquarium.UserId`.

## 12. Что должен делать `NotificationService`

`NotificationService` должен покрывать:
- reminders;
- maintenance log;
- user-facing notifications;
- sensor no data alerts;
- controller offline alerts;
- critical telemetry alerts.

Текущий статус:
- сервис структурно реализован;
- namespace imports по aquarium events исправлены;
- runtime alert smoke еще не подтвержден.

## 13. Практические next steps

1. Подтвердить `docker compose up --build`.
2. Исправить и замкнуть создание `Ecosystem` для `TelemetryService`.
3. Подтвердить validation error messages telemetry ingress с README/docs, включая `ExternalMessageId`, `RecordedAt`, batch size, `value: 0`, unknown sensors.
4. Пройти direct MCU lab mode smoke.
5. Пройти E2E smoke с эмулятором.

## 14. Planned архитектурные расширения

### `planned`: CQRS + MediatR

Внедрять после первого стабильного E2E. Начинать с `ControlService`.

### `planned`: SignalR

Внедрять после стабильных telemetry/alert events. Цель: live dashboard без polling.

### `planned`: gRPC

Внедрять для внутренних synchronous query-сценариев, например проверки лимитов подписки.

### `planned`: gRPC streaming

Внедрять после стабильной HTTP-схемы обмена с МК. Цель: постоянный канал с эмулятором/контроллером.
