#include "ApiClient.h"

#include <ArduinoJson.h>
#include <ProtocolCodecs.h>
#include "PinMap.h"

namespace {
String httpMessage(int statusCode) {
    return "HTTP " + String(statusCode);
}
}

void ApiClient::begin(const DeviceConfig& config) {
    _baseUrl = config.apiBaseUrl;
    _deviceToken = config.deviceToken;
    _telemetryPath = config.telemetryPath;
}

void ApiClient::setMacAddress(const String& macAddress) {
    _macAddress = macAddress;
}

ApiResult ApiClient::fetchRuntimeConfig(BackendRuntimeConfig& runtimeConfig) {
    HTTPClient http;
    const String url = buildUrl(DEFAULT_CONFIG_PATH);
    if (!prepareRequest(http, url)) {
        return {false, -1, "begin failed"};
    }

    http.addHeader("X-Mac-Address", _macAddress);
    const int statusCode = http.GET();
    if (statusCode != HTTP_CODE_OK) {
        const String message = http.getString();
        http.end();
        return {false, statusCode, message.length() > 0 ? message : httpMessage(statusCode)};
    }

    const String payload = http.getString();
    http.end();
    if (!ProtocolCodecs::decodeControllerConfig(payload.c_str(), runtimeConfig)) {
        return {false, -1, "config json decode failed"};
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

    if (statusCode != HTTP_CODE_ACCEPTED) {
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

    response.acceptedCount = responseDocument["acceptedCount"] | 0;
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
    const String body = "\"" + reason + "\"";
    const int statusCode = http.POST(body);
    const String responseBody = http.getString();
    http.end();

    return {statusCode >= 200 && statusCode < 300, statusCode, responseBody};
}

bool ApiClient::prepareRequest(HTTPClient& http, const String& url) {
    if (!http.begin(url)) {
        return false;
    }

    Serial.printf("[DEBUG] URL: %s\n", url.c_str());
    Serial.printf("[DEBUG] Mac: %s\n", _macAddress.c_str());
    if (_deviceToken.length() >= 8) {
        Serial.printf("[DEBUG] Token: %.8s****\n", _deviceToken.c_str());
    } else {
        Serial.println("[DEBUG] Token: ****");
    }

    http.addHeader("X-Device-Token", _deviceToken);
    http.addHeader("X-Mac-Address", _macAddress);
    http.addHeader("X-Firmware-Version", FIRMWARE_VERSION);
    http.setTimeout(5000);
    return true;
}

String ApiClient::buildUrl(const String& path) const {
    if (_baseUrl.endsWith("/") && path.startsWith("/")) {
        return _baseUrl.substring(0, _baseUrl.length() - 1) + path;
    }
    if (!_baseUrl.endsWith("/") && !path.startsWith("/")) {
        return _baseUrl + "/" + path;
    }
    return _baseUrl + path;
}

void ApiClient::parseSensor(const JsonObjectConst& item, SensorRuntimeConfig& sensorConfig, size_t) {
    sensorConfig = SensorRuntimeConfig{};
    sensorConfig.enabled = true;
    copyCString(sensorConfig.sensorId, sizeof(sensorConfig.sensorId), item["sensorId"] | "");
    copyCString(sensorConfig.name, sizeof(sensorConfig.name), item["name"] | "");
    sensorConfig.connectionProtocol = item["connectionProtocol"] | 0;
    copyCString(sensorConfig.connectionAddress, sizeof(sensorConfig.connectionAddress), item["connectionAddress"] | "");
    sensorConfig.sensorType = item["type"] | 0;
    copyCString(sensorConfig.unit, sizeof(sensorConfig.unit), item["unit"] | "");
    sensorConfig.pollIntervalMs = 5000;
}

void ApiClient::parseRelay(const JsonObjectConst& item, RelayRuntimeConfig& relayConfig, size_t index) {
    relayConfig = RelayRuntimeConfig{};
    relayConfig.configured = true;
    relayConfig.channel = static_cast<uint8_t>(index + 1);
    relayConfig.pin = RELAY_PINS[index];
    copyCString(relayConfig.relayId, sizeof(relayConfig.relayId), item["relayId"] | "");
    copyCString(relayConfig.name, sizeof(relayConfig.name), item["name"] | "");
    copyCString(relayConfig.connectionAddress, sizeof(relayConfig.connectionAddress), item["connectionAddress"] | "");
    const bool isNormallyOpen = item["isNormallyOpen"].is<bool>()
        ? item["isNormallyOpen"].as<bool>()
        : (item["isNormalyOpen"] | true);
    relayConfig.activeLow = isNormallyOpen;
    relayConfig.purpose = static_cast<RelayPurpose>(item["purpose"] | static_cast<int>(RelayPurpose::RESERVE));
    relayConfig.currentState = item["isActive"] | false;
    relayConfig.backendState = relayConfig.currentState;
    relayConfig.isManual = item["isManual"] | false;
}
