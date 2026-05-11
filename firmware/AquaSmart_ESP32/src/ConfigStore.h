#ifndef CONFIG_STORE_H
#define CONFIG_STORE_H

#include <Preferences.h>
#include "AppState.h"

class ConfigStore {
public:
    static bool begin();
    static void load(DeviceConfig& config);
    static void save(const DeviceConfig& config);
    static bool isConfigured();
    static bool isValid(const DeviceConfig& config);
    static void reset();

private:
    static Preferences preferences;
};

#endif
