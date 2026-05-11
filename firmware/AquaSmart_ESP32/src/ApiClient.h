#ifndef API_CLIENT_H
#define API_CLIENT_H

#include <ArduinoJson.h>
#include <HTTPClient.h>
#include "AppState.h"

class ApiClient {
public:
    void begin(const DeviceConfig& config);
    void setMacAddress(const String& macAddress);
    ApiResult fetchRuntimeConfig(BackendRuntimeConfig& runtimeConfig);
    ApiResult sendTelemetry(const TelemetryRecord* items, size_t itemCount, ApiTelemetryResponse& response);
    ApiResult fetchPendingCommands(PendingCommand* commands, size_t capacity, size_t& commandCount, const String& controllerId);
    ApiResult completeCommand(const String& commandId);
    ApiResult failCommand(const String& commandId, const String& reason);

private:
    String _baseUrl;
    String _deviceToken;
    String _telemetryPath;
    String _macAddress;

    bool prepareRequest(HTTPClient& http, const String& url);
    String buildUrl(const String& path) const;
    void parseSensor(const JsonObjectConst& item, SensorRuntimeConfig& sensorConfig, size_t index);
    void parseRelay(const JsonObjectConst& item, RelayRuntimeConfig& relayConfig, size_t index);
};

#endif
