#include "MqttClientEx.h"

#include <cstring>

MqttClientEx* MqttClientEx::_activeInstance = nullptr;

namespace {
constexpr uint32_t INITIAL_RECONNECT_DELAY_MS = 2000;
constexpr uint32_t MAX_RECONNECT_DELAY_MS = 30000;
}  // namespace

void MqttClientEx::begin(const DeviceConfig& config, const char* canonicalMac) {
    _activeInstance = this;

    copyCString(_host, sizeof(_host), config.mqttHost);
    _port = config.mqttPort > 0 ? config.mqttPort : 1883;
    _keepAliveSec = config.mqttKeepAliveSec > 0 ? config.mqttKeepAliveSec : 30;
    _heartbeatIntervalMs = static_cast<uint32_t>(
        config.mqttHeartbeatIntervalSec > 0 ? config.mqttHeartbeatIntervalSec : 60) * 1000UL;
    copyCString(_deviceToken, sizeof(_deviceToken), config.deviceToken);

    if (!ProtocolCodecs::normalizeMacCanonical(canonicalMac, _canonicalMac, sizeof(_canonicalMac))) {
        copyCString(_canonicalMac, sizeof(_canonicalMac), "00:00:00:00:00:00");
    }

    ProtocolCodecs::buildClientId(_clientId, sizeof(_clientId), _canonicalMac);
    ProtocolCodecs::buildDeviceTopic(_topicTelemetry, sizeof(_topicTelemetry), _canonicalMac, "telemetry");
    ProtocolCodecs::buildDeviceTopic(_topicCommands, sizeof(_topicCommands), _canonicalMac, "commands");
    ProtocolCodecs::buildDeviceTopic(_topicCommandsAck, sizeof(_topicCommandsAck), _canonicalMac, "commands/ack");
    ProtocolCodecs::buildDeviceTopic(_topicRelaysState, sizeof(_topicRelaysState), _canonicalMac, "relays/state");
    ProtocolCodecs::buildDeviceTopic(_topicStatus, sizeof(_topicStatus), _canonicalMac, "status");
    ProtocolCodecs::buildDeviceTopic(_topicConfigNotify, sizeof(_topicConfigNotify), _canonicalMac, "config/notify");

    _mqttClient.setServer(_host, _port);
    _mqttClient.setKeepAlive(_keepAliveSec);
    _mqttClient.setSocketTimeout(2);
    _mqttClient.setBufferSize(MQTT_PACKET_BUFFER_SIZE);
    _mqttClient.setCallback(&MqttClientEx::staticCallback);

    _configured = (_host[0] != '\0' && _deviceToken[0] != '\0');
    _wasConnected = false;
    _hasConnectedBefore = false;
    _justConnected = false;
    _reconnectCount = 0;
    _reconnectDelayMs = INITIAL_RECONNECT_DELAY_MS;
    _lastConnectAttemptMs = 0;
    _lastHeartbeatAtMs = 0;
    _cmdHead = 0;
    _cmdTail = 0;
    _cmdCount = 0;
    _hasPendingConfigNotify = false;

    Serial.printf("[MQTT] Configured broker=%s:%u clientId=%s buf=%u\n",
                  _host,
                  _port,
                  _clientId,
                  static_cast<unsigned>(_mqttClient.getBufferSize()));
}

void MqttClientEx::update(
    bool wifiConnected,
    const char* ipAddress,
    int rssi,
    const char* timestampUtc,
    ErrorCode& errorOut) {
    errorOut = ErrorCode::NONE;

    if (!_configured || !wifiConnected) {
        if (_wasConnected) {
            _mqttClient.disconnect();
            _wasConnected = false;
        }
        return;
    }

    if (_mqttClient.connected()) {
        _mqttClient.loop();
        const uint32_t nowMs = millis();
        if (nowMs - _lastHeartbeatAtMs >= _heartbeatIntervalMs) {
            publishStatusOnline(ipAddress, rssi, nowMs / 1000UL, timestampUtc);
            _lastHeartbeatAtMs = nowMs;
        }
        return;
    }

    if (_wasConnected) {
        _wasConnected = false;
        _lastMqttState = _mqttClient.state();
        Serial.printf("[MQTT] Disconnected (state=%d)\n", _lastMqttState);
        errorOut = ErrorCode::E011_MQTT_DISCONNECTED;
    }

    const uint32_t nowMs = millis();
    if (_lastConnectAttemptMs != 0 && (nowMs - _lastConnectAttemptMs < _reconnectDelayMs)) {
        return;
    }
    _lastConnectAttemptMs = nowMs;

    if (!attemptConnect(ipAddress, rssi, timestampUtc)) {
        _lastMqttState = _mqttClient.state();
        Serial.printf("[MQTT] Connect failed (state=%d), retry in %lums\n",
                      _lastMqttState,
                      static_cast<unsigned long>(_reconnectDelayMs));
        errorOut = ErrorCode::E011_MQTT_DISCONNECTED;
        if (_reconnectDelayMs < MAX_RECONNECT_DELAY_MS) {
            _reconnectDelayMs = (_reconnectDelayMs * 2 > MAX_RECONNECT_DELAY_MS)
                ? MAX_RECONNECT_DELAY_MS
                : (_reconnectDelayMs * 2);
        }
    }
}

bool MqttClientEx::attemptConnect(const char* ipAddress, int rssi, const char* timestampUtc) {
    char lwtPayload[64] = {};
    ProtocolCodecs::encodeStatusOfflineLwt(lwtPayload, sizeof(lwtPayload));

    Serial.printf("[MQTT] Connecting to %s:%u as %s...\n", _host, _port, _clientId);
    const bool connected = _mqttClient.connect(
        _clientId,
        _canonicalMac,
        _deviceToken,
        _topicStatus,
        1,
        true,
        lwtPayload,
        true);

    if (!connected) {
        return false;
    }

    _wasConnected = true;
    _lastMqttState = _mqttClient.state();
    _reconnectDelayMs = INITIAL_RECONNECT_DELAY_MS;
    if (_hasConnectedBefore) {
        ++_reconnectCount;
    }
    _hasConnectedBefore = true;
    _justConnected = true;
    _lastHeartbeatAtMs = millis();

    Serial.printf("[MQTT] Connected to %s:%u (awaiting priority relay sync & startup)\n", _host, _port);
    return true;
}

void MqttClientEx::completeSessionStartup(
    const char* ipAddress,
    int rssi,
    uint32_t uptimeSec,
    const char* timestampUtc) {
    if (!_mqttClient.connected()) {
        return;
    }

    _mqttClient.subscribe(_topicCommands, 1);
    _mqttClient.subscribe(_topicConfigNotify, 1);
    publishStatusOnline(ipAddress, rssi, uptimeSec, timestampUtc);
    _lastHeartbeatAtMs = millis();

    Serial.printf("[MQTT] Session active: subscribed to %s, %s and published status=online\n",
                  _topicCommands,
                  _topicConfigNotify);
}

bool MqttClientEx::isConnected() {
    return _mqttClient.connected();
}

bool MqttClientEx::consumeJustConnected() {
    if (!_justConnected) {
        return false;
    }
    _justConnected = false;
    return true;
}

bool MqttClientEx::popIncomingCommand(MqttRelayCommand& outCmd) {
    if (_cmdCount == 0) {
        return false;
    }
    outCmd = _commandQueue[_cmdHead];
    _cmdHead = (_cmdHead + 1) % INCOMING_COMMAND_QUEUE_SIZE;
    --_cmdCount;
    return true;
}

bool MqttClientEx::popConfigNotify(ConfigNotifyMessage& outNotify) {
    if (!_hasPendingConfigNotify) {
        return false;
    }
    outNotify = _pendingConfigNotify;
    _hasPendingConfigNotify = false;
    return true;
}

bool MqttClientEx::publishStatusOnline(
    const char* ipAddress,
    int rssi,
    uint32_t uptimeSec,
    const char* timestampUtc) {
    if (!_mqttClient.connected()) {
        return false;
    }

    const size_t len = ProtocolCodecs::encodeStatusOnlinePayload(
        FIRMWARE_VERSION,
        ipAddress,
        rssi,
        uptimeSec,
        timestampUtc,
        _txBuffer,
        sizeof(_txBuffer));
    if (len == 0) {
        return false;
    }

    return _mqttClient.publish(_topicStatus, reinterpret_cast<const uint8_t*>(_txBuffer), len, true);
}

bool MqttClientEx::publishTelemetry(const TelemetryRecord* records, size_t count) {
    if (!_mqttClient.connected() || records == nullptr || count == 0) {
        return false;
    }

    const size_t safeCount = count > MAX_MQTT_BATCH_SIZE ? MAX_MQTT_BATCH_SIZE : count;
    const size_t len = ProtocolCodecs::encodeTelemetryPayload(
        _canonicalMac,
        records,
        safeCount,
        _txBuffer,
        sizeof(_txBuffer));
    if (len == 0) {
        Serial.println("[MQTT] Telemetry payload exceeded buffer limit");
        return false;
    }

    return _mqttClient.publish(_topicTelemetry, reinterpret_cast<const uint8_t*>(_txBuffer), len, false);
}

bool MqttClientEx::publishCommandAck(
    const char* commandId,
    bool completed,
    const char* errorMessage,
    const char* executedAtUtc) {
    if (!_mqttClient.connected()) {
        return false;
    }

    const size_t len = ProtocolCodecs::encodeCommandAckPayload(
        commandId,
        completed,
        errorMessage,
        executedAtUtc,
        _txBuffer,
        sizeof(_txBuffer));
    if (len == 0) {
        return false;
    }

    return _mqttClient.publish(_topicCommandsAck, reinterpret_cast<const uint8_t*>(_txBuffer), len, false);
}

bool MqttClientEx::publishRelaysState(
    const char* source,
    const char* recordedAtUtc,
    const RelaySyncItem* items,
    size_t count) {
    if (!_mqttClient.connected() || items == nullptr || count == 0) {
        return false;
    }

    const size_t len = ProtocolCodecs::encodeRelaysStatePayload(
        _canonicalMac,
        source,
        recordedAtUtc,
        items,
        count,
        _txBuffer,
        sizeof(_txBuffer));
    if (len == 0) {
        return false;
    }

    return _mqttClient.publish(_topicRelaysState, reinterpret_cast<const uint8_t*>(_txBuffer), len, false);
}

void MqttClientEx::disconnect() {
    if (_mqttClient.connected()) {
        _mqttClient.disconnect();
    }
    _wasConnected = false;
}

void MqttClientEx::staticCallback(char* topic, byte* payload, unsigned int length) {
    if (_activeInstance != nullptr) {
        _activeInstance->onMessage(topic, payload, length);
    }
}

void MqttClientEx::onMessage(const char* topic, const uint8_t* payload, unsigned int length) {
    if (topic == nullptr || payload == nullptr || length == 0) {
        return;
    }

    char rxBuf[1024];
    const size_t copyLen = length < (sizeof(rxBuf) - 1) ? length : (sizeof(rxBuf) - 1);
    std::memcpy(rxBuf, payload, copyLen);
    rxBuf[copyLen] = '\0';

    if (std::strcmp(topic, _topicCommands) == 0) {
        MqttRelayCommand cmd{};
        if (!ProtocolCodecs::decodeCommandPayload(rxBuf, cmd)) {
            Serial.println("[MQTT] Failed to decode incoming command payload");
            return;
        }
        if (_cmdCount < INCOMING_COMMAND_QUEUE_SIZE) {
            _commandQueue[_cmdTail] = cmd;
            _cmdTail = (_cmdTail + 1) % INCOMING_COMMAND_QUEUE_SIZE;
            ++_cmdCount;
        } else {
            Serial.println("[MQTT] Incoming command queue full, dropping command");
        }
        return;
    }

    if (std::strcmp(topic, _topicConfigNotify) == 0) {
        ConfigNotifyMessage notify{};
        if (!ProtocolCodecs::decodeConfigNotifyPayload(rxBuf, notify)) {
            Serial.println("[MQTT] Failed to decode config/notify payload");
            return;
        }
        if (!_hasPendingConfigNotify || notify.version >= _pendingConfigNotify.version) {
            _pendingConfigNotify = notify;
            _hasPendingConfigNotify = true;
        }
    }
}
