#include "ConfigStore.h"

#include <cstring>

Preferences ConfigStore::preferences;

namespace {
void copyString(char* destination, size_t capacity, const String& value) {
    memset(destination, 0, capacity);
    value.toCharArray(destination, capacity);
}
}

bool ConfigStore::begin() {
    return preferences.begin("aqua-config", false);
}

void ConfigStore::load(DeviceConfig& config) {
    copyString(config.wifiSsid, sizeof(config.wifiSsid), preferences.getString("ssid", "Your_WiFi"));
    copyString(config.wifiPass, sizeof(config.wifiPass), preferences.getString("pass", "Your_Pass"));
    copyString(config.apiBaseUrl, sizeof(config.apiBaseUrl), preferences.getString("url", "http://192.168.1.100:5237"));
    copyString(config.deviceToken, sizeof(config.deviceToken), preferences.getString("token", ""));
    copyString(config.controllerId, sizeof(config.controllerId), preferences.getString("cid", ""));
    copyString(config.telemetryPath, sizeof(config.telemetryPath), preferences.getString("telemetryPath", DEFAULT_TELEMETRY_PATH));
    copyString(config.ds18WaterAddress, sizeof(config.ds18WaterAddress), preferences.getString("ds18water", ""));
    copyString(config.ds18SurfaceAddress, sizeof(config.ds18SurfaceAddress), preferences.getString("ds18surface", ""));

    config.sendIntervalMs = preferences.getUInt("interval", 10000);
    config.maxBatchSize = static_cast<uint16_t>(preferences.getUInt("batch", 50));
    config.maxQueueSize = static_cast<uint16_t>(preferences.getUInt("queue", 200));
    config.commandPollIntervalMs = preferences.getUInt("cmdPoll", 3000);
    config.bootCounter = preferences.getUInt("bootCount", 0) + 1;
    config.soilDryCalibration = static_cast<uint16_t>(preferences.getUInt("soilDry", 3200));
    config.soilWetCalibration = static_cast<uint16_t>(preferences.getUInt("soilWet", 1200));
    config.waterLevelDebounceMs = static_cast<uint16_t>(preferences.getUInt("lvlDebounce", 150));

    for (size_t index = 0; index < MAX_RELAY_COUNT; ++index) {
        const String key = "safe" + String(index);
        config.relaySafeState[index] = preferences.getBool(key.c_str(), false);
    }
}

void ConfigStore::save(const DeviceConfig& config) {
    preferences.putString("ssid", config.wifiSsid);
    preferences.putString("pass", config.wifiPass);
    preferences.putString("url", config.apiBaseUrl);
    preferences.putString("token", config.deviceToken);
    preferences.putString("cid", config.controllerId);
    preferences.putString("telemetryPath", config.telemetryPath);
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
        const String key = "safe" + String(index);
        preferences.putBool(key.c_str(), config.relaySafeState[index]);
    }
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
           config.maxBatchSize <= 50 &&
           config.maxQueueSize > 0 &&
           config.soilDryCalibration > config.soilWetCalibration &&
           config.waterLevelDebounceMs > 0 &&
           config.maxQueueSize <= MAX_QUEUE_CAPACITY;
}

void ConfigStore::reset() {
    preferences.clear();
}
