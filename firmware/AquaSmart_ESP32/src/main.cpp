#include <Arduino.h>
#include <Wire.h>

#include <ProtocolCodecs.h>
#include <TelemetryQueue.h>
#include <UiController.h>

#include "ApiClient.h"
#include "ConfigStore.h"
#include "DisplayManager.h"
#include "InputManager.h"
#include "LedIndicator.h"
#include "RelayManager.h"
#include "RtcClock.h"
#include "SensorManager.h"
#include "WiFiManagerEx.h"

namespace {
DeviceConfig deviceConfig{};
BackendRuntimeConfig runtimeConfig{};
SystemState systemState = SystemState::BOOT;
ErrorCode lastError = ErrorCode::NONE;
ErrorRegistry errorRegistry;

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
int lastHttpStatus = 0;
bool runtimeConfigLoaded = false;
bool pulseActivity = false;
uint32_t lastTelemetryAttemptAtMs = 0;
uint32_t lastCommandPollAtMs = 0;
uint32_t sequenceNumber = 0;
uint32_t telemetrySentCount = 0;
char runtimeMacCanonical[18] = "00:00:00:00:00:00";
char runtimeMacCompact[13] = "000000000000";
}  // namespace

void setError(ErrorCode errorCode, const char* message, bool critical = false) {
    lastError = errorCode;
    errorRegistry.set(errorCode, message, critical);
    Serial.printf("[ERR] %s %s\n", ErrorRegistry::codeLabel(errorCode), message);
}

void clearError(ErrorCode errorCode) {
    errorRegistry.clear(errorCode);
    lastError = errorRegistry.latest();
}

void changeState(SystemState nextState) {
    if (systemState == nextState) {
        return;
    }

    Serial.printf("[STATE] %d -> %d\n", static_cast<int>(systemState), static_cast<int>(nextState));
    systemState = nextState;
}

TelemetryRecord buildTelemetryRecord(const SensorRuntimeConfig& sensor, const String& timestamp) {
    TelemetryRecord record{};
    copyCString(record.sensorId, sizeof(record.sensorId), sensor.sensorId);
    const char* key = sensor.localKey[0] != '\0' ? sensor.localKey : "sensor";
    copyCString(record.localSensorKey, sizeof(record.localSensorKey), key);
    record.value = sensor.lastValue;
    copyCString(record.recordedAt, sizeof(record.recordedAt), timestamp.c_str());
    record.critical = TelemetryQueue::isCriticalRole(sensor.role);

    TelemetryQueue::formatExternalMessageId(
        record.externalMessageId,
        sizeof(record.externalMessageId),
        runtimeMacCompact,
        deviceConfig.bootCounter,
        ++sequenceNumber,
        record.localSensorKey);

    return record;
}

void queueSensorTelemetry() {
    if (!rtcClock.isTimeValid()) {
        return;
    }

    ErrorCode queueError = ErrorCode::NONE;
    for (size_t index = 0; index < runtimeConfig.sensorCount; ++index) {
        auto& sensor = runtimeConfig.sensors[index];
        if (!sensor.enabled || !sensor.hasValue || sensor.hasError || sensor.sensorId[0] == '\0') {
            continue;
        }

        if (sensor.lastQueuedAtMs == sensor.lastPollAtMs) {
            continue;
        }

        TelemetryRecord record = buildTelemetryRecord(sensor, rtcClock.getIsoTimestampUtc());
        if (!telemetryQueue.enqueue(record, queueError)) {
            if (queueError != ErrorCode::NONE) {
                setError(queueError, "Queue overflow");
            }
            continue;
        }
        if (queueError == ErrorCode::E009_QUEUE_OVERFLOW) {
            setError(queueError, "Queue evicted");
            queueError = ErrorCode::NONE;
        }

        sensor.lastQueuedAtMs = sensor.lastPollAtMs;
    }
}

void sendQueuedTelemetry(bool forceFlush = false) {
    if (!wifiManager.isConnected() || !rtcClock.isTimeValid() || telemetryQueue.count() == 0) {
        return;
    }

    const uint32_t sendInterval = runtimeConfigLoaded ? runtimeConfig.sendIntervalMs : deviceConfig.sendIntervalMs;
    if (!forceFlush && (millis() - lastTelemetryAttemptAtMs < sendInterval)) {
        return;
    }
    lastTelemetryAttemptAtMs = millis();

    TelemetryRecord batch[MAX_HTTP_BATCH_SIZE];
    const size_t configuredMax = (runtimeConfigLoaded && runtimeConfig.maxBatchSize > 0)
        ? runtimeConfig.maxBatchSize
        : deviceConfig.maxBatchSize;
    const size_t maxBatchSize = TelemetryQueue::computeBatchLimit(configuredMax, false);
    const size_t batchSize = telemetryQueue.peekBatch(batch, maxBatchSize);
    if (batchSize == 0) {
        return;
    }

    ApiTelemetryResponse telemetryResponse;
    ApiResult result = apiClient.sendTelemetry(batch, batchSize, telemetryResponse);
    lastHttpStatus = result.statusCode;
    if (!result.success) {
        setError(ErrorCode::E005_BACKEND_UNAVAILABLE, "Telemetry failed");
        apiHealthy = false;
        return;
    }

    telemetryQueue.removeProcessed(
        static_cast<size_t>(telemetryResponse.acceptedCount),
        static_cast<size_t>(telemetryResponse.skippedCount));
    telemetrySentCount += static_cast<uint32_t>(telemetryResponse.acceptedCount);
    apiHealthy = true;
    pulseActivity = true;
    clearError(ErrorCode::E005_BACKEND_UNAVAILABLE);

    if (telemetryResponse.validationErrorCount > 0) {
        setError(ErrorCode::E006_BACKEND_VALIDATION, "Validation err");
    } else {
        clearError(ErrorCode::E006_BACKEND_VALIDATION);
    }
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
    lastHttpStatus = result.statusCode;
    if (!result.success) {
        setError(ErrorCode::E005_BACKEND_UNAVAILABLE, "Command poll fail");
        apiHealthy = false;
        return;
    }

    int64_t nowEpoch = 0;
    if (rtcClock.isTimeValid()) {
        ProtocolCodecs::parseIso8601UtcToEpoch(rtcClock.getIsoTimestampUtc().c_str(), nowEpoch);
    }

    for (size_t index = 0; index < commandCount; ++index) {
        int64_t expireEpoch = 0;
        if (commands[index].expireAt.length() > 0) {
            ProtocolCodecs::parseIso8601UtcToEpoch(commands[index].expireAt.c_str(), expireEpoch);
        }

        const bool desiredState = commands[index].action == 1;
        char errBuf[64] = {};
        const CommandApplyStatus status = relayManager.applyServerCommand(
            commands[index].id.c_str(),
            commands[index].relayId.c_str(),
            desiredState,
            expireEpoch,
            nowEpoch,
            errBuf,
            sizeof(errBuf));

        if (status == CommandApplyStatus::APPLIED || status == CommandApplyStatus::DUPLICATE) {
            ApiResult completeResult = apiClient.completeCommand(commands[index].id);
            if (!completeResult.success) {
                setError(ErrorCode::E005_BACKEND_UNAVAILABLE, "Command ack fail");
                continue;
            }
            apiHealthy = true;
            pulseActivity = true;
        } else {
            apiClient.failCommand(commands[index].id, errBuf[0] != '\0' ? String(errBuf) : String("Command failed"));
        }
    }
}

void loadRuntimeConfig() {
    ApiResult result = apiClient.fetchRuntimeConfig(runtimeConfig);
    lastHttpStatus = result.statusCode;
    if (!result.success) {
        setError(ErrorCode::E005_BACKEND_UNAVAILABLE, "Config fetch fail");
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
    String rawMac = wifiManager.macAddress();
    if (!ProtocolCodecs::normalizeMacCanonical(rawMac.c_str(), runtimeMacCanonical, sizeof(runtimeMacCanonical))) {
        copyCString(runtimeMacCanonical, sizeof(runtimeMacCanonical), "00:00:00:00:00:00");
    }
    if (!ProtocolCodecs::normalizeMacCompact(runtimeMacCanonical, runtimeMacCompact, sizeof(runtimeMacCompact))) {
        copyCString(runtimeMacCompact, sizeof(runtimeMacCompact), "000000000000");
    }
    apiClient.setMacAddress(String(runtimeMacCanonical));
}

void executeUiAction(const UiAction& action) {
    const bool isOnline = (systemState == SystemState::ONLINE);
    switch (action.type) {
        case UiActionType::NONE:
            break;
        case UiActionType::TOGGLE_RELAY_STATE:
            if (relayManager.manualToggle(action.relayChannel, isOnline)) {
                pulseActivity = true;
            }
            break;
        case UiActionType::TOGGLE_RELAY_MODE:
            if (relayManager.manualToggleMode(action.relayChannel, isOnline)) {
                pulseActivity = true;
            }
            break;
        case UiActionType::SYNC_CONFIG:
            if (wifiManager.isConnected()) {
                loadRuntimeConfig();
                pulseActivity = true;
            }
            break;
        case UiActionType::FLUSH_QUEUE:
            sendQueuedTelemetry(true);
            break;
        case UiActionType::CLEAR_NON_CRITICAL_ERRORS:
            errorRegistry.clearNonCritical();
            lastError = errorRegistry.latest();
            break;
        case UiActionType::OTA_CHECK:
            Serial.println("[OTA] Manual OTA Check requested (wired in Stage 6B).");
            break;
        case UiActionType::SAFE_STATE_ALL:
            relayManager.setAllSafe();
            pulseActivity = true;
            break;
        case UiActionType::REBOOT:
            Serial.println("[SYS] Manual reboot requested.");
            delay(200);
            ESP.restart();
            break;
        case UiActionType::FACTORY_RESET:
            Serial.println("[RESET] Factory reset triggered via System -> Factory Reset!");
            changeState(SystemState::FACTORY_RESET);
            ConfigStore::reset();
            delay(500);
            ESP.restart();
            break;
    }
}

void setup() {
    Serial.begin(115200);
    delay(200);
    Serial.printf("[BOOT] AquaSmart firmware v%s\n", FIRMWARE_VERSION);

    changeState(SystemState::LOAD_CONFIG);

    WiFi.mode(WIFI_STA);
    WiFi.disconnect();
    Wire.begin(PIN_I2C_SDA, PIN_I2C_SCL);
    ConfigStore::begin();
    ConfigStore::load(deviceConfig);
    ConfigStore::save(deviceConfig);

    changeState(SystemState::INIT_HARDWARE);

    // Apply safeState to all 8 relay channels BEFORE starting Wi-Fi/network.
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
        setError(ErrorCode::E007_SENSOR_READ_FAILED, "No sensors init");
    }

    if (!rtcClock.begin()) {
        setError(ErrorCode::E003_RTC_UNAVAILABLE, "RTC unavailable");
    }

    if (strlen(deviceConfig.deviceToken) == 0) {
        setError(ErrorCode::E010_DEVICE_TOKEN_MISSING, "Token missing", true);
        changeState(SystemState::SAFE_MODE);
    } else if (!ConfigStore::isValid(deviceConfig)) {
        setError(ErrorCode::E001_CONFIG_INVALID, "Config invalid", true);
        changeState(SystemState::SAFE_MODE);
    } else {
        changeState(SystemState::CONNECT_WIFI);
    }
}

void loop() {
    pulseActivity = false;

    const InputEvent inputEvent = inputManager.update();
    const UiAction action = uiController.handleInput(
        inputEvent,
        runtimeConfig.isLoaded ? runtimeConfig.sensorCount : 0,
        errorRegistry.count());
    executeUiAction(action);

    if (systemState == SystemState::CONNECT_WIFI || !wifiManager.isConnected()) {
        ErrorCode wifiErr = ErrorCode::NONE;
        wifiManager.update(systemState, wifiErr);
        if (wifiErr != ErrorCode::NONE) {
            setError(wifiErr, "WiFi timeout");
        }
    }

    if (systemState == SystemState::CONNECT_WIFI && wifiManager.isConnected()) {
        clearError(ErrorCode::E002_WIFI_FAILED);
        changeState(SystemState::SYNC_TIME);
    }

    if (systemState == SystemState::SYNC_TIME ||
        systemState == SystemState::ONLINE ||
        systemState == SystemState::DEGRADED_OFFLINE) {
        ErrorCode rtcErr = ErrorCode::NONE;
        if (rtcClock.update(rtcErr, wifiManager.isConnected())) {
            clearError(ErrorCode::E003_RTC_UNAVAILABLE);
            clearError(ErrorCode::E004_TIME_INVALID);
        } else if (rtcErr != ErrorCode::NONE) {
            setError(rtcErr, "Time sync fail");
        }
    }

    if (wifiManager.isConnected() && !runtimeConfigLoaded) {
        loadRuntimeConfig();
        if (!runtimeConfigLoaded) {
            changeState(SystemState::DEGRADED_OFFLINE);
        }
    }

    if (runtimeConfigLoaded) {
        ErrorCode sensorErr = ErrorCode::NONE;
        if (!sensorManager.update(sensorErr) && sensorErr != ErrorCode::NONE) {
            setError(sensorErr, "Sensor read fail");
        } else {
            clearError(ErrorCode::E007_SENSOR_READ_FAILED);
        }
        queueSensorTelemetry();
    }

    if (wifiManager.isConnected() && rtcClock.isTimeValid() && runtimeConfigLoaded) {
        changeState(SystemState::ONLINE);
        sendQueuedTelemetry(false);
        processPendingCommands();
    } else if (systemState != SystemState::SAFE_MODE) {
        changeState(SystemState::DEGRADED_OFFLINE);
    }

    LcdViewContext viewCtx{};
    viewCtx.systemState = systemState;
    viewCtx.wifiConnected = wifiManager.isConnected();
    viewCtx.mqttConnected = false;  // Wired in Stage 6B (MqttClientEx)
    viewCtx.httpHealthy = apiHealthy;
    viewCtx.lastHttpStatus = lastHttpStatus;
    viewCtx.wifiRssi = wifiManager.isConnected() ? WiFi.RSSI() : 0;
    copyCString(viewCtx.wifiSsid, sizeof(viewCtx.wifiSsid), deviceConfig.wifiSsid);
    copyCString(viewCtx.ipAddress, sizeof(viewCtx.ipAddress),
                wifiManager.isConnected() ? wifiManager.localIp().toString().c_str() : "0.0.0.0");
    copyCString(viewCtx.mqttHost, sizeof(viewCtx.mqttHost), deviceConfig.mqttHost);
    rtcClock.getShortTime(viewCtx.shortTime, sizeof(viewCtx.shortTime));
    rtcClock.getFullDateTime(viewCtx.fullDateTime, sizeof(viewCtx.fullDateTime));
    viewCtx.timeFromNtp = rtcClock.isSyncedFromNtp();
    viewCtx.timeValid = rtcClock.isTimeValid();
    viewCtx.queueCount = telemetryQueue.count();
    viewCtx.queueCapacity = telemetryQueue.capacity();
    viewCtx.telemetrySentCount = telemetrySentCount;
    viewCtx.snapshot = sensorManager.getSnapshot();
    viewCtx.snapshot.rtcAvailable = rtcClock.isRtcAvailable();
    viewCtx.runtimeConfig = &runtimeConfig;
    viewCtx.relays = relayManager.channels();
    viewCtx.errors = &errorRegistry;

    displayManager.render(uiController, viewCtx);
    ledIndicator.update(systemState, wifiManager.isConnected(), apiHealthy, pulseActivity);
    delay(20);
}
