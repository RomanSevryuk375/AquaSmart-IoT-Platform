#ifndef CONFIG_STORE_H
#define CONFIG_STORE_H

#include <Preferences.h>
#include "AppState.h"

class ConfigStore {
public:
    static bool begin();
    static void load(DeviceConfig& config);
    static void save(const DeviceConfig& config);
    static void savePendingOta(const char* campaignId, const char* version);
    static void clearPendingOta();
    static bool saveRuntimeConfigCache(const char* rawJson, uint64_t configVersion);
    static bool loadRuntimeConfigCache(BackendRuntimeConfig& outConfig);
    static void clearRuntimeConfigCache();
    static bool isConfigured();
    static bool isValid(const DeviceConfig& config);
    static void reset();

private:
    static Preferences preferences;
};

#endif
