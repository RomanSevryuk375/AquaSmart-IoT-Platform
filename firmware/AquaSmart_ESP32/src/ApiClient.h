#ifndef API_CLIENT_H
#define API_CLIENT_H

#include <ArduinoJson.h>
#include <HTTPClient.h>
#include <OtaStateMachine.h>
#include "AppState.h"

class ApiClient {
public:
    void begin(const DeviceConfig& config);
    void setMacAddress(const String& macAddress);

    ApiResult fetchRuntimeConfig(BackendRuntimeConfig& runtimeConfig, String* outRawJson = nullptr);
    ApiResult pingController(const char* controllerId, int64_t& outServerEpochSec);
    ApiResult sendTelemetry(const TelemetryRecord* items, size_t itemCount, ApiTelemetryResponse& response);
    ApiResult fetchPendingCommands(PendingCommand* commands, size_t capacity, size_t& commandCount, const String& controllerId);
    ApiResult completeCommand(const String& commandId);
    ApiResult failCommand(const String& commandId, const String& reason);

    ApiResult checkOtaUpdate(const char* controllerId, OtaCheckInfo& outCheck);
    ApiResult reportOtaStatus(
        const char* campaignId,
        const char* controllerId,
        OtaTargetStatus status,
        const char* errorMessage);

    bool prepareRequest(HTTPClient& http, const String& url, uint16_t timeoutMs = 5000);
    String buildUrl(const String& path) const;

private:
    String _baseUrl;
    String _deviceToken;
    String _telemetryPath;
    String _macAddress;
};

#endif
