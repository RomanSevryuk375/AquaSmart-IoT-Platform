#include "ApiClient.h"

#include <ArduinoJson.h>
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

    JsonDocument doc;
    const DeserializationError error = deserializeJson(doc, http.getString());
    http.end();
    if (error) {
        return {false, -1, error.c_str()};
    }

    runtimeConfig.sendIntervalMs = doc["sendIntervalMs"] | runtimeConfig.sendIntervalMs;
    runtimeConfig.maxBatchSize = doc["maxBatchSize"] | runtimeConfig.maxBatchSize;

    runtimeConfig.sensorCount = 0;
    JsonArrayConst sensors = doc["sensors"].as<JsonArrayConst>();
    for (JsonObjectConst item : sensors) {
        if (runtimeConfig.sensorCount >= MAX_SENSOR_COUNT) {
            break;
        }
        parseSensor(item, runtimeConfig.sensors[runtimeConfig.sensorCount], runtimeConfig.sensorCount);
        ++runtimeConfig.sensorCount;
    }

    runtimeConfig.relayCount = 0;
    JsonArrayConst relays = doc["relays"].as<JsonArrayConst>();
    for (JsonObjectConst item : relays) {
        if (runtimeConfig.relayCount >= MAX_RELAY_COUNT) {
            break;
        }
        parseRelay(item, runtimeConfig.relays[runtimeConfig.relayCount], runtimeConfig.relayCount);
        ++runtimeConfig.relayCount;
    }

    runtimeConfig.isLoaded = true;
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

    JsonDocument responseDocument;
    const DeserializationError error = deserializeJson(responseDocument, responseBody);
    if (error) {
        return {false, -1, error.c_str()};
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
    Serial.printf("[DEBUG] Token: %s\n", _deviceToken.c_str());

    http.addHeader("X-Device-Token", _deviceToken);
    http.addHeader("X-Mac-Address", _macAddress);
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
    sensorConfig.enabled = true;
    sensorConfig.sensorId = item["sensorId"].as<String>();
    sensorConfig.name = item["name"].as<String>();
    sensorConfig.connectionProtocol = item["connectionProtocol"] | 0;
    sensorConfig.connectionAddress = item["connectionAddress"].as<String>();
    sensorConfig.sensorType = item["type"] | 0;
    sensorConfig.unit = item["unit"].as<String>();
    sensorConfig.localKey = "";
    sensorConfig.pollIntervalMs = sensorConfig.name.indexOf("level") >= 0 ? 1000 : 5000;
    sensorConfig.hasValue = false;
    sensorConfig.hasError = false;
    sensorConfig.lastQueuedAtMs = 0;
    sensorConfig.lastPollAtMs = 0;
}

void ApiClient::parseRelay(const JsonObjectConst& item, RelayRuntimeConfig& relayConfig, size_t index) {
    relayConfig.enabled = true;
    relayConfig.channel = static_cast<uint8_t>(index + 1);
    relayConfig.pin = RELAY_PINS[index];
    relayConfig.relayId = item["relayId"].as<String>();
    relayConfig.name = item["name"].as<String>();
    relayConfig.activeLow = !(item["isNormalyOpen"] | false);
    relayConfig.purpose = static_cast<RelayPurpose>(item["purpose"] | 5);
    relayConfig.currentState = item["isActive"] | false;
    relayConfig.backendState = relayConfig.currentState;
    relayConfig.isManual = item["isManual"] | false;
}
