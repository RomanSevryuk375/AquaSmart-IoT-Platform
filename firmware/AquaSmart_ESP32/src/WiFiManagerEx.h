#ifndef WIFI_MANAGER_EX_H
#define WIFI_MANAGER_EX_H

#include <WiFi.h>
#include "AppState.h"

class WiFiManagerEx {
public:
    void begin(const DeviceConfig& config);
    bool update(SystemState& state, ErrorCode& errorCode);
    bool isConnected() const;
    bool hasEverConnected() const;
    IPAddress localIp() const;
    String macAddress() const;

private:
    String _ssid;
    String _password;
    uint32_t _connectStartedAtMs = 0;
    uint32_t _lastAttemptAtMs = 0;
    bool _connectStarted = false;
    bool _hasEverConnected = false;
};

#endif
