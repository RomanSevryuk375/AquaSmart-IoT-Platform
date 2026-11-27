#ifndef RELAY_MANAGER_H
#define RELAY_MANAGER_H

#include <RelayController.h>
#include "AppState.h"
#include "PinMap.h"

class RelayManager {
public:
    void begin(const DeviceConfig& config);
    void applyRuntimeConfig(
        const BackendRuntimeConfig& runtimeConfig,
        const DeviceConfig& config,
        bool applyBackendState = true);
    void setAllSafe();
    bool manualToggle(uint8_t channel, bool isOnline);
    bool manualToggleMode(uint8_t channel, bool isOnline);
    bool setRelayByChannel(uint8_t channel, bool state, RelayChangeSource source = RelayChangeSource::SERVER);
    bool setRelayById(const String& relayId, bool state, RelayChangeSource source = RelayChangeSource::SERVER);
    CommandApplyStatus applyServerCommand(
        const char* commandId,
        const char* relayId,
        bool targetState,
        int64_t expireAtEpochSec,
        int64_t nowEpochSec,
        char* errorOut = nullptr,
        size_t errorOutSize = 0);

    bool getRelayState(uint8_t channel) const;
    size_t getRelayCount() const;
    const RelayRuntimeConfig* getRelayByIndex(size_t index) const;
    const RelayRuntimeConfig* channels() const;
    bool hasAnyPendingSync() const;
    void markChannelSynced(uint8_t channel);
    void markAllSynced();

private:
    RelayController _controller;

    static void writeArduinoGpio(uint8_t pin, bool highLevel, void* context);
};

#endif
