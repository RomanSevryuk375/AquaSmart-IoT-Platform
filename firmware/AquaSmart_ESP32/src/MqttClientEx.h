#ifndef MQTT_CLIENT_EX_H
#define MQTT_CLIENT_EX_H

#include <PubSubClient.h>
#include <WiFiClient.h>
#include <ProtocolCodecs.h>
#include "AppState.h"

class MqttClientEx {
public:
    static constexpr size_t INCOMING_COMMAND_QUEUE_SIZE = 8;

    void begin(const DeviceConfig& config, const char* canonicalMac);
    void update(bool wifiConnected, const char* ipAddress, int rssi, const char* timestampUtc, ErrorCode& errorOut);

    bool isConnected();
    bool consumeJustConnected();
    void completeSessionStartup(const char* ipAddress, int rssi, uint32_t uptimeSec, const char* timestampUtc);
    uint32_t reconnectCount() const { return _reconnectCount; }
    int lastState() const { return _lastMqttState; }

    bool popIncomingCommand(MqttRelayCommand& outCmd);
    bool popConfigNotify(ConfigNotifyMessage& outNotify);

    bool publishStatusOnline(const char* ipAddress, int rssi, uint32_t uptimeSec, const char* timestampUtc);
    bool publishTelemetry(const TelemetryRecord* records, size_t count);
    bool publishCommandAck(const char* commandId, bool completed, const char* errorMessage, const char* executedAtUtc);
    bool publishRelaysState(const char* source, const char* recordedAtUtc, const RelaySyncItem* items, size_t count);

    void disconnect();

private:
    WiFiClient _wifiClient;
    PubSubClient _mqttClient{_wifiClient};

    char _host[64] = {};
    uint16_t _port = 1883;
    uint16_t _keepAliveSec = 30;
    uint32_t _heartbeatIntervalMs = 60000;
    char _canonicalMac[18] = {};
    char _deviceToken[128] = {};
    char _clientId[32] = {};

    char _topicTelemetry[64] = {};
    char _topicCommands[64] = {};
    char _topicCommandsAck[64] = {};
    char _topicRelaysState[64] = {};
    char _topicStatus[64] = {};
    char _topicConfigNotify[64] = {};

    char _txBuffer[MQTT_PACKET_BUFFER_SIZE] = {};

    bool _configured = false;
    bool _wasConnected = false;
    bool _hasConnectedBefore = false;
    bool _justConnected = false;
    uint32_t _reconnectCount = 0;
    int _lastMqttState = 0;
    uint32_t _lastConnectAttemptMs = 0;
    uint32_t _reconnectDelayMs = 2000;
    uint32_t _lastHeartbeatAtMs = 0;

    MqttRelayCommand _commandQueue[INCOMING_COMMAND_QUEUE_SIZE] = {};
    size_t _cmdHead = 0;
    size_t _cmdTail = 0;
    size_t _cmdCount = 0;

    bool _hasPendingConfigNotify = false;
    ConfigNotifyMessage _pendingConfigNotify{};

    static MqttClientEx* _activeInstance;
    static void staticCallback(char* topic, byte* payload, unsigned int length);
    void onMessage(const char* topic, const uint8_t* payload, unsigned int length);
    bool attemptConnect(const char* ipAddress, int rssi, const char* timestampUtc);
};

#endif
