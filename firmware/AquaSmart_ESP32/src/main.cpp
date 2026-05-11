#include <Arduino.h>
#include <Wire.h>

#include "ApiClient.h"
#include "ConfigStore.h"
#include "DisplayManager.h"
#include "InputManager.h"
#include "LedIndicator.h"
#include "RelayManager.h"
#include "RtcClock.h"
#include "SensorManager.h"
#include "TelemetryQueue.h"
#include "UiController.h"
#include "WiFiManagerEx.h"

namespace {
DeviceConfig deviceConfig{};
BackendRuntimeConfig runtimeConfig{};
SystemState systemState = SystemState::BOOT;
ErrorCode lastError = ErrorCode::NONE;

RelayManager relayManager;
SensorManager sensorManager;
TelemetryQueue telemetryQueue;
WiFiManagerEx wifiManager;
RtcClock rtcClock;
ApiClient apiClient;
DisplayManager displayManager;
InputManager inputManager;
LedIndicator ledIndicator;
UiController uiController;

bool apiHealthy = false;
bool runtimeConfigLoaded = false;
bool pulseActivity = false;
uint32_t lastTelemetryAttemptAtMs = 0;
uint32_t lastCommandPollAtMs = 0;
uint32_t sequenceNumber = 0;
String runtimeMacAddress;
String normalizedMacAddress;
String commandHistory[MAX_COMMAND_HISTORY];
size_t commandHistoryCount = 0;
}

String normalizeMac(String macAddress) {
    macAddress.replace(":", "");
    macAddress.toUpperCase();
    return macAddress;
}

String safeLocalKey(const SensorRuntimeConfig& sensor) {
    if (sensor.localKey.length() > 0) {
        return sensor.localKey;
    }

    String key = sensor.name;
    key.toLowerCase();
    key.replace(" ", "_");
    return key.length() > 0 ? key : "sensor";
}

String validationHint(const String& validationError) {
    const String lowered = validationError;
    if (lowered.indexOf("ExternalMessageId") >= 0) {
        return "E006 EXTID";
    }
    if (lowered.indexOf("RecordedAt") >= 0) {
        return "E006 TIME";
    }
    if (lowered.indexOf("SensorId") >= 0) {
        return "E006 SID";
    }
    if (lowered.indexOf("Items") >= 0) {
        return "E006 SIZE";
    }
    if (lowered.indexOf("MacAddress") >= 0) {
        return "E006 MAC";
    }
    return "E006 VALID";
}

void setError(ErrorCode errorCode, const String& message) {
    lastError = errorCode;
    Serial.printf("[ERR] %d %s\n", static_cast<int>(errorCode), message.c_str());
}

void clearError(ErrorCode errorCode) {
    if (lastError == errorCode) {
        lastError = ErrorCode::NONE;
    }
}

void changeState(SystemState nextState) {
    if (systemState == nextState) {
        return;
    }

    Serial.printf("[STATE] %d -> %d\n", static_cast<int>(systemState), static_cast<int>(nextState));
    systemState = nextState;
}

TelemetryRecord buildTelemetryRecord(const SensorRuntimeConfig& sensor, const String& timestamp) {
    TelemetryRecord record;
    record.sensorId = sensor.sensorId;
    record.localSensorKey = safeLocalKey(sensor);
    record.value = sensor.lastValue;
    record.recordedAt = timestamp;
    record.critical = sensor.role == SensorRole::WATER_LEVEL || sensor.role == SensorRole::WATER_TEMPERATURE;
    record.externalMessageId = normalizedMacAddress + "-" +
                               String(deviceConfig.bootCounter) + "-" +
                               String(++sequenceNumber) + "-" +
                               record.localSensorKey;

    if (record.externalMessageId.length() > 100) {
        const int allowed = 100 - (normalizedMacAddress.length() + 1 + String(deviceConfig.bootCounter).length() + 1 + String(sequenceNumber).length() + 1);
        if (allowed > 0 && allowed < record.localSensorKey.length()) {
            record.localSensorKey = record.localSensorKey.substring(0, allowed);
            record.externalMessageId = normalizedMacAddress + "-" +
                                       String(deviceConfig.bootCounter) + "-" +
                                       String(sequenceNumber) + "-" +
                                       record.localSensorKey;
        }
    }

    return record;
}

bool wasCommandProcessed(const String& commandId) {
    for (size_t index = 0; index < commandHistoryCount; ++index) {
        if (commandHistory[index] == commandId) {
            return true;
        }
    }

    return false;
}

void rememberProcessedCommand(const String& commandId) {
    if (wasCommandProcessed(commandId)) {
        return;
    }

    if (commandHistoryCount < MAX_COMMAND_HISTORY) {
        commandHistory[commandHistoryCount++] = commandId;
        return;
    }

    for (size_t index = 1; index < MAX_COMMAND_HISTORY; ++index) {
        commandHistory[index - 1] = commandHistory[index];
    }
    commandHistory[MAX_COMMAND_HISTORY - 1] = commandId;
}

void queueSensorTelemetry() {
    if (!rtcClock.isTimeValid()) {
        return;
    }

    ErrorCode queueError = ErrorCode::NONE;
    for (size_t index = 0; index < runtimeConfig.sensorCount; ++index) {
        auto& sensor = runtimeConfig.sensors[index];
        if (!sensor.enabled || !sensor.hasValue || sensor.hasError || sensor.sensorId.length() == 0) {
            continue;
        }

        if (sensor.lastQueuedAtMs == sensor.lastPollAtMs) {
            continue;
        }

        TelemetryRecord record = buildTelemetryRecord(sensor, rtcClock.getIsoTimestampUtc());
        if (!telemetryQueue.enqueue(record, queueError)) {
            if (queueError != ErrorCode::NONE) {
                setError(queueError, "Telemetry queue overflow");
            }
            continue;
        }

        sensor.lastQueuedAtMs = sensor.lastPollAtMs;
    }
}

void applySafetyRules() {
    const SensorSnapshot& snapshot = sensorManager.getSnapshot();

    bool safeOverride = false;
    if (!snapshot.waterLevelSafe) {
        safeOverride = true;

        const uint8_t pumpChannel = relayManager.getChannelByPurpose(RelayPurpose::PUMP);
        const uint8_t heaterChannel = relayManager.getChannelByPurpose(RelayPurpose::HEATING);
        const uint8_t boilerChannel = relayManager.getChannelByPurpose(RelayPurpose::BOILER);

        if (pumpChannel > 0) {
            relayManager.setRelayByChannel(pumpChannel, false);
        }
        if (heaterChannel > 0) {
            relayManager.setRelayByChannel(heaterChannel, false);
        }
        if (boilerChannel > 0) {
            relayManager.setRelayByChannel(boilerChannel, false);
        }
    }

    if (!snapshot.hasWaterTemperature) {
        const uint8_t heaterChannel = relayManager.getChannelByPurpose(RelayPurpose::HEATING);
        const uint8_t boilerChannel = relayManager.getChannelByPurpose(RelayPurpose::BOILER);
        if (heaterChannel > 0) {
            relayManager.setRelayByChannel(heaterChannel, false);
        }
        if (boilerChannel > 0) {
            relayManager.setRelayByChannel(boilerChannel, false);
        }
    }

    relayManager.setSafeOverride(safeOverride);
}

void processPendingCommands() {
    if (!wifiManager.isConnected() || strlen(deviceConfig.controllerId) == 0) {
        return;
    }

    if (millis() - lastCommandPollAtMs < deviceConfig.commandPollIntervalMs) {
        return;
    }
    lastCommandPollAtMs = millis();

    PendingCommand commands[8];
    size_t commandCount = 0;
    ApiResult result = apiClient.fetchPendingCommands(commands, 8, commandCount, deviceConfig.controllerId);
    if (!result.success) {
        setError(ErrorCode::E005_BACKEND_UNAVAILABLE, "Command poll failed: " + result.message);
        apiHealthy = false;
        return;
    }

    for (size_t index = 0; index < commandCount; ++index) {
        if (wasCommandProcessed(commands[index].id)) {
            apiClient.completeCommand(commands[index].id);
            continue;
        }

        const bool desiredState = commands[index].action == 1;
        if (!relayManager.setRelayById(commands[index].relayId, desiredState)) {
            apiClient.failCommand(commands[index].id, "Relay mapping not found");
            continue;
        }

        ApiResult completeResult = apiClient.completeCommand(commands[index].id);
        if (!completeResult.success) {
            setError(ErrorCode::E005_BACKEND_UNAVAILABLE, "Command ack failed: " + completeResult.message);
            continue;
        }

        rememberProcessedCommand(commands[index].id);
        apiHealthy = true;
        pulseActivity = true;
    }
}

void sendQueuedTelemetry() {
    if (!wifiManager.isConnected() || !rtcClock.isTimeValid() || telemetryQueue.count() == 0) {
        return;
    }

    const uint32_t sendInterval = runtimeConfigLoaded ? runtimeConfig.sendIntervalMs : deviceConfig.sendIntervalMs;
    if (millis() - lastTelemetryAttemptAtMs < sendInterval) {
        return;
    }
    lastTelemetryAttemptAtMs = millis();

    TelemetryRecord batch[50];
    const size_t maxBatchSize = runtimeConfigLoaded && runtimeConfig.maxBatchSize < deviceConfig.maxBatchSize
        ? runtimeConfig.maxBatchSize
        : deviceConfig.maxBatchSize;
    const size_t batchSize = telemetryQueue.peekBatch(batch, maxBatchSize);
    if (batchSize == 0) {
        return;
    }

    ApiTelemetryResponse telemetryResponse;
    ApiResult result = apiClient.sendTelemetry(batch, batchSize, telemetryResponse);
    if (!result.success) {
        setError(ErrorCode::E005_BACKEND_UNAVAILABLE, "Telemetry failed: " + result.message);
        apiHealthy = false;
        return;
    }

    telemetryQueue.removeAccepted(telemetryResponse.acceptedCount);
    apiHealthy = true;
    pulseActivity = true;
    clearError(ErrorCode::E005_BACKEND_UNAVAILABLE);

    if (telemetryResponse.validationErrorCount > 0) {
        setError(ErrorCode::E006_BACKEND_VALIDATION, validationHint(telemetryResponse.validationErrors[0]));
        for (size_t index = 0; index < telemetryResponse.validationErrorCount; ++index) {
            Serial.println(telemetryResponse.validationErrors[index]);
        }
    } else {
        clearError(ErrorCode::E006_BACKEND_VALIDATION);
    }
}

void loadRuntimeConfig() {
    ApiResult result = apiClient.fetchRuntimeConfig(runtimeConfig);
    if (!result.success) {
        setError(ErrorCode::E005_BACKEND_UNAVAILABLE, "Config fetch failed: " + result.message);
        apiHealthy = false;
        return;
    }

    runtimeConfigLoaded = true;
    sensorManager.applyRuntimeConfig(runtimeConfig, deviceConfig);
    relayManager.applyRuntimeConfig(runtimeConfig, deviceConfig);
    apiHealthy = true;
    clearError(ErrorCode::E005_BACKEND_UNAVAILABLE);
}

void initializeRuntimeIdentity() {
    runtimeMacAddress = wifiManager.macAddress();
    if (runtimeMacAddress.length() == 0) {
        runtimeMacAddress = "00:00:00:00:00:00";
    }

    normalizedMacAddress = normalizeMac(runtimeMacAddress);
    apiClient.setMacAddress(runtimeMacAddress);
}

void setup() {
    Serial.begin(115200);
    delay(200);
    Serial.println("[BOOT] AquaSmart firmware");

    changeState(SystemState::LOAD_CONFIG);

    Wire.begin(PIN_I2C_SDA, PIN_I2C_SCL);
    ConfigStore::begin();
    ConfigStore::load(deviceConfig);
    ConfigStore::save(deviceConfig);

    changeState(SystemState::INIT_HARDWARE);

    relayManager.begin(deviceConfig);
    telemetryQueue.begin(deviceConfig.maxQueueSize);
    displayManager.begin();
    inputManager.begin();
    ledIndicator.begin();
    uiController.begin();
    wifiManager.begin(deviceConfig);
    initializeRuntimeIdentity();
    apiClient.begin(deviceConfig);

    if (!sensorManager.begin(deviceConfig)) {
        setError(ErrorCode::E007_SENSOR_READ_FAILED, "No sensors initialized");
    }

    if (!rtcClock.begin()) {
        setError(ErrorCode::E003_RTC_UNAVAILABLE, "RTC not available");
    }

    if (strlen(deviceConfig.deviceToken) == 0) {
        setError(ErrorCode::E010_DEVICE_TOKEN_MISSING, "Missing device token");
        changeState(SystemState::SAFE_MODE);
    } else if (!ConfigStore::isValid(deviceConfig)) {
        setError(ErrorCode::E001_CONFIG_INVALID, "Invalid local config");
        changeState(SystemState::SAFE_MODE);
    } else {
        changeState(SystemState::CONNECT_WIFI);
    }
}

void loop() {
    pulseActivity = false;

    bool toggleRelayRequested = false;
    uiController.handleInput(inputManager.update(), runtimeConfig, toggleRelayRequested);
    if (toggleRelayRequested && runtimeConfig.relayCount > 0) {
        relayManager.toggleRelayByChannel(static_cast<uint8_t>(uiController.selectedRelayIndex() + 1));
        pulseActivity = true;
    }

    if (systemState == SystemState::SAFE_MODE) {
        relayManager.setAllSafe();
    }

    if (systemState == SystemState::CONNECT_WIFI || !wifiManager.isConnected()) {
        wifiManager.update(systemState, lastError);
    }

    if (wifiManager.isConnected()) {
        changeState(SystemState::SYNC_TIME);
    }

    if (systemState == SystemState::SYNC_TIME || systemState == SystemState::ONLINE || systemState == SystemState::DEGRADED_OFFLINE) {
        rtcClock.update(lastError, wifiManager.isConnected());
    }

    if (wifiManager.isConnected() && !runtimeConfigLoaded) {
        loadRuntimeConfig();
        if (!runtimeConfigLoaded) {
            changeState(SystemState::DEGRADED_OFFLINE);
        }
    }

    if (runtimeConfigLoaded) {
        sensorManager.update(lastError);
        queueSensorTelemetry();
        applySafetyRules();
    }

    if (wifiManager.isConnected() && rtcClock.isTimeValid() && runtimeConfigLoaded) {
        changeState(SystemState::ONLINE);
        sendQueuedTelemetry();
        processPendingCommands();
    } else if (systemState != SystemState::SAFE_MODE) {
        changeState(SystemState::DEGRADED_OFFLINE);
    }

    SensorSnapshot snapshot = sensorManager.getSnapshot();
    snapshot.rtcAvailable = rtcClock.isRtcAvailable();

    displayManager.render(uiController.currentPage(),
                          systemState,
                          lastError,
                          snapshot,
                          &telemetryQueue,
                          runtimeConfig,
                          uiController.selectedSensorIndex(),
                          uiController.selectedRelayIndex(),
                          wifiManager.isConnected(),
                          apiHealthy,
                          rtcClock.getDisplayTime());

    ledIndicator.update(systemState, wifiManager.isConnected(), apiHealthy, pulseActivity);
    delay(20);
}
