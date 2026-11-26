#include "ProtocolCodecs.h"

#include <ArduinoJson.h>
#include <cctype>
#include <cstdio>
#include <cstring>

namespace ProtocolCodecs {

namespace {
bool isHexChar(char c) {
    return (c >= '0' && c <= '9') ||
           (c >= 'a' && c <= 'f') ||
           (c >= 'A' && c <= 'F');
}

int64_t daysFromCivil(int y, unsigned m, unsigned d) {
    y -= m <= 2;
    const int era = (y >= 0 ? y : y - 399) / 400;
    const unsigned yoe = static_cast<unsigned>(y - era * 400);
    const unsigned doy = (153 * (m + (m > 2 ? -3 : 9)) + 2) / 5 + d - 1;
    const unsigned doe = yoe * 365 + yoe / 4 - yoe / 100 + doy;
    return static_cast<int64_t>(era) * 146097 + static_cast<int64_t>(doe) - 719468;
}
}  // namespace

bool normalizeMacCompact(const char* rawMac, char* outCompact, size_t outSize) {
    if (rawMac == nullptr || outCompact == nullptr || outSize < 13) {
        return false;
    }

    size_t idx = 0;
    for (const char* p = rawMac; *p != '\0'; ++p) {
        if (*p == ':' || *p == '-' || std::isspace(static_cast<unsigned char>(*p))) {
            continue;
        }
        if (!isHexChar(*p) || idx >= 12) {
            return false;
        }
        outCompact[idx++] = static_cast<char>(std::toupper(static_cast<unsigned char>(*p)));
    }

    if (idx != 12) {
        outCompact[0] = '\0';
        return false;
    }

    outCompact[12] = '\0';
    return true;
}

bool normalizeMacCanonical(const char* rawMac, char* outCanonical, size_t outSize) {
    if (outCanonical == nullptr || outSize < 18) {
        return false;
    }

    char compact[13] = {};
    if (!normalizeMacCompact(rawMac, compact, sizeof(compact))) {
        outCanonical[0] = '\0';
        return false;
    }

    std::snprintf(
        outCanonical,
        outSize,
        "%c%c:%c%c:%c%c:%c%c:%c%c:%c%c",
        compact[0], compact[1], compact[2], compact[3],
        compact[4], compact[5], compact[6], compact[7],
        compact[8], compact[9], compact[10], compact[11]);
    return true;
}

bool buildDeviceTopic(char* outTopic, size_t outSize, const char* macAddress, const char* suffix) {
    if (outTopic == nullptr || outSize == 0 || suffix == nullptr || suffix[0] == '\0') {
        return false;
    }

    char compact[13] = {};
    if (!normalizeMacCompact(macAddress, compact, sizeof(compact))) {
        outTopic[0] = '\0';
        return false;
    }

    const int written = std::snprintf(outTopic, outSize, "aquasmart/devices/%s/%s", compact, suffix);
    return written > 0 && static_cast<size_t>(written) < outSize;
}

bool buildClientId(char* outClientId, size_t outSize, const char* macAddress) {
    if (outClientId == nullptr || outSize == 0) {
        return false;
    }

    char compact[13] = {};
    if (!normalizeMacCompact(macAddress, compact, sizeof(compact))) {
        outClientId[0] = '\0';
        return false;
    }

    const int written = std::snprintf(outClientId, outSize, "aqua-esp32-%s", compact);
    return written > 0 && static_cast<size_t>(written) < outSize;
}

size_t encodeTelemetryPayload(
    const char* canonicalMac,
    const TelemetryRecord* records,
    size_t count,
    char* outJson,
    size_t outSize) {
    if (outJson == nullptr || outSize == 0 || records == nullptr || count == 0) {
        return 0;
    }

    JsonDocument doc;
    doc["macAddress"] = (canonicalMac != nullptr && canonicalMac[0] != '\0') ? canonicalMac : "00:00:00:00:00:00";
    JsonArray items = doc["items"].to<JsonArray>();

    for (size_t i = 0; i < count; ++i) {
        JsonObject item = items.add<JsonObject>();
        item["sensorId"] = records[i].sensorId;
        item["value"] = records[i].value;
        item["externalMessageId"] = records[i].externalMessageId;
        item["recordedAt"] = records[i].recordedAt;
    }

    const size_t required = measureJson(doc);
    if (required >= outSize) {
        outJson[0] = '\0';
        return 0;
    }

    return serializeJson(doc, outJson, outSize);
}

size_t encodeCommandAckPayload(
    const char* commandId,
    bool completed,
    const char* errorMessage,
    const char* executedAtUtc,
    char* outJson,
    size_t outSize) {
    if (outJson == nullptr || outSize == 0 || commandId == nullptr || commandId[0] == '\0') {
        return 0;
    }

    JsonDocument doc;
    doc["commandId"] = commandId;
    doc["status"] = completed ? "completed" : "failed";
    if (completed || errorMessage == nullptr || errorMessage[0] == '\0') {
        doc["errorMessage"] = nullptr;
    } else {
        doc["errorMessage"] = errorMessage;
    }
    doc["executedAt"] = (executedAtUtc != nullptr) ? executedAtUtc : "";

    const size_t required = measureJson(doc);
    if (required >= outSize) {
        outJson[0] = '\0';
        return 0;
    }

    return serializeJson(doc, outJson, outSize);
}

size_t encodeRelaysStatePayload(
    const char* canonicalMac,
    const char* source,
    const char* recordedAtUtc,
    const RelaySyncItem* items,
    size_t count,
    char* outJson,
    size_t outSize) {
    if (outJson == nullptr || outSize == 0 || source == nullptr || items == nullptr || count == 0) {
        return 0;
    }

    JsonDocument doc;
    doc["macAddress"] = (canonicalMac != nullptr && canonicalMac[0] != '\0') ? canonicalMac : "00:00:00:00:00:00";
    doc["source"] = source;
    doc["recordedAt"] = (recordedAtUtc != nullptr) ? recordedAtUtc : "";

    JsonArray relays = doc["relays"].to<JsonArray>();
    for (size_t i = 0; i < count; ++i) {
        JsonObject obj = relays.add<JsonObject>();
        obj["relayId"] = items[i].relayId;
        obj["state"] = items[i].state;
        obj["isManual"] = items[i].isManual;
    }

    const size_t required = measureJson(doc);
    if (required >= outSize) {
        outJson[0] = '\0';
        return 0;
    }

    return serializeJson(doc, outJson, outSize);
}

size_t encodeStatusOnlinePayload(
    const char* fwVersion,
    const char* ip,
    int rssi,
    uint32_t uptimeSec,
    const char* timestampUtc,
    char* outJson,
    size_t outSize) {
    if (outJson == nullptr || outSize == 0) {
        return 0;
    }

    JsonDocument doc;
    doc["status"] = "online";
    doc["fwVersion"] = (fwVersion != nullptr) ? fwVersion : FIRMWARE_VERSION;
    doc["ip"] = (ip != nullptr) ? ip : "0.0.0.0";
    doc["rssi"] = rssi;
    doc["uptimeSec"] = uptimeSec;
    doc["timestamp"] = (timestampUtc != nullptr) ? timestampUtc : "";

    const size_t required = measureJson(doc);
    if (required >= outSize) {
        outJson[0] = '\0';
        return 0;
    }

    return serializeJson(doc, outJson, outSize);
}

size_t encodeStatusOfflineLwt(char* outJson, size_t outSize) {
    if (outJson == nullptr || outSize == 0) {
        return 0;
    }

    JsonDocument doc;
    doc["status"] = "offline";

    const size_t required = measureJson(doc);
    if (required >= outSize) {
        outJson[0] = '\0';
        return 0;
    }

    return serializeJson(doc, outJson, outSize);
}

bool decodeCommandPayload(const char* json, MqttRelayCommand& outCmd) {
    outCmd = MqttRelayCommand{};
    if (json == nullptr || json[0] == '\0') {
        return false;
    }

    JsonDocument doc;
    if (deserializeJson(doc, json)) {
        return false;
    }

    const char* commandId = doc["commandId"] | "";
    const char* relayId = doc["relayId"] | "";
    if (commandId[0] == '\0' || relayId[0] == '\0' || !doc["targetState"].is<bool>()) {
        return false;
    }

    copyCString(outCmd.commandId, sizeof(outCmd.commandId), commandId);
    copyCString(outCmd.relayId, sizeof(outCmd.relayId), relayId);
    outCmd.targetState = doc["targetState"].as<bool>();
    copyCString(outCmd.expireAt, sizeof(outCmd.expireAt), doc["expireAt"] | "");
    copyCString(outCmd.createdAt, sizeof(outCmd.createdAt), doc["createdAt"] | "");

    if (outCmd.expireAt[0] != '\0') {
        parseIso8601UtcToEpoch(outCmd.expireAt, outCmd.expireAtEpochSec);
    }

    return true;
}

bool decodeConfigNotifyPayload(const char* json, ConfigNotifyMessage& outNotify) {
    outNotify = ConfigNotifyMessage{};
    if (json == nullptr || json[0] == '\0') {
        return false;
    }

    JsonDocument doc;
    if (deserializeJson(doc, json)) {
        return false;
    }

    if (!doc["version"].is<uint64_t>() && !doc["version"].is<int64_t>()) {
        return false;
    }

    const int64_t verSigned = doc["version"].as<int64_t>();
    if (verSigned < 0) {
        return false;
    }

    outNotify.version = static_cast<uint64_t>(verSigned);
    copyCString(outNotify.reason, sizeof(outNotify.reason), doc["reason"] | "");
    copyCString(outNotify.timestamp, sizeof(outNotify.timestamp), doc["timestamp"] | "");
    return true;
}

bool shouldTriggerConfigSync(uint64_t incomingVersion, uint64_t localConfigVersion) {
    return incomingVersion > localConfigVersion;
}

bool decodeControllerConfig(const char* json, BackendRuntimeConfig& outConfig) {
    if (json == nullptr || json[0] == '\0') {
        return false;
    }

    JsonDocument doc;
    if (deserializeJson(doc, json)) {
        return false;
    }

    outConfig.configVersion = doc["configVersion"] | outConfig.configVersion;
    outConfig.sendIntervalMs = doc["sendIntervalMs"] | outConfig.sendIntervalMs;
    outConfig.maxBatchSize = doc["maxBatchSize"] | outConfig.maxBatchSize;

    outConfig.sensorCount = 0;
    JsonArrayConst sensors = doc["sensors"].as<JsonArrayConst>();
    for (JsonObjectConst item : sensors) {
        if (outConfig.sensorCount >= MAX_SENSOR_COUNT) {
            break;
        }
        SensorRuntimeConfig& s = outConfig.sensors[outConfig.sensorCount];
        s = SensorRuntimeConfig{};
        s.enabled = true;
        copyCString(s.sensorId, sizeof(s.sensorId), item["sensorId"] | "");
        copyCString(s.name, sizeof(s.name), item["name"] | "");
        s.connectionProtocol = item["connectionProtocol"] | 0;
        copyCString(s.connectionAddress, sizeof(s.connectionAddress), item["connectionAddress"] | "");
        s.sensorType = item["type"] | 0;
        copyCString(s.unit, sizeof(s.unit), item["unit"] | "");
        s.pollIntervalMs = 5000;
        ++outConfig.sensorCount;
    }

    outConfig.relayCount = 0;
    JsonArrayConst relays = doc["relays"].as<JsonArrayConst>();
    for (JsonObjectConst item : relays) {
        if (outConfig.relayCount >= MAX_RELAY_COUNT) {
            break;
        }
        RelayRuntimeConfig& r = outConfig.relays[outConfig.relayCount];
        r = RelayRuntimeConfig{};
        r.configured = true;
        r.channel = static_cast<uint8_t>(outConfig.relayCount + 1);
        r.pin = CORE_RELAY_PINS[outConfig.relayCount];
        copyCString(r.relayId, sizeof(r.relayId), item["relayId"] | "");
        copyCString(r.name, sizeof(r.name), item["name"] | "");
        copyCString(r.connectionAddress, sizeof(r.connectionAddress), item["connectionAddress"] | "");
        const bool isNormallyOpen = item["isNormallyOpen"].is<bool>()
            ? item["isNormallyOpen"].as<bool>()
            : (item["isNormalyOpen"] | true);
        r.activeLow = isNormallyOpen;
        r.purpose = static_cast<RelayPurpose>(item["purpose"] | static_cast<int>(RelayPurpose::RESERVE));
        r.backendState = item["isActive"] | false;
        r.currentState = r.backendState;
        r.isManual = item["isManual"] | false;
        ++outConfig.relayCount;
    }

    outConfig.isLoaded = true;
    return true;
}

bool parseIso8601UtcToEpoch(const char* isoUtc, int64_t& outEpochSec) {
    outEpochSec = 0;
    if (isoUtc == nullptr || std::strlen(isoUtc) < 19) {
        return false;
    }

    int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0;
    if (std::sscanf(isoUtc, "%4d-%2d-%2dT%2d:%2d:%2d", &year, &month, &day, &hour, &minute, &second) != 6) {
        return false;
    }

    if (year < 1970 || month < 1 || month > 12 || day < 1 || day > 31 ||
        hour < 0 || hour > 23 || minute < 0 || minute > 59 || second < 0 || second > 60) {
        return false;
    }

    const int64_t days = daysFromCivil(year, static_cast<unsigned>(month), static_cast<unsigned>(day));
    outEpochSec = days * 86400LL + hour * 3600LL + minute * 60LL + second;
    return true;
}

}  // namespace ProtocolCodecs
