# ESP32 Firmware TZ and Implementation Plan

## Document Purpose

Этот документ описывает техническое задание и план реализации прошивки для микроконтроллера ESP32, который подключается к backend `AquaAPI`.

Документ специально структурирован так, чтобы его было удобно использовать нейросетям и coding agents:
- требования разбиты на атомарные пункты;
- аппаратные модули перечислены явно;
- состояния, события, команды и контракты описаны отдельно;
- план реализации разделен на независимые firmware-модули;
- критерии приемки сформулированы проверяемо.

## Target Hardware

Обязательное железо:
- MCU: `ESP32`.
- Relay module: восьмиканальный модуль реле, `8 channels`.
- Display: `LCD1602`, preferably via I2C adapter.
- RTC: `DS1307`.
- Indicators: светодиоды разных цветов для статуса.
- Navigation input: rotary encoder `KY-040` and one ordinary momentary push button.

Обязательные датчики первой версии:
- `XKC-Y25-V`: бесконтактный датчик уровня воды, digital input.
- `I2C soil moisture sensor`: датчик влажности грунта/субстрата, I2C.
- `AHT20 I2C`: датчик влажности воздуха и температуры поверхности/окружения, I2C.
- `DS18B20`: погружной датчик температуры воды, OneWire.
- `DS18B20`: датчик температуры поверхности, OneWire; можно использовать отдельный DS18B20 на той же OneWire-шине.

Потенциальные датчики для следующих версий:
- мутность воды;
- дополнительные датчики pH/kH/NO3, если будут добавлены позже.

Explicit exclusions for first firmware version:
- four-digit seven-segment indicator `5641AS` is not used at all;
- potentiometers are not used for navigation.

## Backend Integration Summary

Текущий recommended ingress:

```http
POST http://localhost:5237/api/device/v1/sensors/telemetry
Header: X-Device-Token: <controller-device-token>
Content-Type: application/json
```

Payload:

```json
{
  "macAddress": "AA:BB:CC:DD:EE:FF",
  "items": [
    {
      "sensorId": "a3c2f9c1-1111-2222-3333-444444444444",
      "value": 25.4,
      "externalMessageId": "esp32-000001-temp-water",
      "recordedAt": "2026-04-26T09:00:00Z"
    }
  ]
}
```

Expected response:

```json
{
  "acceptedCount": 1,
  "validationErrors": [],
  "skippedCount": 0
}
```

Important backend assumptions:
- `macAddress` identifies the controller.
- `X-Device-Token` authenticates the controller.
- each telemetry item must reference a registered backend `sensorId`;
- `externalMessageId` must be unique and stable for idempotency;
- first lab mode should use direct `device-api` on port `5237`;
- gateway mode requires JWT and is not recommended for first MCU smoke.

Known backend notes before stable MCU test:
- `TelemetryBatchRequestValidator` now matches the expected `1..50` batch size contract;
- protected user-facing endpoints accept `Authorization: Bearer <accessToken>` and, for browser clients, `AccessToken` cookie;
- login/register/refresh set `AccessToken` and `RefreshToken` cookies;
- gateway `/api/device/*` routes require JWT before requests reach downstream `[AllowAnonymous]` device endpoints, so firmware must use direct `device-api` for first smoke;
- `DeviceService` build is currently confirmed successful;
- backend relay command enqueue uses `AddAsync`, but command flow still needs runtime smoke;
- command polling marks returned commands as `Sent`; if acknowledgement is lost, backend reissues `Sent` commands after 1 minute while `AttemptCount < 3` and before `ExpireAt`;
- telemetry aggregate config/migration/snapshot and applied runtime database use non-unique indexes on `IsAggregated`;
- `TelemetryService` also requires an `Ecosystem` projection for the controller before telemetry can be saved;
- current backend code has an `EcosystemService.CreateEcosystemAsync` bug: missing ecosystem returns `Success` without creating a projection, while an existing ecosystem path attempts to create a duplicate;
- telemetry validation now has explicit messages for `MacAddress`, `Items`, `SensorId`, `ExternalMessageId` and `RecordedAt`; `ExternalMessageId` is limited to 100 characters;
- `value: 0` is no longer rejected by `TelemetryItemRequestValidator`, but must still be included in first smoke tests.

## Firmware Goals

Primary goal:
- ESP32 must collect sensor values, control 8 relay channels, show local status on LCD1602/LEDs, keep local time using DS1307, and send telemetry batches to `AquaAPI`.

Secondary goals:
- tolerate temporary Wi-Fi/backend failures;
- preserve relay safety state after reboot;
- provide local navigation/settings with `KY-040` rotary encoder and one momentary push button;
- keep firmware modular enough for later migration to MQTT/gRPC/streaming if backend evolves.

Non-goals for first firmware version:
- OTA updates;
- encrypted local storage;
- complex UI menus;
- full offline automation engine;
- Wi-Fi provisioning portal;
- gateway JWT flow.

## Firmware Architecture

Recommended language/framework:
- Arduino framework for ESP32 or ESP-IDF.

Recommended first implementation:
- Arduino framework if development speed is more important.
- ESP-IDF if long-term production quality is more important.

Core modules:
- `ConfigStore`: stores controller settings.
- `WiFiManager`: connects and reconnects Wi-Fi.
- `RtcClock`: reads and writes time using DS1307.
- `SensorManager`: polls physical sensors.
- `RelayManager`: controls 8 relay channels.
- `TelemetryQueue`: buffers telemetry before send.
- `ApiClient`: sends HTTP telemetry batches to AquaAPI.
- `DisplayManager`: controls LCD1602.
- `LedIndicator`: controls status LEDs.
- `InputManager`: reads KY-040 rotation, KY-040 press and the extra momentary push button.
- `UiController`: local menu/state screen logic.
- `Scheduler`: periodic task timing.
- `Diagnostics`: logs boot, network, API, sensor and relay status.

## Configuration Model

Required firmware config:

```json
{
  "device": {
    "macAddress": "AA:BB:CC:DD:EE:FF",
    "controllerId": "backend-controller-guid",
    "deviceToken": "backend-device-token",
    "apiBaseUrl": "http://192.168.1.100:5237",
    "telemetryPath": "/api/device/v1/sensors/telemetry"
  },
  "wifi": {
    "ssid": "wifi-name",
    "password": "wifi-password"
  },
  "telemetry": {
    "sendIntervalMs": 10000,
    "maxBatchSize": 50,
    "maxQueueSize": 500
  },
  "relaySafeState": {
    "channel1": false,
    "channel2": false,
    "channel3": false,
    "channel4": false,
    "channel5": false,
    "channel6": false,
    "channel7": false,
    "channel8": false
  }
}
```

Sensor mapping config:

```json
[
  {
    "localKey": "water_temperature",
    "sensorId": "backend-sensor-guid",
    "driver": "DS18B20",
    "bus": "onewire1",
    "deviceAddress": "28-000000000001",
    "unit": "C",
    "pollIntervalMs": 5000,
    "enabled": true
  },
  {
    "localKey": "surface_temperature",
    "sensorId": "backend-sensor-guid",
    "driver": "DS18B20",
    "bus": "onewire1",
    "deviceAddress": "28-000000000002",
    "unit": "C",
    "pollIntervalMs": 5000,
    "enabled": true
  },
  {
    "localKey": "air_humidity",
    "sensorId": "backend-sensor-guid",
    "driver": "AHT20",
    "bus": "i2c1",
    "address": "0x38",
    "unit": "%",
    "pollIntervalMs": 5000,
    "enabled": true
  },
  {
    "localKey": "aht20_temperature",
    "sensorId": "backend-sensor-guid",
    "driver": "AHT20",
    "bus": "i2c1",
    "address": "0x38",
    "unit": "C",
    "pollIntervalMs": 5000,
    "enabled": true
  },
  {
    "localKey": "soil_moisture",
    "sensorId": "backend-sensor-guid",
    "driver": "I2C_SOIL_MOISTURE",
    "bus": "i2c1",
    "address": "device-specific",
    "unit": "%",
    "pollIntervalMs": 10000,
    "enabled": true
  },
  {
    "localKey": "water_level",
    "sensorId": "backend-sensor-guid",
    "driver": "XKC-Y25-V",
    "pin": 34,
    "unit": "bool",
    "pollIntervalMs": 1000,
    "enabled": true
  }
]
```

Relay mapping config:

```json
[
  {
    "channel": 1,
    "relayId": "backend-relay-guid",
    "pin": 16,
    "purpose": "filter",
    "activeLow": true,
    "safeState": false
  }
]
```

## Pin Planning

Create a dedicated pin map before coding.

Pin map requirements:
- avoid ESP32 boot-strapping pins for relay outputs where unsafe startup toggles are possible;
- keep I2C pins shared by LCD1602 I2C adapter, DS1307, AHT20 and I2C soil moisture sensor;
- verify I2C addresses before coding, because LCD1602 adapters commonly use `0x27` or `0x3F`, DS1307 uses `0x68`, AHT20 commonly uses `0x38`, and soil moisture sensor address depends on the module;
- reserve one GPIO for OneWire DS18B20 bus; multiple DS18B20 sensors can share this bus if their ROM addresses are configured;
- reserve one input GPIO for `XKC-Y25-V` water level signal;
- reserve two input GPIOs for `KY-040` `CLK`/`DT`, one input for the KY-040 push switch if used, and one input for the extra momentary push button;
- use pull-up/pull-down strategy consistently for KY-040 and the ordinary button;
- do not allocate pins for `5641AS`; this indicator is excluded from the first firmware.

Recommended abstractions:
- never hardcode pins inside business logic;
- define all pins in `PinMap.h` or `pins_config.cpp`;
- each hardware module receives pins through constructor/config.

## Runtime State Machine

Firmware states:

```text
BOOT
LOAD_CONFIG
INIT_HARDWARE
CONNECT_WIFI
SYNC_TIME
ONLINE
DEGRADED_OFFLINE
SAFE_MODE
FACTORY_RESET
```

State definitions:
- `BOOT`: CPU started, no hardware assumption yet.
- `LOAD_CONFIG`: read config from flash/NVS.
- `INIT_HARDWARE`: initialize GPIO, relay safe states, displays, RTC, sensors.
- `CONNECT_WIFI`: attempt Wi-Fi connection.
- `SYNC_TIME`: prefer NTP if online, fallback to DS1307.
- `ONLINE`: telemetry can be sent to backend.
- `DEGRADED_OFFLINE`: sensors and local display work, telemetry is queued.
- `SAFE_MODE`: relays forced to safe states after critical error.
- `FACTORY_RESET`: clear config after explicit local action.

State transition rules:
- `BOOT -> LOAD_CONFIG` always.
- `LOAD_CONFIG -> INIT_HARDWARE` if config valid.
- `LOAD_CONFIG -> FACTORY_RESET` if config missing and reset requested.
- `INIT_HARDWARE -> CONNECT_WIFI` after hardware init.
- `CONNECT_WIFI -> SYNC_TIME` if Wi-Fi connected.
- `CONNECT_WIFI -> DEGRADED_OFFLINE` after retry timeout.
- `SYNC_TIME -> ONLINE` when time source is valid.
- `ONLINE -> DEGRADED_OFFLINE` on repeated API/Wi-Fi failures.
- `DEGRADED_OFFLINE -> ONLINE` after Wi-Fi and API health recover.
- any state -> `SAFE_MODE` on critical hardware failure.

## Relay Control Requirements

Relay module:
- 8 channels.
- Each channel has configured GPIO pin.
- Each channel supports `activeLow`.
- Each channel has `safeState`.
- Startup must apply safe states before network operations.

Relay purposes:
- channel 1: filter;
- channel 2: filter reserve or second filter;
- channel 3: heater;
- channel 4: pump;
- channel 5: lamp;
- channel 6: light;
- channel 7: reserve;
- channel 8: reserve.

Relay commands for first firmware:
- local manual toggle from UI;
- apply configured safe state;
- poll backend for desired state through the DeviceService command queue.

Important gap:
- backend exposes HTTP command polling/ack endpoints and `SetRelayStateAsync` now creates new commands with `AddAsync`.
- firmware should implement polling, but backend-driven relay control remains smoke-test pending until `set-state/toggle-state -> pending -> complete/fail` is verified on the running stack;
- after `GET pending`, backend marks commands as `Sent`; if acknowledgement is lost, the command is returned again after 1 minute while `attemptCount < 3` and `expireAt` is still in the future.

Command retrieval for first version:
- `GET /api/device/v1/commands/pending/{controllerId}`;
- `POST /api/device/v1/commands/{commandId}/complete`;
- `POST /api/device/v1/commands/{commandId}/fail`;
- include `X-Device-Token` on every command request;
- ESP32 polls commands every `2..5 seconds`;
- command response uses `id`, `controllerId`, `relayId`, `action`, `status`, `expireAt`, `attemptCount`, `processedAt`, `errorMessage`, `createdAt`;
- ESP32 acknowledges command execution.

## Sensor Requirements

### XKC-Y25-V Water Level Sensor

Purpose:
- detect whether water is present at the configured level without direct contact.

Firmware requirements:
- read as digital input;
- debounce/filter state changes to avoid false transitions from water movement;
- expose telemetry as `0/1` or equivalent boolean numeric value agreed with backend;
- trigger local safety rule when unsafe low level is detected.

Safety behavior:
- if water level is unsafe, turn pump and heater relays off;
- show water-level error/status on LCD1602;
- continue telemetry queueing so backend receives the condition when online.

### I2C Soil Moisture Sensor

Purpose:
- measure substrate/soil moisture for terrarium or planted ecosystem scenarios.

Firmware requirements:
- initialize on I2C bus after address detection/config load;
- convert raw reading to percent only after calibration values are configured;
- expose raw value in diagnostics even if calibrated percent is used for telemetry;
- report sensor read failure as `E007`.

Calibration:
- store dry and wet calibration points in config;
- clamp computed percentage to `0..100`;
- avoid sending calibration constants to backend as telemetry.

### AHT20 I2C Humidity and Temperature Sensor

Purpose:
- measure air humidity and local surface/ambient temperature.

Firmware requirements:
- initialize through I2C;
- read humidity and temperature as separate telemetry items with separate backend `sensorId` values;
- retry read once before marking `E007`;
- avoid blocking the main loop while waiting for measurement.

### DS18B20 Temperature Sensors

Purpose:
- measure water temperature with a submerged probe;
- measure surface temperature with a second DS18B20 probe if installed.

Firmware requirements:
- use OneWire bus;
- support at least two configured DS18B20 ROM addresses on the same bus;
- map each ROM address to its own `localKey` and backend `sensorId`;
- reject invalid readings such as disconnected-device sentinel values;
- use non-blocking conversion timing where possible.

Safety behavior:
- if water temperature sensor is unavailable, turn heater relay off;
- if surface temperature sensor is unavailable, continue water safety logic but show degraded sensor status.

## Telemetry Requirements

Telemetry item fields:
- `sensorId`: backend GUID.
- `value`: numeric sensor value.
- `externalMessageId`: unique message id.
- `recordedAt`: ISO-8601 UTC timestamp.

Backend validation expectations for firmware:
- `sensorId` must not be `00000000-0000-0000-0000-000000000000`;
- `externalMessageId` must not be empty or whitespace;
- `externalMessageId` length must be `<= 100`;
- `recordedAt` must not be more than about 5 minutes in the future;
- batch size must be `1..50`;
- `value: 0` should be sent normally when it represents a valid reading; current backend validator does not reject zero.

Important `ExternalMessageId` validation note:
- current code has explicit messages: `ExternalMessageId must not be empty.` and `ExternalMessageId is too long (max 100).`;
- validation response shape is still a string list such as `Items[0].ExternalMessageId: ExternalMessageId must not be empty.`;
- firmware should log the full text, but branch logic should prefer local field validation before sending rather than parsing backend strings.

`externalMessageId` format:

```text
<mac-without-separators>-<bootCounter>-<sequence>-<localSensorKey>
```

Example:

```text
AABBCCDDEEFF-0007-00000042-water_temperature
```

Telemetry batching:
- collect readings into queue;
- send every `sendIntervalMs`;
- max batch size: `50`;
- if offline, keep queue until `maxQueueSize`;
- if queue full, drop oldest non-critical readings first;
- never generate duplicate `externalMessageId` for a new reading;
- retry same queued reading with same `externalMessageId`.

Timestamp rules:
- use NTP when Wi-Fi is available;
- use DS1307 as fallback source;
- if time is invalid, mark device degraded and avoid sending telemetry until time is usable;
- all backend timestamps should be UTC.

## Display Requirements

LCD1602:
- default screen: current status.
- show Wi-Fi state.
- show API state.
- show current selected sensor value.
- show relay state page.
- show error page if API validation fails.
- show water level state from `XKC-Y25-V`.
- show humidity/temperature from `AHT20`.
- show water/surface temperature from configured `DS18B20` sensors.
- show soil moisture percentage after calibration.

Example LCD screens:

```text
AquaAPI ONLINE
Twater 25.4 C
```

```text
WiFi OFFLINE
Queue 024/500
```

```text
Relay 1 FILTER
State ON
```

The `5641AS` seven-segment indicator is not part of the firmware UI. Do not allocate pins, display modes, drivers or tasks for it.

LED indicators:
- green: online/API OK;
- blue: Wi-Fi connected;
- yellow: degraded/offline queue active;
- red: critical error/safe mode;
- white or custom: relay activity pulse.

## Input Requirements

Supported navigation hardware:
- `KY-040` rotary encoder;
- one ordinary momentary push button.

KY-040 mapping:
- clockwise rotation: next item / increase value.
- counter-clockwise rotation: previous item / decrease value.
- KY-040 switch press, if connected: `OK` / select.
- KY-040 long press, if connected: back or mode switch.

Ordinary button mapping:
- short press: `BACK` or page switch, depending on current menu level.
- long press: confirm critical action, for example factory reset confirmation.
- double press can be used for quick status page switch if implemented reliably.

Input processing:
- debounce KY-040 `CLK`/`DT` and both push buttons;
- decode encoder direction without blocking the main loop;
- ignore invalid bounce transitions from the encoder;
- keep navigation usable while telemetry/network tasks run;
- prevent accidental factory reset with long-press confirmation.

## Error Codes

Firmware should have stable short error codes.

Suggested codes:
- `E001`: config missing or invalid.
- `E002`: Wi-Fi connection failed.
- `E003`: RTC unavailable.
- `E004`: time invalid.
- `E005`: backend unavailable.
- `E006`: backend rejected telemetry validation.
- `E007`: sensor read failed.
- `E008`: relay hardware init failed.
- `E009`: telemetry queue overflow.
- `E010`: device token missing.

Each error should be:
- logged to serial;
- visible on LCD1602;
- mapped to LED behavior.

`E006` handling details:
- log the full `validationErrors[]` strings to serial;
- show only short status on displays, for example `E006 EXTID`, `E006 VALUE`, `E006 TIME`, `E006 SIZE`;
- if backend returns `acceptedCount > 0` and `skippedCount > 0`, use `validationErrors[]` to identify unknown/unowned sensor ids; if mapping is ambiguous, log and keep local diagnostics for manual correction;
- do not infer `ExternalMessageId` failure by searching for unrelated field names.

## Safety Requirements

Startup safety:
- initialize relay GPIO as output immediately;
- set all relays to configured safe states before Wi-Fi;
- do not toggle relays during display/RTC init.

Offline safety:
- continue local sensor reading;
- keep relays in last safe/local state unless critical sensor condition requires safe state;
- queue telemetry;
- show offline status.

Critical safety:
- if DS18B20 water temperature sensor is unavailable, turn heater relay off;
- if water level sensor indicates unsafe low level, turn pump/heater off;
- if firmware detects invalid config, enter safe mode;
- if relay control pin init fails, enter safe mode.
- if I2C bus fails, mark AHT20, DS1307, LCD1602 and soil moisture sensor degraded according to which devices are unavailable.

## Implementation Plan

### Phase 0: Firmware Project Skeleton

Tasks:
1. Choose Arduino framework or ESP-IDF.
2. Create project structure.
3. Add modules:
   - `PinMap`;
   - `ConfigStore`;
   - `RelayManager`;
   - `RtcClock`;
   - `DisplayManager`;
   - `InputManager`;
   - `SensorManager`;
   - `XkcY25vSensor`;
   - `SoilMoistureI2cSensor`;
   - `Aht20Sensor`;
   - `Ds18b20Sensor`;
   - `TelemetryQueue`;
   - `ApiClient`;
   - `Diagnostics`.
4. Add serial logging.
5. Add compile-time feature flags.

Acceptance:
- firmware compiles;
- serial boot log prints firmware version;
- relays initialize to safe state.

### Phase 1: Hardware Bring-Up

Tasks:
1. Test relay channel 1..8 manually.
2. Test LCD1602 output.
3. Test DS1307 read/write.
4. Test XKC-Y25-V digital water-level reading.
5. Test I2C soil moisture reading and calibration output.
6. Test AHT20 humidity and temperature reading.
7. Test DS18B20 water and surface temperature probes.
8. Test KY-040 rotation and press.
9. Test ordinary momentary push button.
10. Test LEDs.

Acceptance:
- each hardware module has a standalone diagnostic screen/log.
- no relay toggles unexpectedly during boot.
- I2C device scan identifies expected devices or reports missing devices clearly.

### Phase 2: Config and UI

Tasks:
1. Store Wi-Fi credentials.
2. Store `apiBaseUrl`.
3. Store `macAddress`.
4. Store `deviceToken`.
5. Store sensor mapping.
6. Store relay mapping.
7. Implement LCD status screens.
8. Implement basic navigation.

Acceptance:
- config survives reboot;
- user can view Wi-Fi/API/sensor/relay status locally.

### Phase 3: Wi-Fi and Time

Tasks:
1. Connect to Wi-Fi.
2. Reconnect after disconnect.
3. Sync time from NTP.
4. Write synced time into DS1307.
5. Use DS1307 fallback when offline.

Acceptance:
- valid UTC timestamp is available before telemetry send.
- device enters `DEGRADED_OFFLINE` if Wi-Fi is unavailable.

### Phase 4: Sensor Polling

Tasks:
1. Implement sensor abstraction.
2. Implement XKC-Y25-V water-level driver.
3. Implement I2C soil moisture driver and calibration conversion.
4. Implement AHT20 driver integration for humidity and temperature.
5. Implement DS18B20 OneWire integration for water and surface temperature.
6. Poll sensors by configured interval.
7. Validate sensor readings.
8. Push readings into telemetry queue.
9. Show primary readings and sensor status on LCD1602.

Acceptance:
- readings are stable in serial logs;
- invalid readings produce `E007`;
- telemetry queue receives one item per configured sensor interval.
- unsafe water-level state forces configured safety behavior.

### Phase 5: Telemetry HTTP Integration

Tasks:
1. Implement `ApiClient`.
2. Add `X-Device-Token` header.
3. Build JSON payload.
4. Send up to 50 items per request.
5. Parse `acceptedCount`, `skippedCount`, `validationErrors`.
6. On success, remove accepted items from queue.
7. On network failure, keep queue.
8. On validation failure, show `E006` and log backend message.

Acceptance:
- ESP32 can send telemetry to direct `device-api`.
- backend returns `202 Accepted`.
- duplicate retry uses same `externalMessageId`.

### Phase 6: Relay Local Control

Tasks:
1. Implement relay state model.
2. Implement manual local toggle from UI.
3. Show relay state on LCD1602.
4. Persist last selected relay mode if needed.
5. Add safety overrides.

Acceptance:
- each relay can be toggled locally.
- safe mode forces configured safe states.

### Phase 7: Backend Command Flow

Tasks:
1. Use backend command polling endpoint.
2. Implement command poll in firmware.
3. Apply relay command.
4. Acknowledge command result.
5. Add idempotency by `commandId`.

Acceptance:
- backend can change physical relay state.
- repeated command does not double-apply unsafe operation.

### Phase 8: Long-Run Smoke

Tasks:
1. Run firmware for 8 hours.
2. Disconnect Wi-Fi for 10 minutes.
3. Restore Wi-Fi and verify queue flush.
4. Reboot ESP32 and verify safe relay states.
5. Verify DS1307 fallback time.
6. Verify backend telemetry history.

Acceptance:
- no crash/reboot loop;
- no unexpected relay toggle;
- telemetry gap and recovery are explainable in logs.

## Suggested Firmware File Structure

```text
firmware/
  platformio.ini
  src/
    main.cpp
    PinMap.h
    AppState.h
    ConfigStore.h
    ConfigStore.cpp
    WiFiManagerEx.h
    WiFiManagerEx.cpp
    RtcClock.h
    RtcClock.cpp
    RelayManager.h
    RelayManager.cpp
    SensorManager.h
    SensorManager.cpp
    sensors/
      XkcY25vSensor.h
      XkcY25vSensor.cpp
      SoilMoistureI2cSensor.h
      SoilMoistureI2cSensor.cpp
      Aht20Sensor.h
      Aht20Sensor.cpp
      Ds18b20Sensor.h
      Ds18b20Sensor.cpp
    TelemetryQueue.h
    TelemetryQueue.cpp
    ApiClient.h
    ApiClient.cpp
    DisplayManager.h
    DisplayManager.cpp
    LedIndicator.h
    LedIndicator.cpp
    InputManager.h
    InputManager.cpp
    UiController.h
    UiController.cpp
    Diagnostics.h
    Diagnostics.cpp
  test/
    test_telemetry_queue.cpp
    test_external_message_id.cpp
```

## AI Agent Implementation Rules

When generating firmware code, follow these rules:
- keep hardware access isolated behind modules;
- no business logic directly inside `loop()`;
- `loop()` should call scheduler/task methods only;
- all pins must come from `PinMap`;
- do not implement or reference `5641AS`;
- do not implement potentiometer navigation;
- navigation must use KY-040 and the ordinary momentary button;
- I2C bus access must be shared safely between LCD1602, DS1307, AHT20 and soil moisture sensor;
- DS18B20 sensors must be identified by configured OneWire ROM addresses, not by discovery order alone;
- all backend URLs and tokens must come from config;
- do not hardcode backend `sensorId` values in source code;
- generate stable `externalMessageId`;
- use non-blocking timing with `millis()` where possible;
- avoid long `delay()` calls except short display timing;
- relay safe state must be applied before Wi-Fi connection;
- serial logs must include state transitions and API responses;
- telemetry queue must survive temporary network errors;
- every API validation error should be visible in logs and UI.

## Backend Tasks Needed For Full MCU Support

Required before stable MCU telemetry smoke:
- keep telemetry batch-size validator at `1..50`;
- treat telemetry migration `20260429103915_AddDataAggregating` as applied baseline and verify telemetry persistence in smoke;
- ensure `TelemetryService` receives or creates `Ecosystem` projection for the controller before telemetry consumption;
- fix `EcosystemService.CreateEcosystemAsync` existing/null check;
- keep explicit validation messages documented for firmware diagnostics or add machine-readable error codes later;
- verify valid `value: 0` readings in runtime smoke;
- verify diagnostics for skipped unknown/unowned sensor ids;
- verify at least two telemetry rows can be saved from one or multiple batches;
- run `docker compose up --build`;
- run E2E smoke with ESP32 or emulator.

Required before stable relay control from backend:
- verify documented relay command retrieval and acknowledgement endpoints with ESP32/emulator;
- verify resend behavior for commands stuck in `Sent`: retry after 1 minute, max 3 attempts, before `ExpireAt`;
- provide a device-facing config endpoint or documented manual provisioning process;

Optional later:
- MQTT command channel;
- SignalR/gRPC streaming channel;
- OTA firmware update service;
- signed firmware/config;
- encrypted secrets at rest.

## First MCU Smoke Checklist

Backend checklist:
- `device-api` reachable from ESP32 network as `http://<host-ip>:5237`;
- controller exists in backend;
- `DeviceToken` copied into firmware config;
- aquarium/ecosystem exists for the controller and is synced into `TelemetryService`;
- sensors exist in backend;
- backend `sensorId` values copied into firmware config;
- telemetry batch-size validator accepts `1..50`;
- telemetry `IsAggregated` indexes are non-unique in the applied database schema;
- backend accepts `value: 0`;
- backend validation text for `externalMessageId`, `RecordedAt`, `SensorId`, `Items` and batch size is documented for logs;
- direct `device-api`, not gateway, is used for telemetry and command polling unless firmware also sends JWT;
- RabbitMQ and service databases running.

Firmware checklist:
- ESP32 connects to Wi-Fi;
- DS1307 returns valid time or NTP sync works;
- relays start in safe state;
- LCD1602 shows `ONLINE` or `OFFLINE`;
- LEDs reflect state;
- KY-040 changes pages/menu items without blocking telemetry;
- ordinary push button is debounced and mapped to the configured UI action;
- XKC-Y25-V reports water level and triggers safety state when unsafe;
- I2C soil moisture sensor reports calibrated moisture or clear `E007`;
- AHT20 reports humidity and temperature;
- DS18B20 water and surface probes report valid readings;
- telemetry queue receives readings;
- HTTP request includes `X-Device-Token`;
- backend returns `202 Accepted`.

Pass criteria:
- at least one telemetry item is saved in `TelemetryService`;
- backend response has `acceptedCount > 0`;
- firmware logs `ONLINE`;
- LCD1602 shows no critical error;
- no relay toggles unexpectedly.
