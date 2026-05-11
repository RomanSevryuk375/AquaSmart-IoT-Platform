#ifndef RELAY_MANAGER_H
#define RELAY_MANAGER_H

#include "AppState.h"
#include "PinMap.h"

class RelayManager {
public:
    void begin(const DeviceConfig& config);
    void applyRuntimeConfig(BackendRuntimeConfig& runtimeConfig, const DeviceConfig& config);
    void setAllSafe();
    bool setRelayByChannel(uint8_t channel, bool state);
    bool setRelayById(const String& relayId, bool state);
    bool toggleRelayByChannel(uint8_t channel);
    bool getRelayState(uint8_t channel) const;
    size_t getRelayCount() const;
    const RelayRuntimeConfig* getRelayByIndex(size_t index) const;
    uint8_t getChannelByPurpose(RelayPurpose purpose) const;
    void setSafeOverride(bool enabled);

private:
    BackendRuntimeConfig* _runtimeConfig = nullptr;
    bool _safeOverride = false;

    void writePhysical(const RelayRuntimeConfig& relay, bool state);
};

#endif
