#include "RelayManager.h"

void RelayManager::writeArduinoGpio(uint8_t pin, bool highLevel, void*) {
    digitalWrite(pin, highLevel ? HIGH : LOW);
}

void RelayManager::begin(const DeviceConfig& config) {
    for (size_t i = 0; i < MAX_RELAY_COUNT; ++i) {
        pinMode(RELAY_PINS[i], OUTPUT);
    }
    _controller.begin(config, &RelayManager::writeArduinoGpio, nullptr);
}

void RelayManager::applyRuntimeConfig(const BackendRuntimeConfig& runtimeConfig, const DeviceConfig& config) {
    _controller.applyServerConfig(runtimeConfig, config);
}

void RelayManager::setAllSafe() {
    _controller.safeStateAll();
}

bool RelayManager::manualToggle(uint8_t channel, bool isOnline) {
    return _controller.manualToggle(channel, isOnline);
}

bool RelayManager::manualToggleMode(uint8_t channel, bool isOnline) {
    return _controller.manualToggleMode(channel, isOnline);
}

bool RelayManager::setRelayByChannel(uint8_t channel, bool state, RelayChangeSource source) {
    return _controller.setRelayByChannel(channel, state, source);
}

bool RelayManager::setRelayById(const String& relayId, bool state, RelayChangeSource source) {
    return _controller.setRelayById(relayId.c_str(), state, source);
}

CommandApplyStatus RelayManager::applyServerCommand(
    const char* commandId,
    const char* relayId,
    bool targetState,
    int64_t expireAtEpochSec,
    int64_t nowEpochSec,
    char* errorOut,
    size_t errorOutSize) {
    return _controller.applyServerCommand(
        commandId, relayId, targetState, expireAtEpochSec, nowEpochSec, errorOut, errorOutSize);
}

bool RelayManager::getRelayState(uint8_t channel) const {
    return _controller.getRelayState(channel);
}

size_t RelayManager::getRelayCount() const {
    return _controller.channelCount();
}

const RelayRuntimeConfig* RelayManager::getRelayByIndex(size_t index) const {
    return _controller.getChannelByIndex(index);
}

const RelayRuntimeConfig* RelayManager::channels() const {
    return _controller.getChannelByIndex(0);
}

bool RelayManager::hasAnyPendingSync() const {
    return _controller.hasAnyPendingSync();
}

void RelayManager::markAllSynced() {
    _controller.markAllSynced();
}
