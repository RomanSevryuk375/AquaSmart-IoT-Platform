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
#include "MqttClientEx.h"
#include "OtaManager.h"
#include "RelayManager.h"
#include "RtcClock.h"
#include "SensorManager.h"
#include "WiFiManagerEx.h"

namespace {
constexpr uint32_t CONFIG_RETRY_INTERVAL_MS = 15000;
constexpr uint32_t OTA_PERIODIC_CHECK_INTERVAL_MS = 1800000;  // 30 minutes
constexpr uint32_t OTA_POSTBOOT_RETRY_INTERVAL_MS = 10000;

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
MqttClientEx mqttClient;
OtaManager otaManager;
DisplayManager displayManager;
InputManager inputManager;
LedIndicator ledIndicator;
UiController uiController;

bool apiHealthy = false;
int lastHttpStatus = 0;
bool runtimeConfigLoaded = false;
bool serverConfigSyncedOnce = false;
bool bootRelaySyncPublished = false;
bool pulseActivity = false;

uint32_t lastConfigAttemptAtMs = 0;
uint32_t lastTelemetryAttemptAtMs = 0;
uint32_t lastCommandPollAtMs = 0;
uint32_t lastHttpPingAtMs = 0;
uint32_t lastOtaCheckAtMs = 0;
uint32_t lastPostBootAttemptAtMs = 0;

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

String currentTimestampUtc() {
    if (rtcClock.isTimeValid()) {
        return rtcClock.getIsoTimestampUtc();
    }
    return String("1970-01-01T00:00:00Z");
}

void publishPendingManualRelays() {
    if (!mqttClient.isConnected() || !relayManager.hasAnyPendingSync()) {
        return;
    }

    RelaySyncItem items[MAX_RELAY_COUNT];
    uint8_t channelsToClear[MAX_RELAY_COUNT];
    size_t count = 0;

    const RelayRuntimeConfig* channels = relayManager.channels();
    for (size_t i = 0; i < MAX_RELAY_COUNT; ++i) {
        if (channels[i].pendingSync && channels[i].configured && channels[i].relayId[0] != '\0') {
            copyCString(items[count].relayId, sizeof(items[count].relayId), channels[i].relayId);
            items[count].state = channels[i].currentState;
            items[count].isManual = channels[i].isManual;
            channelsToClear[count] = channels[i].channel;
            ++count;
        }
    }

    if (count == 0) {
        return;
    }

    const String nowIso = currentTimestampUtc();
    if (mqttClient.publishRelaysState("manual", nowIso.c_str(), items, count)) {
        for (size_t i = 0; i < count; ++i) {
            relayManager.markChannelSynced(channelsToClear[i]);
        }
        pulseActivity = true;
        Serial.printf("[MQTT] Published %u manual relay override(s)\n", static_cast<unsigned>(count));
    }
}

void publishBootRelaysState() {
    if (bootRelaySyncPublished || !mqttClient.isConnected() || !runtimeConfigLoaded) {
        return;
    }

    // Priority 1: any offline manual overrides made prior to MQTT connect win first.
    publishPendingManualRelays();

    // Priority 2: report remaining configured boot physical states without forcing isManual.
    RelaySyncItem items[MAX_RELAY_COUNT];
    size_t count = 0;
    const RelayRuntimeConfig* channels = relayManager.channels();
    for (size_t i = 0; i < MAX_RELAY_COUNT; ++i) {
        if (channels[i].configured &&
            channels[i].relayId[0] != '\0' &&
            !channels[i].pendingSync &&
            channels[i].lastChangeSource == RelayChangeSource::BOOT) {
            copyCString(items[count].relayId, sizeof(items[count].relayId), channels[i].relayId);
            items[count].state = channels[i].currentState;
            items[count].isManual = channels[i].isManual;
            ++count;
        }
    }

    if (count > 0) {
        const String nowIso = currentTimestampUtc();
        if (mqttClient.publishRelaysState("boot", nowIso.c_str(), items, count)) {
            bootRelaySyncPublished = true;
            pulseActivity = true;
            Serial.printf("[MQTT] Published boot state for %u relay(s)\n", static_cast<unsigned>(count));
        }
    } else {
        bootRelaySyncPublished = true;
    }
}

void publishSingleManualRelay(uint8_t channel) {
    if (!mqttClient.isConnected()) {
        return;
    }
    const RelayRuntimeConfig* ch = relayManager.getRelayByIndex(channel > 0 ? channel - 1 : MAX_RELAY_COUNT);
    if (ch == nullptr || !ch->configured || ch->relayId[0] == '\0') {
        return;
    }

    RelaySyncItem item{};
    copyCString(item.relayId, sizeof(item.relayId), ch->relayId);
    item.state = ch->currentState;
    item.isManual = ch->isManual;

    const String nowIso = currentTimestampUtc();
    if (mqttClient.publishRelaysState("manual", nowIso.c_str(), &item, 1)) {
        relayManager.markChannelSynced(channel);
        pulseActivity = true;
    }
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

    const bool useMqtt = mqttClient.isConnected();
    const size_t configuredMax = (runtimeConfigLoaded && runtimeConfig.maxBatchSize > 0)
        ? runtimeConfig.maxBatchSize
        : deviceConfig.maxBatchSize;
    const size_t effectiveCap = useMqtt
        ? (configuredMax < deviceConfig.maxMqttBatchSize ? configuredMax : deviceConfig.maxMqttBatchSize)
        : configuredMax;
    const size_t maxBatchSize = TelemetryQueue::computeBatchLimit(effectiveCap, useMqtt);

    TelemetryRecord batch[MAX_HTTP_BATCH_SIZE];
    const size_t batchSize = telemetryQueue.peekBatch(batch, maxBatchSize);
    if (batchSize == 0) {
        return;
    }

    if (useMqtt) {
        if (mqttClient.publishTelemetry(batch, batchSize)) {
            telemetryQueue.removeProcessed(batchSize, 0);
            telemetrySentCount += static_cast<uint32_t>(batchSize);
            pulseActivity = true;
            clearError(ErrorCode::E005_BACKEND_UNAVAILABLE);
            clearError(ErrorCode::E011_MQTT_DISCONNECTED);
        } else {
            setError(ErrorCode::E011_MQTT_DISCONNECTED, "MQTT pub fail");
        }
        return;
    }

    // Permanent HTTP fallback via ApiGateway (:5055) when MQTT broker is unavailable.
    ApiTelemetryResponse telemetryResponse;
    ApiResult result = apiClient.sendTelemetry(batch, batchSize, telemetryResponse);
    lastHttpStatus = result.statusCode;
    if (!result.success) {
        if (result.statusCode == 401 || result.statusCode == 403) {
            setError(ErrorCode::E010_DEVICE_TOKEN_MISSING, "Token rejected", true);
        } else {
            setError(ErrorCode::E005_BACKEND_UNAVAILABLE, "Telemetry failed");
        }
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

void processHttpFallbackPing() {
    if (!wifiManager.isConnected() || mqttClient.isConnected() || deviceConfig.controllerId[0] == '\0') {
        return;
    }

    const uint32_t intervalMs = static_cast<uint32_t>(
        deviceConfig.mqttHeartbeatIntervalSec > 0 ? deviceConfig.mqttHeartbeatIntervalSec : 60) * 1000UL;
    if (lastHttpPingAtMs != 0 && (millis() - lastHttpPingAtMs < intervalMs)) {
        return;
    }
    lastHttpPingAtMs = millis();

    int64_t serverEpoch = 0;
    const ApiResult res = apiClient.pingController(deviceConfig.controllerId, serverEpoch);
    lastHttpStatus = res.statusCode;
    if (!res.success) {
        apiHealthy = false;
        setError(ErrorCode::E005_BACKEND_UNAVAILABLE, "HTTP ping fail");
        return;
    }

    apiHealthy = true;
    pulseActivity = true;
    clearError(ErrorCode::E005_BACKEND_UNAVAILABLE);

    if (!rtcClock.isTimeValid() && serverEpoch > 0 && rtcClock.syncFromServerEpoch(serverEpoch)) {
        clearError(ErrorCode::E003_RTC_UNAVAILABLE);
        clearError(ErrorCode::E004_TIME_INVALID);
    }
}

void processHttpFallbackCommands() {
    // HTTP command polling is disabled while MQTT session is active.
    if (!wifiManager.isConnected() || mqttClient.isConnected() || deviceConfig.controllerId[0] == '\0') {
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

bool loadRuntimeConfig(bool force = false) {
    if (!wifiManager.isConnected()) {
        return false;
    }
    if (!force && lastConfigAttemptAtMs != 0 && (millis() - lastConfigAttemptAtMs < CONFIG_RETRY_INTERVAL_MS)) {
        return false;
    }
    lastConfigAttemptAtMs = millis();

    String rawJson;
    ApiResult result = apiClient.fetchRuntimeConfig(runtimeConfig, &rawJson);
    lastHttpStatus = result.statusCode;
    if (!result.success) {
        if (result.statusCode == 401 || result.statusCode == 403) {
            setError(ErrorCode::E010_DEVICE_TOKEN_MISSING, "Token rejected", true);
        } else {
            setError(ErrorCode::E005_BACKEND_UNAVAILABLE, "Config fetch fail");
        }
        apiHealthy = false;
        return false;
    }

    ConfigStore::saveRuntimeConfigCache(rawJson.c_str(), runtimeConfig.configVersion);
    runtimeConfigLoaded = true;
    serverConfigSyncedOnce = true;
    sensorManager.applyRuntimeConfig(runtimeConfig, deviceConfig);
    // Keep physical relay contacts safe/intact on config refresh; sync metadata & mode.
    relayManager.applyRuntimeConfig(runtimeConfig, deviceConfig, false);
    apiHealthy = true;
    pulseActivity = true;
    clearError(ErrorCode::E005_BACKEND_UNAVAILABLE);
    clearError(ErrorCode::E010_DEVICE_TOKEN_MISSING);

    if (mqttClient.isConnected()) {
        if (!bootRelaySyncPublished) {
            publishBootRelaysState();
        } else if (relayManager.hasAnyPendingSync()) {
            publishPendingManualRelays();
        }
    }

    Serial.printf("[CFG] Synced config v%llu (sensors=%u relays=%u)\n",
                  static_cast<unsigned long long>(runtimeConfig.configVersion),
                  static_cast<unsigned>(runtimeConfig.sensorCount),
                  static_cast<unsigned>(runtimeConfig.relayCount));
    return true;
}

void processMqttIncoming() {
    if (!mqttClient.isConnected()) {
        return;
    }

    int64_t nowEpoch = 0;
    const String nowIso = currentTimestampUtc();
    if (rtcClock.isTimeValid()) {
        ProtocolCodecs::parseIso8601UtcToEpoch(nowIso.c_str(), nowEpoch);
    }

    MqttRelayCommand cmd{};
    while (mqttClient.popIncomingCommand(cmd)) {
        char errBuf[64] = {};
        const CommandApplyStatus status = relayManager.applyServerCommand(
            cmd.commandId,
            cmd.relayId,
            cmd.targetState,
            cmd.expireAtEpochSec,
            nowEpoch,
            errBuf,
            sizeof(errBuf));

        const bool completed = (status == CommandApplyStatus::APPLIED || status == CommandApplyStatus::DUPLICATE);
        mqttClient.publishCommandAck(
            cmd.commandId,
            completed,
            completed ? nullptr : (errBuf[0] != '\0' ? errBuf : "Command failed"),
            nowIso.c_str());
        pulseActivity = true;
        Serial.printf("[MQTT] Command %s relay=%s target=%d -> %s\n",
                      cmd.commandId,
                      cmd.relayId,
                      cmd.targetState ? 1 : 0,
                      completed ? "completed" : "failed");
    }

    ConfigNotifyMessage notify{};
    if (mqttClient.popConfigNotify(notify)) {
        Serial.printf("[MQTT] config/notify version=%llu (local=%llu, reason=%s)\n",
                      static_cast<unsigned long long>(notify.version),
                      static_cast<unsigned long long>(runtimeConfig.configVersion),
                      notify.reason);
        if (ProtocolCodecs::shouldTriggerConfigSync(notify.version, runtimeConfig.configVersion)) {
            loadRuntimeConfig(true);
        }
    }
}

void runOtaCheck() {
    if (!wifiManager.isConnected()) {
        return;
    }

    lastOtaCheckAtMs = millis();
    ErrorCode otaErr = ErrorCode::NONE;
    if (otaManager.checkAndPerformUpdate(deviceConfig, systemState, otaErr)) {
        clearError(ErrorCode::E012_OTA_FAILED);
    } else if (otaErr != ErrorCode::NONE) {
        setError(otaErr, otaErr == ErrorCode::E010_DEVICE_TOKEN_MISSING ? "OTA token err" : "OTA check fail");
    }
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
    switch (action.type) {
        case UiActionType::NONE:
            break;
        case UiActionType::TOGGLE_RELAY_STATE:
            // Mark pendingSync first; cleared immediately upon confirmed MQTT publish.
            if (relayManager.manualToggle(action.relayChannel, false)) {
                pulseActivity = true;
                publishSingleManualRelay(action.relayChannel);
            }
            break;
        case UiActionType::TOGGLE_RELAY_MODE:
            if (relayManager.manualToggleMode(action.relayChannel, false)) {
                pulseActivity = true;
                publishSingleManualRelay(action.relayChannel);
            }
            break;
        case UiActionType::SYNC_CONFIG:
            if (wifiManager.isConnected()) {
                loadRuntimeConfig(true);
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
            runOtaCheck();
            break;
        case UiActionType::SAFE_STATE_ALL:
            relayManager.setAllSafe();
            pulseActivity = true;
            publishPendingManualRelays();
            break;
        case UiActionType::REBOOT:
            Serial.println("[SYS] Manual reboot requested.");
            mqttClient.disconnect();
            delay(200);
            ESP.restart();
            break;
        case UiActionType::FACTORY_RESET:
            Serial.println("[RESET] Factory reset triggered via System -> Factory Reset!");
            changeState(SystemState::FACTORY_RESET);
            mqttClient.disconnect();
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
    runtimeConfigLoaded = ConfigStore::loadRuntimeConfigCache(runtimeConfig);

    changeState(SystemState::INIT_HARDWARE);

    // Apply safeState to all 8 relay channels BEFORE starting Wi-Fi/MQTT.
    relayManager.begin(deviceConfig);
    telemetryQueue.begin(deviceConfig.maxQueueSize);
    displayManager.begin();
    inputManager.begin();
    ledIndicator.begin();
    uiController.begin();
    wifiManager.begin(deviceConfig);
    initializeRuntimeIdentity();
    apiClient.begin(deviceConfig);
    mqttClient.begin(deviceConfig, runtimeMacCanonical);
    otaManager.begin(&apiClient, &relayManager, &displayManager);

    if (!sensorManager.begin(deviceConfig)) {
        setError(ErrorCode::E007_SENSOR_READ_FAILED, "No sensors init");
    }

    if (runtimeConfigLoaded) {
        sensorManager.applyRuntimeConfig(runtimeConfig, deviceConfig);
        relayManager.applyRuntimeConfig(runtimeConfig, deviceConfig, false);
        Serial.printf("[CFG] Loaded NVS cache v%llu (sensors=%u relays=%u)\n",
                      static_cast<unsigned long long>(runtimeConfig.configVersion),
                      static_cast<unsigned>(runtimeConfig.sensorCount),
                      static_cast<unsigned>(runtimeConfig.relayCount));
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
        systemState == SystemState::CONNECT_MQTT ||
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

    if (wifiManager.isConnected() && systemState == SystemState::SYNC_TIME) {
        changeState(SystemState::CONNECT_MQTT);
    }

    if (wifiManager.isConnected() && !serverConfigSyncedOnce) {
        loadRuntimeConfig(false);
    }

    if (wifiManager.isConnected() && !otaManager.isPostBootConfirmed() &&
        (lastPostBootAttemptAtMs == 0 || millis() - lastPostBootAttemptAtMs >= OTA_POSTBOOT_RETRY_INTERVAL_MS)) {
        lastPostBootAttemptAtMs = millis();
        ErrorCode otaErr = ErrorCode::NONE;
        otaManager.handlePostBootConfirmation(deviceConfig, otaErr);
        if (otaErr != ErrorCode::NONE) {
            setError(otaErr, "OTA rollback");
        }
    }

    const String ipStr = wifiManager.isConnected() ? wifiManager.localIp().toString() : String("0.0.0.0");
    const int rssi = wifiManager.isConnected() ? WiFi.RSSI() : 0;
    const String nowIso = currentTimestampUtc();

    if (systemState != SystemState::SAFE_MODE && systemState != SystemState::OTA_UPDATING) {
        ErrorCode mqttErr = ErrorCode::NONE;
        mqttClient.update(wifiManager.isConnected(), ipStr.c_str(), rssi, nowIso.c_str(), mqttErr);
        if (mqttErr != ErrorCode::NONE) {
            setError(mqttErr, "MQTT disconn");
        }
    }

    if (mqttClient.consumeJustConnected()) {
        clearError(ErrorCode::E011_MQTT_DISCONNECTED);
        // Priority 1: sync offline manual overrides BEFORE status=online triggers server redelivery.
        publishPendingManualRelays();
        publishBootRelaysState();
        // Priority 2: subscribe to commands/config and publish retained status=online.
        mqttClient.completeSessionStartup(ipStr.c_str(), rssi, millis() / 1000UL, nowIso.c_str());
        // Priority 3: confirm pending OTA rollback state if applicable.
        if (!otaManager.isPostBootConfirmed()) {
            ErrorCode otaErr = ErrorCode::NONE;
            otaManager.handlePostBootConfirmation(deviceConfig, otaErr);
            if (otaErr != ErrorCode::NONE) {
                setError(otaErr, "OTA rollback");
            }
        }
    }

    processMqttIncoming();

    if (runtimeConfigLoaded) {
        ErrorCode sensorErr = ErrorCode::NONE;
        if (!sensorManager.update(sensorErr) && sensorErr != ErrorCode::NONE) {
            setError(sensorErr, "Sensor read fail");
        } else {
            clearError(ErrorCode::E007_SENSOR_READ_FAILED);
        }
        queueSensorTelemetry();
    }

    if (wifiManager.isConnected() && mqttClient.isConnected()) {
        changeState(SystemState::ONLINE);
        if (relayManager.hasAnyPendingSync()) {
            publishPendingManualRelays();
        }
        if (runtimeConfigLoaded && !bootRelaySyncPublished) {
            publishBootRelaysState();
        }
        sendQueuedTelemetry(false);
        if (otaManager.isPostBootConfirmed() &&
            (millis() - lastOtaCheckAtMs >= OTA_PERIODIC_CHECK_INTERVAL_MS)) {
            runOtaCheck();
        }
    } else if (wifiManager.isConnected() && systemState != SystemState::SAFE_MODE) {
        // MQTT broker offline -> remain in DEGRADED_OFFLINE while HTTP fallback keeps data flowing.
        changeState(SystemState::DEGRADED_OFFLINE);
        processHttpFallbackPing();
        sendQueuedTelemetry(false);
        processHttpFallbackCommands();
    } else if (systemState != SystemState::SAFE_MODE && systemState != SystemState::CONNECT_WIFI) {
        changeState(SystemState::DEGRADED_OFFLINE);
    }

    LcdViewContext viewCtx{};
    viewCtx.systemState = systemState;
    viewCtx.wifiConnected = wifiManager.isConnected();
    viewCtx.mqttConnected = mqttClient.isConnected();
    viewCtx.httpHealthy = apiHealthy;
    viewCtx.lastHttpStatus = lastHttpStatus;
    viewCtx.wifiRssi = rssi;
    viewCtx.mqttReconnectCount = mqttClient.reconnectCount();
    copyCString(viewCtx.wifiSsid, sizeof(viewCtx.wifiSsid), deviceConfig.wifiSsid);
    copyCString(viewCtx.ipAddress, sizeof(viewCtx.ipAddress), ipStr.c_str());
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
    ledIndicator.update(
        systemState,
        wifiManager.isConnected(),
        mqttClient.isConnected(),
        pulseActivity,
        errorRegistry.count() > 0);
    delay(20);
}
