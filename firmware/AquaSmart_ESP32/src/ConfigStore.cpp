#include "ConfigStore.h"

#include <ProtocolCodecs.h>
#include <cstring>

#if __has_include("LocalDevSecrets.h")
#include "LocalDevSecrets.h"
#endif

#ifndef LOCAL_DEV_WIFI_SSID
#define LOCAL_DEV_WIFI_SSID "Wokwi-GUEST"
#endif
#ifndef LOCAL_DEV_WIFI_PASS
#define LOCAL_DEV_WIFI_PASS ""
#endif
#ifndef LOCAL_DEV_API_URL
#define LOCAL_DEV_API_URL "http://172.20.10.2:5055"
#endif
#ifndef LOCAL_DEV_MQTT_HOST
#define LOCAL_DEV_MQTT_HOST "172.20.10.2"
#endif
#ifndef LOCAL_DEV_DEVICE_TOKEN
#define LOCAL_DEV_DEVICE_TOKEN "39392510-96dc-4aa4-bc8e-b01310aa9b50"
#endif
#ifndef LOCAL_DEV_CONTROLLER_ID
#define LOCAL_DEV_CONTROLLER_ID "3afee932-b839-439c-b31b-45d83bb5569b"
#endif

Preferences ConfigStore::preferences;

namespace {
constexpr size_t MAX_CACHED_CONFIG_JSON_BYTES = 6144;

void copyString(char* destination, size_t capacity, const String& value) {
    memset(destination, 0, capacity);
    value.toCharArray(destination, capacity);
}
}

bool ConfigStore::begin() {
    return preferences.begin("aqua-config", false);
}

void ConfigStore::load(DeviceConfig& config) {
    copyString(config.wifiSsid, sizeof(config.wifiSsid), preferences.getString("ssid", LOCAL_DEV_WIFI_SSID));
    copyString(config.wifiPass, sizeof(config.wifiPass), preferences.getString("pass", LOCAL_DEV_WIFI_PASS));
    copyString(config.apiBaseUrl, sizeof(config.apiBaseUrl), preferences.getString("url", LOCAL_DEV_API_URL));
    copyString(config.deviceToken, sizeof(config.deviceToken), preferences.getString("token", LOCAL_DEV_DEVICE_TOKEN));
    copyString(config.controllerId, sizeof(config.controllerId), preferences.getString("cid", LOCAL_DEV_CONTROLLER_ID));
    copyString(config.telemetryPath, sizeof(config.telemetryPath), preferences.getString("telemetryPath", DEFAULT_TELEMETRY_PATH));
    copyString(config.mqttHost, sizeof(config.mqttHost), preferences.getString("mqttHost", LOCAL_DEV_MQTT_HOST));
    config.mqttPort = static_cast<uint16_t>(preferences.getUInt("mqttPort", 1883));
    config.mqttUseTls = preferences.getBool("mqttTls", false);
    config.mqttKeepAliveSec = static_cast<uint16_t>(preferences.getUInt("mqttKeepAlive", 30));
    config.mqttHeartbeatIntervalSec = static_cast<uint16_t>(preferences.getUInt("mqttHbSec", 60));
    config.maxMqttBatchSize = static_cast<uint16_t>(preferences.getUInt("mqttBatch", MAX_MQTT_BATCH_SIZE));
    if (config.maxMqttBatchSize == 0 || config.maxMqttBatchSize > MAX_MQTT_BATCH_SIZE) {
        config.maxMqttBatchSize = MAX_MQTT_BATCH_SIZE;
    }

    copyString(config.pendingOtaCampaignId, sizeof(config.pendingOtaCampaignId), preferences.getString("otaCampId", ""));
    copyString(config.pendingOtaVersion, sizeof(config.pendingOtaVersion), preferences.getString("otaVer", ""));

    copyString(config.ds18WaterAddress, sizeof(config.ds18WaterAddress), preferences.getString("ds18water", ""));
    copyString(config.ds18SurfaceAddress, sizeof(config.ds18SurfaceAddress), preferences.getString("ds18surface", ""));

    config.sendIntervalMs = preferences.getUInt("interval", 10000);
    config.maxBatchSize = static_cast<uint16_t>(preferences.getUInt("batch", MAX_HTTP_BATCH_SIZE));
    config.maxQueueSize = static_cast<uint16_t>(preferences.getUInt("queue", MAX_QUEUE_CAPACITY));
    if (config.maxQueueSize == 0 || config.maxQueueSize > MAX_QUEUE_CAPACITY) {
        config.maxQueueSize = MAX_QUEUE_CAPACITY;
    }
    config.commandPollIntervalMs = preferences.getUInt("cmdPoll", 3000);
    config.bootCounter = preferences.getUInt("bootCount", 0) + 1;
    config.soilDryCalibration = static_cast<uint16_t>(preferences.getUInt("soilDry", 3200));
    config.soilWetCalibration = static_cast<uint16_t>(preferences.getUInt("soilWet", 1400));
    config.waterLevelDebounceMs = static_cast<uint16_t>(preferences.getUInt("lvlDebounce", 500));

    for (size_t index = 0; index < MAX_RELAY_COUNT; ++index) {
        const String safeKey = "safe" + String(index);
        const String activeLowKey = "actLow" + String(index);
        config.relaySafeState[index] = preferences.getBool(safeKey.c_str(), false);
        config.relayActiveLow[index] = preferences.getBool(activeLowKey.c_str(), true);
    }
}

void ConfigStore::save(const DeviceConfig& config) {
    preferences.putString("ssid", config.wifiSsid);
    preferences.putString("pass", config.wifiPass);
    preferences.putString("url", config.apiBaseUrl);
    preferences.putString("token", config.deviceToken);
    preferences.putString("cid", config.controllerId);
    preferences.putString("telemetryPath", config.telemetryPath);
    preferences.putString("mqttHost", config.mqttHost);
    preferences.putUInt("mqttPort", config.mqttPort);
    preferences.putBool("mqttTls", config.mqttUseTls);
    preferences.putUInt("mqttKeepAlive", config.mqttKeepAliveSec);
    preferences.putUInt("mqttHbSec", config.mqttHeartbeatIntervalSec);
    preferences.putUInt("mqttBatch", config.maxMqttBatchSize);
    preferences.putString("otaCampId", config.pendingOtaCampaignId);
    preferences.putString("otaVer", config.pendingOtaVersion);
    preferences.putString("ds18water", config.ds18WaterAddress);
    preferences.putString("ds18surface", config.ds18SurfaceAddress);
    preferences.putUInt("interval", config.sendIntervalMs);
    preferences.putUInt("batch", config.maxBatchSize);
    preferences.putUInt("queue", config.maxQueueSize);
    preferences.putUInt("cmdPoll", config.commandPollIntervalMs);
    preferences.putUInt("bootCount", config.bootCounter);
    preferences.putUInt("soilDry", config.soilDryCalibration);
    preferences.putUInt("soilWet", config.soilWetCalibration);
    preferences.putUInt("lvlDebounce", config.waterLevelDebounceMs);

    for (size_t index = 0; index < MAX_RELAY_COUNT; ++index) {
        const String safeKey = "safe" + String(index);
        const String activeLowKey = "actLow" + String(index);
        preferences.putBool(safeKey.c_str(), config.relaySafeState[index]);
        preferences.putBool(activeLowKey.c_str(), config.relayActiveLow[index]);
    }
}

void ConfigStore::savePendingOta(const char* campaignId, const char* version) {
    preferences.putString("otaCampId", campaignId != nullptr ? campaignId : "");
    preferences.putString("otaVer", version != nullptr ? version : "");
}

void ConfigStore::clearPendingOta() {
    preferences.remove("otaCampId");
    preferences.remove("otaVer");
}

bool ConfigStore::saveRuntimeConfigCache(const char* rawJson, uint64_t configVersion) {
    if (rawJson == nullptr) {
        return false;
    }
    const size_t len = std::strlen(rawJson);
    if (len == 0 || len >= MAX_CACHED_CONFIG_JSON_BYTES) {
        return false;
    }

    const size_t written = preferences.putBytes("cfgJson", rawJson, len);
    if (written != len) {
        return false;
    }
    preferences.putULong64("cfgVer", configVersion);
    return true;
}

bool ConfigStore::loadRuntimeConfigCache(BackendRuntimeConfig& outConfig) {
    const size_t len = preferences.getBytesLength("cfgJson");
    if (len == 0 || len >= MAX_CACHED_CONFIG_JSON_BYTES) {
        return false;
    }

    char* buffer = static_cast<char*>(std::malloc(len + 1));
    if (buffer == nullptr) {
        return false;
    }

    const size_t readLen = preferences.getBytes("cfgJson", buffer, len);
    buffer[readLen] = '\0';

    BackendRuntimeConfig parsed{};
    const bool ok = (readLen == len) && ProtocolCodecs::decodeControllerConfig(buffer, parsed);
    std::free(buffer);

    if (!ok) {
        return false;
    }

    if (parsed.configVersion == 0) {
        parsed.configVersion = preferences.getULong64("cfgVer", 0);
    }
    outConfig = parsed;
    return true;
}

void ConfigStore::clearRuntimeConfigCache() {
    preferences.remove("cfgJson");
    preferences.remove("cfgVer");
}

bool ConfigStore::isConfigured() {
    return preferences.isKey("token") && preferences.getString("token").length() > 0;
}

bool ConfigStore::isValid(const DeviceConfig& config) {
    return strlen(config.wifiSsid) > 0 &&
           strlen(config.apiBaseUrl) > 0 &&
           strlen(config.deviceToken) > 0 &&
           strlen(config.controllerId) > 0 &&
           config.maxBatchSize > 0 &&
           config.maxBatchSize <= MAX_HTTP_BATCH_SIZE &&
           config.maxQueueSize > 0 &&
           config.soilDryCalibration > config.soilWetCalibration &&
           config.waterLevelDebounceMs > 0 &&
           config.maxQueueSize <= MAX_QUEUE_CAPACITY;
}

void ConfigStore::reset() {
    preferences.clear();
}
