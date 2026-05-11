#ifndef APP_STATE_H
#define APP_STATE_H

#include <Arduino.h>

constexpr size_t MAX_SENSOR_COUNT = 12;
constexpr size_t MAX_RELAY_COUNT = 8;
constexpr size_t MAX_QUEUE_CAPACITY = 500;
constexpr size_t MAX_VALIDATION_ERRORS = 8;
constexpr size_t MAX_COMMAND_HISTORY = 8;
constexpr size_t MAX_DS18_ADDRESS_LENGTH = 24;

constexpr char DEFAULT_TELEMETRY_PATH[] = "/api/device/v1/sensors/telemetry";
constexpr char DEFAULT_CONFIG_PATH[] = "/api/device/v1/controllers/me/config";
constexpr char DEFAULT_COMMANDS_PATH_PREFIX[] = "/api/device/v1/commands/pending/";
constexpr char DEFAULT_COMMAND_COMPLETE_SUFFIX[] = "/complete";
constexpr char DEFAULT_COMMAND_FAIL_SUFFIX[] = "/fail";

enum class SystemState : uint8_t {
    BOOT,
    LOAD_CONFIG,
    INIT_HARDWARE,
    CONNECT_WIFI,
    SYNC_TIME,
    ONLINE,
    DEGRADED_OFFLINE,
    SAFE_MODE,
    FACTORY_RESET
};

enum class ErrorCode : uint8_t {
    NONE,
    E001_CONFIG_INVALID,
    E002_WIFI_FAILED,
    E003_RTC_UNAVAILABLE,
    E004_TIME_INVALID,
    E005_BACKEND_UNAVAILABLE,
    E006_BACKEND_VALIDATION,
    E007_SENSOR_READ_FAILED,
    E008_RELAY_INIT_FAILED,
    E009_QUEUE_OVERFLOW,
    E010_DEVICE_TOKEN_MISSING
};

enum class DisplayPage : uint8_t {
    STATUS,
    NETWORK,
    QUEUE,
    SENSOR,
    RELAY,
    ERROR
};

enum class InputEventType : uint8_t {
    NONE,
    NEXT,
    PREVIOUS,
    SELECT,
    BACK
};

enum class SensorRole : uint8_t {
    UNKNOWN,
    WATER_TEMPERATURE,
    SURFACE_TEMPERATURE,
    AIR_TEMPERATURE,
    AIR_HUMIDITY,
    WATER_LEVEL,
    SOIL_MOISTURE
};

enum class RelayPurpose : uint8_t {
    FILTER = 0,
    BOILER = 1,
    LIGHT = 2,
    PUMP = 3,
    HEATING = 4,
    RESERVE = 5
};

struct DeviceConfig {
    char wifiSsid[32];
    char wifiPass[64];
    char apiBaseUrl[128];
    char deviceToken[128];
    char controllerId[40];
    char telemetryPath[64];
    char ds18WaterAddress[MAX_DS18_ADDRESS_LENGTH];
    char ds18SurfaceAddress[MAX_DS18_ADDRESS_LENGTH];
    uint32_t sendIntervalMs;
    uint16_t maxBatchSize;
    uint16_t maxQueueSize;
    uint32_t commandPollIntervalMs;
    uint32_t bootCounter;
    uint16_t soilDryCalibration;
    uint16_t soilWetCalibration;
    uint16_t waterLevelDebounceMs;
    bool relaySafeState[MAX_RELAY_COUNT];
};

struct SensorRuntimeConfig {
    bool enabled = false;
    SensorRole role = SensorRole::UNKNOWN;
    int connectionProtocol = 0;
    int sensorType = 0;
    String sensorId;
    String name;
    String connectionAddress;
    String unit;
    String localKey;
    uint32_t pollIntervalMs = 5000;
    uint32_t lastPollAtMs = 0;
    uint32_t lastQueuedAtMs = 0;
    float lastValue = 0.0f;
    float lastRawValue = 0.0f;
    bool hasValue = false;
    bool hasError = false;
};

struct RelayRuntimeConfig {
    bool enabled = false;
    uint8_t channel = 0;
    uint8_t pin = 0;
    bool activeLow = true;
    bool safeState = false;
    bool currentState = false;
    bool isManual = false;
    bool backendState = false;
    RelayPurpose purpose = RelayPurpose::RESERVE;
    String relayId;
    String name;
};

struct BackendRuntimeConfig {
    bool isLoaded = false;
    uint32_t sendIntervalMs = 10000;
    uint16_t maxBatchSize = 50;
    bool hasHeaterRelay = false;
    bool hasPumpRelay = false;
    uint8_t heaterRelayChannel = 0;
    uint8_t pumpRelayChannel = 0;
    SensorRuntimeConfig sensors[MAX_SENSOR_COUNT];
    size_t sensorCount = 0;
    RelayRuntimeConfig relays[MAX_RELAY_COUNT];
    size_t relayCount = 0;
};

struct TelemetryRecord {
    bool occupied = false;
    bool critical = false;
    String sensorId;
    String localSensorKey;
    String externalMessageId;
    String recordedAt;
    float value = 0.0f;
};

struct SensorSnapshot {
    bool waterLevelSafe = true;
    bool hasWaterTemperature = false;
    bool hasSurfaceTemperature = false;
    bool hasAirTemperature = false;
    bool hasHumidity = false;
    bool hasSoilMoisture = false;
    bool ahtAvailable = false;
    bool rtcAvailable = false;
    bool ds18Available = false;
    bool soilAvailable = false;
    float waterTemperature = 0.0f;
    float surfaceTemperature = 0.0f;
    float airTemperature = 0.0f;
    float humidity = 0.0f;
    float soilMoisture = 0.0f;
    float soilMoistureRaw = 0.0f;
};

struct PendingCommand {
    String id;
    String controllerId;
    String relayId;
    int action = 0;
    int status = 0;
    int attemptCount = 0;
    String expireAt;
    String processedAt;
    String errorMessage;
    String createdAt;
};

struct ApiTelemetryResponse {
    int acceptedCount = 0;
    int skippedCount = 0;
    String validationErrors[MAX_VALIDATION_ERRORS];
    size_t validationErrorCount = 0;
};

struct ApiResult {
    bool success;
    int statusCode;
    String message;
    ApiResult(bool s, int c, String m) : success(s), statusCode(c), message(m) {}
};

struct InputEvent {
    InputEventType type;
    bool longPress;
    InputEvent() : type(InputEventType::NONE) {} 
    InputEvent(InputEventType t, bool isLongPress = false) : type(t), longPress(isLongPress) {}
};

#endif
