#ifndef AQUA_CORE_TYPES_H
#define AQUA_CORE_TYPES_H

#include <cstddef>
#include <cstdint>
#include <cstring>

constexpr size_t MAX_SENSOR_COUNT = 12;
constexpr size_t MAX_RELAY_COUNT = 8;
constexpr size_t MAX_QUEUE_CAPACITY = 200;
constexpr size_t MAX_MQTT_BATCH_SIZE = 20;
constexpr size_t MAX_HTTP_BATCH_SIZE = 50;
constexpr size_t MQTT_PACKET_BUFFER_SIZE = 4096;
constexpr size_t MAX_EXTERNAL_MESSAGE_ID_LEN = 100;
constexpr size_t MAX_VALIDATION_ERRORS = 8;
constexpr size_t MAX_COMMAND_HISTORY = 16;
constexpr size_t MAX_DS18_ADDRESS_LENGTH = 24;
constexpr size_t MAX_ACTIVE_ERRORS = 12;

constexpr uint32_t SELECT_LONG_PRESS_MS = 1200;
constexpr uint32_t BACK_FACTORY_RESET_HOLD_MS = 5000;

constexpr uint8_t CORE_RELAY_PINS[MAX_RELAY_COUNT] = {16, 17, 18, 19, 25, 26, 27, 32};

constexpr char FIRMWARE_VERSION[] = "1.0.0";

constexpr char DEFAULT_TELEMETRY_PATH[] = "/api/telemetry/v1/data";
constexpr char DEFAULT_CONFIG_PATH[] = "/api/device/v1/controllers/me/config";
constexpr char DEFAULT_COMMANDS_PATH_PREFIX[] = "/api/device/v1/commands/pending/";
constexpr char DEFAULT_COMMAND_COMPLETE_SUFFIX[] = "/complete";
constexpr char DEFAULT_COMMAND_FAIL_SUFFIX[] = "/fail";
constexpr char DEFAULT_OTA_CHECK_PATH[] = "/api/firmware/v1/ota/check";
constexpr char DEFAULT_OTA_STATUS_PATH[] = "/api/firmware/v1/ota/status";

enum class SystemState : uint8_t {
    BOOT,
    LOAD_CONFIG,
    INIT_HARDWARE,
    CONNECT_WIFI,
    SYNC_TIME,
    CONNECT_MQTT,
    ONLINE,
    DEGRADED_OFFLINE,
    SAFE_MODE,
    OTA_UPDATING,
    FACTORY_RESET
};

enum class ErrorCode : uint8_t {
    NONE = 0,
    E001_CONFIG_INVALID = 1,
    E002_WIFI_FAILED = 2,
    E003_RTC_UNAVAILABLE = 3,
    E004_TIME_INVALID = 4,
    E005_BACKEND_UNAVAILABLE = 5,
    E006_BACKEND_VALIDATION = 6,
    E007_SENSOR_READ_FAILED = 7,
    E008_RELAY_INIT_FAILED = 8,
    E009_QUEUE_OVERFLOW = 9,
    E010_DEVICE_TOKEN_MISSING = 10,
    E011_MQTT_DISCONNECTED = 11,
    E012_OTA_FAILED = 12
};

enum class InputEventType : uint8_t {
    NONE,
    NEXT,
    PREVIOUS,
    SELECT,
    BACK,
    FACTORY_RESET
};

struct InputEvent {
    InputEventType type = InputEventType::NONE;
    bool longPress = false;

    constexpr InputEvent() = default;
    constexpr InputEvent(InputEventType t, bool isLongPress = false)
        : type(t), longPress(isLongPress) {}
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

enum class RelayChangeSource : uint8_t {
    BOOT = 0,
    MANUAL = 1,
    SERVER = 2,
    SYNC = 3
};

struct DeviceConfig {
    char wifiSsid[32] = {};
    char wifiPass[64] = {};
    char apiBaseUrl[128] = {};
    char deviceToken[128] = {};
    char controllerId[40] = {};
    char telemetryPath[64] = {};
    char mqttHost[64] = {};
    uint16_t mqttPort = 1883;
    bool mqttUseTls = false;
    uint16_t mqttKeepAliveSec = 30;
    uint16_t mqttHeartbeatIntervalSec = 60;
    uint16_t maxMqttBatchSize = MAX_MQTT_BATCH_SIZE;
    char pendingOtaCampaignId[40] = {};
    char pendingOtaVersion[24] = {};
    char ds18WaterAddress[MAX_DS18_ADDRESS_LENGTH] = {};
    char ds18SurfaceAddress[MAX_DS18_ADDRESS_LENGTH] = {};
    uint32_t sendIntervalMs = 10000;
    uint16_t maxBatchSize = MAX_HTTP_BATCH_SIZE;
    uint16_t maxQueueSize = MAX_QUEUE_CAPACITY;
    uint32_t commandPollIntervalMs = 3000;
    uint32_t bootCounter = 0;
    uint16_t soilDryCalibration = 3200;
    uint16_t soilWetCalibration = 1400;
    uint16_t waterLevelDebounceMs = 500;
    bool relayActiveLow[MAX_RELAY_COUNT] = {true, true, true, true, true, true, true, true};
    bool relaySafeState[MAX_RELAY_COUNT] = {false, false, false, false, false, false, false, false};
};

struct SensorRuntimeConfig {
    bool enabled = false;
    SensorRole role = SensorRole::UNKNOWN;
    int connectionProtocol = 0;
    int sensorType = 0;
    char sensorId[40] = {};
    char name[32] = {};
    char connectionAddress[32] = {};
    char unit[12] = {};
    char localKey[32] = {};
    uint32_t pollIntervalMs = 5000;
    uint32_t lastPollAtMs = 0;
    uint32_t lastQueuedAtMs = 0;
    float lastValue = 0.0f;
    float lastRawValue = 0.0f;
    bool hasValue = false;
    bool hasError = false;
};

struct RelayRuntimeConfig {
    bool configured = false;
    uint8_t channel = 0;  // 1..8
    uint8_t pin = 0;
    bool activeLow = true;
    bool safeState = false;
    bool currentState = false;
    bool isManual = true;
    bool backendState = false;
    bool pendingSync = false;
    RelayChangeSource lastChangeSource = RelayChangeSource::BOOT;
    RelayPurpose purpose = RelayPurpose::RESERVE;
    char relayId[40] = {};
    char name[32] = {};
    char connectionAddress[16] = {};
};

struct BackendRuntimeConfig {
    bool isLoaded = false;
    uint64_t configVersion = 0;
    uint32_t sendIntervalMs = 10000;
    uint16_t maxBatchSize = MAX_HTTP_BATCH_SIZE;
    SensorRuntimeConfig sensors[MAX_SENSOR_COUNT] = {};
    size_t sensorCount = 0;
    RelayRuntimeConfig relays[MAX_RELAY_COUNT] = {};
    size_t relayCount = 0;
};

// Strictly fixed char[] layout — zero Arduino String allocations.
struct TelemetryRecord {
    bool occupied = false;
    bool critical = false;
    char sensorId[37] = {};
    char localSensorKey[32] = {};
    char externalMessageId[MAX_EXTERNAL_MESSAGE_ID_LEN + 1] = {};
    char recordedAt[25] = {};
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

struct ErrorEntry {
    ErrorCode code = ErrorCode::NONE;
    bool critical = false;
    char detail[24] = {};
};

class ErrorRegistry {
public:
    void clearAll() { _count = 0; }

    void set(ErrorCode code, const char* detail = "", bool critical = false) {
        if (code == ErrorCode::NONE) {
            return;
        }
        for (size_t i = 0; i < _count; ++i) {
            if (_entries[i].code == code) {
                _entries[i].critical = critical;
                copySafe(_entries[i].detail, sizeof(_entries[i].detail), detail);
                return;
            }
        }
        if (_count < MAX_ACTIVE_ERRORS) {
            _entries[_count].code = code;
            _entries[_count].critical = critical;
            copySafe(_entries[_count].detail, sizeof(_entries[_count].detail), detail);
            ++_count;
        }
    }

    void clear(ErrorCode code) {
        for (size_t i = 0; i < _count; ++i) {
            if (_entries[i].code == code) {
                for (size_t j = i; j + 1 < _count; ++j) {
                    _entries[j] = _entries[j + 1];
                }
                --_count;
                return;
            }
        }
    }

    void clearNonCritical() {
        size_t writeIdx = 0;
        for (size_t i = 0; i < _count; ++i) {
            if (_entries[i].critical) {
                _entries[writeIdx++] = _entries[i];
            }
        }
        _count = writeIdx;
    }

    size_t count() const { return _count; }

    const ErrorEntry* get(size_t index) const {
        return index < _count ? &_entries[index] : nullptr;
    }

    ErrorCode latest() const {
        return _count > 0 ? _entries[_count - 1].code : ErrorCode::NONE;
    }

    static const char* codeLabel(ErrorCode code) {
        switch (code) {
            case ErrorCode::E001_CONFIG_INVALID: return "E001";
            case ErrorCode::E002_WIFI_FAILED: return "E002";
            case ErrorCode::E003_RTC_UNAVAILABLE: return "E003";
            case ErrorCode::E004_TIME_INVALID: return "E004";
            case ErrorCode::E005_BACKEND_UNAVAILABLE: return "E005";
            case ErrorCode::E006_BACKEND_VALIDATION: return "E006";
            case ErrorCode::E007_SENSOR_READ_FAILED: return "E007";
            case ErrorCode::E008_RELAY_INIT_FAILED: return "E008";
            case ErrorCode::E009_QUEUE_OVERFLOW: return "E009";
            case ErrorCode::E010_DEVICE_TOKEN_MISSING: return "E010";
            case ErrorCode::E011_MQTT_DISCONNECTED: return "E011";
            case ErrorCode::E012_OTA_FAILED: return "E012";
            default: return "NONE";
        }
    }

    static const char* defaultMessage(ErrorCode code) {
        switch (code) {
            case ErrorCode::E001_CONFIG_INVALID: return "Config invalid";
            case ErrorCode::E002_WIFI_FAILED: return "WiFi failed";
            case ErrorCode::E003_RTC_UNAVAILABLE: return "RTC unavailable";
            case ErrorCode::E004_TIME_INVALID: return "Time invalid";
            case ErrorCode::E005_BACKEND_UNAVAILABLE: return "HTTP unavailable";
            case ErrorCode::E006_BACKEND_VALIDATION: return "Validation err";
            case ErrorCode::E007_SENSOR_READ_FAILED: return "Sensor read fail";
            case ErrorCode::E008_RELAY_INIT_FAILED: return "Relay init fail";
            case ErrorCode::E009_QUEUE_OVERFLOW: return "Queue overflow";
            case ErrorCode::E010_DEVICE_TOKEN_MISSING: return "Token missing";
            case ErrorCode::E011_MQTT_DISCONNECTED: return "MQTT disconn";
            case ErrorCode::E012_OTA_FAILED: return "OTA failed";
            default: return "No active errors";
        }
    }

private:
    ErrorEntry _entries[MAX_ACTIVE_ERRORS] = {};
    size_t _count = 0;

    static void copySafe(char* dst, size_t dstSize, const char* src) {
        if (dst == nullptr || dstSize == 0) {
            return;
        }
        if (src == nullptr) {
            dst[0] = '\0';
            return;
        }
        std::strncpy(dst, src, dstSize - 1);
        dst[dstSize - 1] = '\0';
    }
};

inline void copyCString(char* dst, size_t dstSize, const char* src) {
    if (dst == nullptr || dstSize == 0) {
        return;
    }
    if (src == nullptr) {
        dst[0] = '\0';
        return;
    }
    std::strncpy(dst, src, dstSize - 1);
    dst[dstSize - 1] = '\0';
}

#endif
