#include "RelayManager.h"

void RelayManager::begin(const DeviceConfig& config) {
    for (size_t index = 0; index < MAX_RELAY_COUNT; ++index) {
        pinMode(RELAY_PINS[index], OUTPUT);
        digitalWrite(RELAY_PINS[index], HIGH);
    }

    for (size_t index = 0; index < MAX_RELAY_COUNT; ++index) {
        const bool safeState = config.relaySafeState[index];
        digitalWrite(RELAY_PINS[index], safeState ? LOW : HIGH);
    }
}

void RelayManager::applyRuntimeConfig(BackendRuntimeConfig& runtimeConfig, const DeviceConfig& config) {
    _runtimeConfig = &runtimeConfig;
    _runtimeConfig->hasHeaterRelay = false;
    _runtimeConfig->hasPumpRelay = false;
    _runtimeConfig->heaterRelayChannel = 0;
    _runtimeConfig->pumpRelayChannel = 0;

    for (size_t index = 0; index < _runtimeConfig->relayCount; ++index) {
        auto& relay = _runtimeConfig->relays[index];
        relay.channel = index + 1;
        relay.pin = RELAY_PINS[index];
        relay.safeState = config.relaySafeState[index];
        writePhysical(relay, relay.safeState);
        relay.currentState = relay.safeState;

        if (relay.purpose == RelayPurpose::HEATING || relay.purpose == RelayPurpose::BOILER) {
            _runtimeConfig->hasHeaterRelay = true;
            _runtimeConfig->heaterRelayChannel = relay.channel;
        }

        if (relay.purpose == RelayPurpose::PUMP) {
            _runtimeConfig->hasPumpRelay = true;
            _runtimeConfig->pumpRelayChannel = relay.channel;
        }
    }
}

void RelayManager::setAllSafe() {
    if (_runtimeConfig == nullptr) {
        for (size_t index = 0; index < MAX_RELAY_COUNT; ++index) {
            digitalWrite(RELAY_PINS[index], HIGH);
        }
        return;
    }

    for (size_t index = 0; index < _runtimeConfig->relayCount; ++index) {
        auto& relay = _runtimeConfig->relays[index];
        writePhysical(relay, relay.safeState);
        relay.currentState = relay.safeState;
    }
}

bool RelayManager::setRelayByChannel(uint8_t channel, bool state) {
    if (_runtimeConfig == nullptr || channel == 0 || channel > _runtimeConfig->relayCount) {
        return false;
    }

    auto& relay = _runtimeConfig->relays[channel - 1];
    if (_safeOverride && state != relay.safeState) {
        state = relay.safeState;
    }

    writePhysical(relay, state);
    relay.currentState = state;
    return true;
}

bool RelayManager::setRelayById(const String& relayId, bool state) {
    if (_runtimeConfig == nullptr) {
        return false;
    }

    for (size_t index = 0; index < _runtimeConfig->relayCount; ++index) {
        auto& relay = _runtimeConfig->relays[index];
        if (relay.relayId == relayId) {
            return setRelayByChannel(relay.channel, state);
        }
    }

    return false;
}

bool RelayManager::toggleRelayByChannel(uint8_t channel) {
    return setRelayByChannel(channel, !getRelayState(channel));
}

bool RelayManager::getRelayState(uint8_t channel) const {
    if (_runtimeConfig == nullptr || channel == 0 || channel > _runtimeConfig->relayCount) {
        return false;
    }

    return _runtimeConfig->relays[channel - 1].currentState;
}

size_t RelayManager::getRelayCount() const {
    return _runtimeConfig == nullptr ? 0 : _runtimeConfig->relayCount;
}

const RelayRuntimeConfig* RelayManager::getRelayByIndex(size_t index) const {
    if (_runtimeConfig == nullptr || index >= _runtimeConfig->relayCount) {
        return nullptr;
    }

    return &_runtimeConfig->relays[index];
}

uint8_t RelayManager::getChannelByPurpose(RelayPurpose purpose) const {
    if (_runtimeConfig == nullptr) {
        return 0;
    }

    for (size_t index = 0; index < _runtimeConfig->relayCount; ++index) {
        if (_runtimeConfig->relays[index].purpose == purpose) {
            return _runtimeConfig->relays[index].channel;
        }
    }

    return 0;
}

void RelayManager::setSafeOverride(bool enabled) {
    _safeOverride = enabled;
    if (enabled) {
        setAllSafe();
    }
}

void RelayManager::writePhysical(const RelayRuntimeConfig& relay, bool state) {
    const bool level = relay.activeLow ? !state : state;
    digitalWrite(relay.pin, level ? HIGH : LOW);
}
