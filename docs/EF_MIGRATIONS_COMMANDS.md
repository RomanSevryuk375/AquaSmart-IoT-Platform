# EF migrations commands

Команды рассчитаны на запуск из корня репозитория `C:\Work Space\AquaAPI`.

В проекте сейчас 5 EF Core контекстов:
- `IdentityService`: `IdentityDbContext`;
- `DeviceService`: `SystemDbContext`;
- `TelemetryService`: `SystemDbContext`;
- `ControlService`: `SystemDbContext`;
- `NotificationService`: `SystemDbContext`.

> Заменяй `MigrationName` на имя новой миграции, например `AddUserIdToControllers`.

## Перед запуском

Проверить наличие `dotnet-ef`:

```powershell
dotnet ef --version
```

Если tool не установлен:

```powershell
dotnet tool install --global dotnet-ef
```

Для локального применения миграций БД должны быть подняты:

```powershell
docker compose up -d identity-db device-db telemetry-db control-db notification-db
```

Если `dotnet` падает на first-time setup из-за прав, можно задать локальный home:

```powershell
$env:DOTNET_CLI_HOME="C:\Work Space\AquaAPI\.dotnet-home"
$env:DOTNET_SKIP_FIRST_TIME_EXPERIENCE="1"
```

Для design-time запуска startup projects также нужны базовые RabbitMQ настройки:

```powershell
$env:RabbitMqOptions__Host="amqp://localhost:5672"
$env:RabbitMqOptions__UserName="guest"
$env:RabbitMqOptions__Password="guest"
```

## IdentityService

Connection string:

```powershell
$env:ConnectionStrings__IdentityDbContext="Host=localhost;Port=5441;Database=identity_db;Username=postgres;Password=password"
```

Добавить миграцию:

```powershell
dotnet ef migrations add AddTimeZoneForUser `
  --project src/Services/IdentityService/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj `
  --startup-project src/Services/IdentityService/IdentityService/IdentityService.API.csproj `
  --context IdentityDbContext `
  --output-dir Migrations
```

Текущий статус на 29 апреля 2026:

- миграция `20260429182407_AddTimeZoneForUser` добавляет обязательный `time_zone` в users;
- `RegisterUserRequestDto` уже принимает значение, но поле называется `TimaZone` из-за legacy typo;
- `UserProfileResponseDto` и `UpdateProfileRequestDto` пока не экспонируют `TimeZone`, поэтому API профиля еще не полностью согласован с новой колонкой.

Применить миграции:

```powershell
dotnet ef database update `
  --project src/Services/IdentityService/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj `
  --startup-project src/Services/IdentityService/IdentityService/IdentityService.API.csproj `
  --context IdentityDbContext `
  --connection "Host=localhost;Port=5441;Database=identity_db;Username=postgres;Password=password"
```

## DeviceService

Connection string:

```powershell
$env:ConnectionStrings__SystemDbContext="Host=localhost;Port=5439;Database=device_db;Username=postgres;Password=password"
```

Добавить миграцию:

```powershell
dotnet ef migrations add AddRelayCommandsQueue `
  --project src/Services/DeviceService/Device.Infrastrucrute/Device.Infrastructure.csproj `
  --startup-project src/Services/DeviceService/DeviceService/Device.API.csproj `
  --context SystemDbContext `
  --output-dir Migrations
```

Применить миграции:

```powershell
dotnet ef database update `
  --project src/Services/DeviceService/Device.Infrastrucrute/Device.Infrastructure.csproj `
  --startup-project src/Services/DeviceService/DeviceService/Device.API.csproj `
  --context SystemDbContext `
  --connection "Host=localhost;Port=5439;Database=device_db;Username=postgres;Password=password"
```

## TelemetryService

Connection string:

```powershell
$env:ConnectionStrings__SystemDbContext="Host=localhost;Port=5438;Database=telemetry_db;Username=postgres;Password=password"
```

Добавить миграцию:

```powershell
dotnet ef migrations add AddDataAggregating `
  --project src/Services/TelemetryService/Telemetry.Infrastructure/Telemetry.Infrastructure.csproj `
  --startup-project src/Services/TelemetryService/TelemetryService/Telemetry.API.csproj `
  --context SystemDbContext `
  --output-dir Migrations
```

Текущий статус на 29 апреля 2026:

- `TelemetryRawEntityConfiguration` и `TelemetryAggregateEntityConfiguration` используют обычный index по `IsAggregated`;
- `SystemDbContextModelSnapshot.cs` синхронизирован с non-unique `IsAggregated`;
- migration `20260429103915_AddDataAggregating.cs` drop/create-ит indexes `ix_telemetry_raw_data_is_aggregated` и `ix_telemetry_aggregate_data_is_aggregated` без `unique: true`;
- migration `20260429103915_AddDataAggregating.cs` применена на runtime-БД; non-unique `IsAggregated` является текущим baseline.

Audit-проверка indexes в PostgreSQL:

```sql
select indexname, indexdef
from pg_indexes
where tablename in ('telemetry_raw_data', 'telemetry_aggregate_data')
  and indexname like '%is_aggregated%';
```

В `indexdef` не должно быть `UNIQUE`.

Применить миграции:

```powershell
dotnet ef database update `
  --project src/Services/TelemetryService/Telemetry.Infrastructure/Telemetry.Infrastructure.csproj `
  --startup-project src/Services/TelemetryService/TelemetryService/Telemetry.API.csproj `
  --context SystemDbContext `
  --connection "Host=localhost;Port=5438;Database=telemetry_db;Username=postgres;Password=password"
```

## ControlService

Connection string:

```powershell
$env:ConnectionStrings__SystemDbContext="Host=localhost;Port=5440;Database=control_db;Username=postgres;Password=password"
```

Добавить миграцию:

```powershell
dotnet ef migrations add MigrationName `
  --project src/Services/ControlService/ControlService.Infrastructure/Control.Infrastructure.csproj `
  --startup-project src/Services/ControlService/ControlService/Control.API.csproj `
  --context SystemDbContext `
  --output-dir Migrations
```

Применить миграции:

```powershell
dotnet ef database update `
  --project src/Services/ControlService/ControlService.Infrastructure/Control.Infrastructure.csproj `
  --startup-project src/Services/ControlService/ControlService/Control.API.csproj `
  --context SystemDbContext `
  --connection "Host=localhost;Port=5440;Database=control_db;Username=postgres;Password=password"
```

## NotificationService

Connection string:

```powershell
$env:ConnectionStrings__SystemDbContext="Host=localhost;Port=5442;Database=notification_db;Username=postgres;Password=password"
```

Добавить миграцию:

```powershell
dotnet ef migrations add MigrationName `
  --project src/Services/NotificationService/Notification.Infrastructure/Notification.Infrastructure.csproj `
  --startup-project src/Services/NotificationService/NotificationService/Notification.API.csproj `
  --context SystemDbContext `
  --output-dir Migrations
```

Применить миграции:

```powershell
dotnet ef database update `
  --project src/Services/NotificationService/Notification.Infrastructure/Notification.Infrastructure.csproj `
  --startup-project src/Services/NotificationService/NotificationService/Notification.API.csproj `
  --context SystemDbContext `
  --connection "Host=localhost;Port=5442;Database=notification_db;Username=postgres;Password=password"
```

## Примечание про автоприменение миграций

Во всех API-проектах сейчас есть вызов:

```csharp
context.Database.Migrate();
```

То есть при старте сервиса миграции также применяются автоматически. Команды `dotnet ef database update` нужны для ручного обновления БД без запуска всего API или для проверки миграций отдельно.
