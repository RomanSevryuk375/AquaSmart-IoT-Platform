#include "ApiClient.h"

#include <ArduinoJson.h>
#include <ProtocolCodecs.h>
#include "PinMap.h"

namespace {
String httpMessage(int statusCode) {
    return "HTTP " + String(statusCode);
}
}  // namespace

void ApiClient::begin(const DeviceConfig& config) {
    _baseUrl = config.apiBaseUrl;
    _deviceToken = config.deviceToken;
    _telemetryPath = config.telemetryPath[0] != '\0' ? config.telemetryPath : DEFAULT_TELEMETRY_PATH;
}

void ApiClient::setMacAddress(const String& macAddress) {
    _macAddress = macAddress;
}

ApiResult ApiClient::fetchRuntimeConfig(BackendRuntimeConfig& runtimeConfig, String* outRawJson) {
    HTTPClient http;
    const String url = buildUrl(DEFAULT_CONFIG_PATH);
    if (!prepareRequest(http, url)) {
        return {false, -1, "begin failed"};
    }

    const int statusCode = http.GET();
    const String payload = http.getString();
    http.end();

    if (statusCode != HTTP_CODE_OK) {
        return {false, statusCode, payload.length() > 0 ? payload : httpMessage(statusCode)};
    }

    if (!ProtocolCodecs::decodeControllerConfig(payload.c_str(), runtimeConfig)) {
        return {false, -1, "config json decode failed"};
    }

    if (outRawJson != nullptr) {
        *outRawJson = payload;
    }

    return {true, statusCode, "ok"};
}

ApiResult ApiClient::pingController(const char* controllerId, int64_t& outServerEpochSec) {
    outServerEpochSec = 0;
    if (controllerId == nullptr || controllerId[0] == '\0') {
        return {false, -1, "missing controllerId"};
    }

    HTTPClient http;
    const String url = buildUrl(String("/api/device/v1/controllers/") + controllerId + "/ping");
    if (!prepareRequest(http, url)) {
        return {false, -1, "begin failed"};
    }

    const int statusCode = http.POST("");
    const String responseBody = http.getString();
    http.end();

    if (statusCode < 200 || statusCode >= 300) {
        return {false, statusCode, responseBody.length() > 0 ? responseBody : httpMessage(statusCode)};
    }

    if (responseBody.length() > 0) {
        ProtocolCodecs::decodePingResponsePayload(responseBody.c_str(), outServerEpochSec);
    }

    return {true, statusCode, "ok"};
}

ApiResult ApiClient::sendTelemetry(const TelemetryRecord* items, size_t itemCount, ApiTelemetryResponse& response) {
    HTTPClient http;
    const String url = buildUrl(_telemetryPath);
    if (!prepareRequest(http, url)) {
        return {false, -1, "begin failed"};
    }

    http.addHeader("Content-Type", "application/json");

    JsonDocument doc;
    doc["macAddress"] = _macAddress.length() > 0 ? _macAddress : String("00:00:00:00:00:00");
    JsonArray payloadItems = doc["items"].to<JsonArray>();
    for (size_t index = 0; index < itemCount; ++index) {
        JsonObject item = payloadItems.add<JsonObject>();
        item["sensorId"] = items[index].sensorId;
        item["value"] = items[index].value;
        item["externalMessageId"] = items[index].externalMessageId;
        item["recordedAt"] = items[index].recordedAt;
    }

    String body;
    serializeJson(doc, body);

    const int statusCode = http.POST(body);
    const String responseBody = http.getString();
    http.end();

    if (statusCode != HTTP_CODE_ACCEPTED && statusCode != HTTP_CODE_OK) {
        return {false, statusCode, responseBody.length() > 0 ? responseBody : httpMessage(statusCode)};
    }

    if (responseBody.length() == 0) {
        response.acceptedCount = static_cast<int>(itemCount);
        response.skippedCount = 0;
        response.validationErrorCount = 0;
        return {true, statusCode, "accepted"};
    }

    JsonDocument responseDocument;
    const DeserializationError error = deserializeJson(responseDocument, responseBody);
    if (error) {
        response.acceptedCount = static_cast<int>(itemCount);
        response.skippedCount = 0;
        response.validationErrorCount = 0;
        return {true, statusCode, "accepted"};
    }

    response.acceptedCount = responseDocument["acceptedCount"] | static_cast<int>(itemCount);
    response.skippedCount = responseDocument["skippedCount"] | 0;
    response.validationErrorCount = 0;
    JsonArrayConst validationErrors = responseDocument["validationErrors"].as<JsonArrayConst>();
    for (JsonVariantConst item : validationErrors) {
        if (response.validationErrorCount >= MAX_VALIDATION_ERRORS) {
            break;
        }
        response.validationErrors[response.validationErrorCount++] = item.as<String>();
    }

    return {true, statusCode, "accepted"};
}

ApiResult ApiClient::fetchPendingCommands(PendingCommand* commands, size_t capacity, size_t& commandCount, const String& controllerId) {
    HTTPClient http;
    const String url = buildUrl(String(DEFAULT_COMMANDS_PATH_PREFIX) + controllerId);
    if (!prepareRequest(http, url)) {
        return {false, -1, "begin failed"};
    }

    const int statusCode = http.GET();
    const String responseBody = http.getString();
    http.end();

    if (statusCode != HTTP_CODE_OK) {
        return {false, statusCode, responseBody.length() > 0 ? responseBody : httpMessage(statusCode)};
    }

    JsonDocument document;
    const DeserializationError error = deserializeJson(document, responseBody);
    if (error) {
        return {false, -1, error.c_str()};
    }

    commandCount = 0;
    JsonArrayConst list = document.as<JsonArrayConst>();
    for (JsonObjectConst item : list) {
        if (commandCount >= capacity) {
            break;
        }

        commands[commandCount].id = item["id"].as<String>();
        commands[commandCount].controllerId = item["controllerId"].as<String>();
        commands[commandCount].relayId = item["relayId"].as<String>();
        commands[commandCount].action = item["action"] | 0;
        commands[commandCount].status = item["status"] | 0;
        commands[commandCount].attemptCount = item["attemptCount"] | 0;
        commands[commandCount].expireAt = item["expireAt"].as<String>();
        commands[commandCount].processedAt = item["processedAt"].as<String>();
        commands[commandCount].errorMessage = item["errorMessage"].as<String>();
        commands[commandCount].createdAt = item["createdAt"].as<String>();
        ++commandCount;
    }

    return {true, statusCode, "ok"};
}

ApiResult ApiClient::completeCommand(const String& commandId) {
    HTTPClient http;
    const String url = buildUrl(String("/api/device/v1/commands/") + commandId + DEFAULT_COMMAND_COMPLETE_SUFFIX);
    if (!prepareRequest(http, url)) {
        return {false, -1, "begin failed"};
    }

    const int statusCode = http.POST("");
    const String responseBody = http.getString();
    http.end();

    return {statusCode >= 200 && statusCode < 300, statusCode, responseBody};
}

ApiResult ApiClient::failCommand(const String& commandId, const String& reason) {
    HTTPClient http;
    const String url = buildUrl(String("/api/device/v1/commands/") + commandId + DEFAULT_COMMAND_FAIL_SUFFIX);
    if (!prepareRequest(http, url)) {
        return {false, -1, "begin failed"};
    }

    http.addHeader("Content-Type", "application/json");
    JsonDocument doc;
    doc.set(reason);
    String body;
    serializeJson(doc, body);

    const int statusCode = http.POST(body);
    const String responseBody = http.getString();
    http.end();

    return {statusCode >= 200 && statusCode < 300, statusCode, responseBody};
}

ApiResult ApiClient::checkOtaUpdate(const char* controllerId, OtaCheckInfo& outCheck) {
    outCheck = OtaCheckInfo{};
    if (controllerId == nullptr || controllerId[0] == '\0') {
        return {false, -1, "missing controllerId"};
    }

    HTTPClient http;
    const String url = buildUrl(String(DEFAULT_OTA_CHECK_PATH) + "?controller_id=" + controllerId);
    if (!prepareRequest(http, url, 8000)) {
        return {false, -1, "begin failed"};
    }

    const int statusCode = http.GET();
    const String payload = http.getString();
    http.end();

    if (statusCode != HTTP_CODE_OK) {
        return {false, statusCode, payload.length() > 0 ? payload : httpMessage(statusCode)};
    }

    if (!ProtocolCodecs::decodeOtaCheckPayload(payload.c_str(), outCheck)) {
        return {false, -1, "ota check decode failed"};
    }

    return {true, statusCode, "ok"};
}

ApiResult ApiClient::reportOtaStatus(
    const char* campaignId,
    const char* controllerId,
    OtaTargetStatus status,
    const char* errorMessage) {
    char payload[320] = {};
    const size_t len = ProtocolCodecs::encodeOtaStatusPayload(
        campaignId,
        controllerId,
        status,
        errorMessage,
        payload,
        sizeof(payload));
    if (len == 0) {
        return {false, -1, "ota status encode failed"};
    }

    HTTPClient http;
    const String url = buildUrl(DEFAULT_OTA_STATUS_PATH);
    if (!prepareRequest(http, url, 8000)) {
        return {false, -1, "begin failed"};
    }

    http.addHeader("Content-Type", "application/json");
    const int statusCode = http.POST(reinterpret_cast<uint8_t*>(payload), len);
    const String responseBody = http.getString();
    http.end();

    const bool ok = (statusCode >= 200 && statusCode < 300);
    return {ok, statusCode, responseBody.length() > 0 ? responseBody : httpMessage(statusCode)};
}

bool ApiClient::prepareRequest(HTTPClient& http, const String& url, uint16_t timeoutMs) {
    if (!http.begin(url)) {
        return false;
    }

    http.addHeader("X-Device-Token", _deviceToken);
    if (_macAddress.length() > 0) {
        http.addHeader("X-Mac-Address", _macAddress);
    }
    http.addHeader("X-Firmware-Version", FIRMWARE_VERSION);
    http.setTimeout(timeoutMs);
    return true;
}

String ApiClient::buildUrl(const String& path) const {
    if (path.startsWith("http://") || path.startsWith("https://")) {
        return path;
    }
    if (_baseUrl.endsWith("/") && path.startsWith("/")) {
        return _baseUrl.substring(0, _baseUrl.length() - 1) + path;
    }
    if (!_baseUrl.endsWith("/") && !path.startsWith("/")) {
        return _baseUrl + "/" + path;
    }
    return _baseUrl + path;
}
