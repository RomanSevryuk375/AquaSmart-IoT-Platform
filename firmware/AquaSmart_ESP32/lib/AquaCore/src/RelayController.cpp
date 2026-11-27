#include "RelayController.h"

#include <cctype>
#include <cstdlib>
#include <cstring>

void RelayController::begin(const DeviceConfig& config, GpioWriteCallback gpioWrite, void* gpioContext) {
    _gpioWrite = gpioWrite;
    _gpioContext = gpioContext;
    _commandHistoryHead = 0;
    _commandHistoryCount = 0;

    for (size_t i = 0; i < MAX_RELAY_COUNT; ++i) {
        RelayRuntimeConfig& ch = _channels[i];
        ch = RelayRuntimeConfig{};
        ch.configured = false;
        ch.channel = static_cast<uint8_t>(i + 1);
        ch.pin = CORE_RELAY_PINS[i];
        ch.activeLow = config.relayActiveLow[i];
        ch.safeState = config.relaySafeState[i];
        ch.currentState = ch.safeState;
        ch.backendState = ch.safeState;
        ch.isManual = true;
        ch.pendingSync = false;
        ch.lastChangeSource = RelayChangeSource::BOOT;
        ch.purpose = RelayPurpose::RESERVE;
        writePhysical(ch, ch.safeState);
    }
}

void RelayController::applyServerConfig(
    const BackendRuntimeConfig& serverConfig,
    const DeviceConfig& deviceConfig,
    bool applyBackendState) {
    bool bound[MAX_RELAY_COUNT] = {};

    for (size_t i = 0; i < MAX_RELAY_COUNT; ++i) {
        _channels[i].activeLow = deviceConfig.relayActiveLow[i];
        _channels[i].safeState = deviceConfig.relaySafeState[i];
    }

    for (size_t i = 0; i < serverConfig.relayCount && i < MAX_RELAY_COUNT; ++i) {
        const RelayRuntimeConfig& src = serverConfig.relays[i];
        uint8_t targetChannel = parseChannelFromAddress(src.connectionAddress);
        if (targetChannel == 0 || targetChannel > MAX_RELAY_COUNT || bound[targetChannel - 1]) {
            targetChannel = static_cast<uint8_t>(i + 1);
            if (bound[targetChannel - 1]) {
                for (size_t k = 0; k < MAX_RELAY_COUNT; ++k) {
                    if (!bound[k]) {
                        targetChannel = static_cast<uint8_t>(k + 1);
                        break;
                    }
                }
            }
        }

        const size_t idx = static_cast<size_t>(targetChannel - 1);
        bound[idx] = true;

        RelayRuntimeConfig& dst = _channels[idx];
        dst.configured = true;
        dst.purpose = src.purpose;
        dst.backendState = src.backendState;
        copyCString(dst.relayId, sizeof(dst.relayId), src.relayId);
        copyCString(dst.name, sizeof(dst.name), src.name);
        copyCString(dst.connectionAddress, sizeof(dst.connectionAddress), src.connectionAddress);

        if (!dst.pendingSync) {
            dst.isManual = src.isManual;
            if (applyBackendState) {
                dst.currentState = src.backendState;
                dst.lastChangeSource = RelayChangeSource::SYNC;
                writePhysical(dst, dst.currentState);
            }
        }
    }

    for (size_t i = 0; i < MAX_RELAY_COUNT; ++i) {
        if (!bound[i]) {
            _channels[i].configured = false;
            _channels[i].relayId[0] = '\0';
            _channels[i].name[0] = '\0';
            _channels[i].connectionAddress[0] = '\0';
        }
    }
}

bool RelayController::manualToggle(uint8_t channel, bool isOnline) {
    if (channel == 0 || channel > MAX_RELAY_COUNT) {
        return false;
    }

    RelayRuntimeConfig& ch = _channels[channel - 1];
    ch.currentState = !ch.currentState;
    ch.isManual = true;
    ch.lastChangeSource = RelayChangeSource::MANUAL;
    if (!isOnline) {
        ch.pendingSync = true;
    }
    writePhysical(ch, ch.currentState);
    return true;
}

bool RelayController::manualToggleMode(uint8_t channel, bool isOnline) {
    if (channel == 0 || channel > MAX_RELAY_COUNT) {
        return false;
    }

    RelayRuntimeConfig& ch = _channels[channel - 1];
    ch.isManual = !ch.isManual;
    ch.lastChangeSource = RelayChangeSource::MANUAL;
    if (!isOnline) {
        ch.pendingSync = true;
    }
    // Physical state and pin MUST remain unchanged when toggling MAN <-> AUTO.
    return true;
}

void RelayController::safeStateAll() {
    for (size_t i = 0; i < MAX_RELAY_COUNT; ++i) {
        RelayRuntimeConfig& ch = _channels[i];
        ch.currentState = ch.safeState;
        ch.lastChangeSource = RelayChangeSource::MANUAL;
        ch.pendingSync = true;
        writePhysical(ch, ch.safeState);
    }
}

bool RelayController::setRelayByChannel(uint8_t channel, bool state, RelayChangeSource source) {
    if (channel == 0 || channel > MAX_RELAY_COUNT) {
        return false;
    }

    RelayRuntimeConfig& ch = _channels[channel - 1];
    ch.currentState = state;
    ch.lastChangeSource = source;
    writePhysical(ch, state);
    return true;
}

bool RelayController::setRelayById(const char* relayId, bool state, RelayChangeSource source) {
    if (relayId == nullptr || relayId[0] == '\0') {
        return false;
    }

    for (size_t i = 0; i < MAX_RELAY_COUNT; ++i) {
        if (_channels[i].configured && std::strcmp(_channels[i].relayId, relayId) == 0) {
            _channels[i].backendState = state;
            return setRelayByChannel(_channels[i].channel, state, source);
        }
    }

    return false;
}

CommandApplyStatus RelayController::applyServerCommand(
    const char* commandId,
    const char* relayId,
    bool targetState,
    int64_t expireAtEpochSec,
    int64_t nowEpochSec,
    char* errorOut,
    size_t errorOutSize) {
    if (errorOut != nullptr && errorOutSize > 0) {
        errorOut[0] = '\0';
    }

    if (commandId == nullptr || commandId[0] == '\0' || relayId == nullptr || relayId[0] == '\0') {
        copyCString(errorOut, errorOutSize, "Missing commandId or relayId");
        return CommandApplyStatus::INVALID_ARGUMENT;
    }

    if (wasCommandProcessed(commandId)) {
        return CommandApplyStatus::DUPLICATE;
    }

    if (expireAtEpochSec > 0 && nowEpochSec > 0 && expireAtEpochSec < nowEpochSec) {
        copyCString(errorOut, errorOutSize, "Command expired");
        return CommandApplyStatus::EXPIRED;
    }

    if (!setRelayById(relayId, targetState, RelayChangeSource::SERVER)) {
        copyCString(errorOut, errorOutSize, "Unknown relayId");
        return CommandApplyStatus::UNKNOWN_RELAY;
    }

    rememberCommandId(commandId);
    return CommandApplyStatus::APPLIED;
}

bool RelayController::wasCommandProcessed(const char* commandId) const {
    if (commandId == nullptr || commandId[0] == '\0') {
        return false;
    }
    for (size_t i = 0; i < _commandHistoryCount; ++i) {
        if (std::strcmp(_commandHistory[i], commandId) == 0) {
            return true;
        }
    }
    return false;
}

bool RelayController::getRelayState(uint8_t channel) const {
    if (channel == 0 || channel > MAX_RELAY_COUNT) {
        return false;
    }
    return _channels[channel - 1].currentState;
}

bool RelayController::getRelayIsManual(uint8_t channel) const {
    if (channel == 0 || channel > MAX_RELAY_COUNT) {
        return true;
    }
    return _channels[channel - 1].isManual;
}

const RelayRuntimeConfig* RelayController::getChannel(uint8_t channel) const {
    if (channel == 0 || channel > MAX_RELAY_COUNT) {
        return nullptr;
    }
    return &_channels[channel - 1];
}

const RelayRuntimeConfig* RelayController::getChannelByIndex(size_t index) const {
    if (index >= MAX_RELAY_COUNT) {
        return nullptr;
    }
    return &_channels[index];
}

bool RelayController::hasAnyPendingSync() const {
    for (size_t i = 0; i < MAX_RELAY_COUNT; ++i) {
        if (_channels[i].pendingSync) {
            return true;
        }
    }
    return false;
}

void RelayController::markChannelSynced(uint8_t channel) {
    if (channel > 0 && channel <= MAX_RELAY_COUNT) {
        _channels[channel - 1].pendingSync = false;
    }
}

void RelayController::markAllSynced() {
    for (size_t i = 0; i < MAX_RELAY_COUNT; ++i) {
        _channels[i].pendingSync = false;
    }
}

uint8_t RelayController::parseChannelFromAddress(const char* connectionAddress) {
    if (connectionAddress == nullptr || connectionAddress[0] == '\0') {
        return 0;
    }

    const char* p = connectionAddress;
    while (*p != '\0' && std::isspace(static_cast<unsigned char>(*p))) {
        ++p;
    }

    // Strip optional non-digit prefixes like "CH", "GPIO", "D", "PIN"
    while (*p != '\0' && !std::isdigit(static_cast<unsigned char>(*p))) {
        ++p;
    }

    if (*p == '\0') {
        return 0;
    }

    char* endPtr = nullptr;
    const long num = std::strtol(p, &endPtr, 10);
    if (endPtr == p || num <= 0 || num > 255) {
        return 0;
    }

    // First check exact GPIO pin match against RELAY_PINS[0..7]
    for (size_t i = 0; i < MAX_RELAY_COUNT; ++i) {
        if (CORE_RELAY_PINS[i] == static_cast<uint8_t>(num)) {
            return static_cast<uint8_t>(i + 1);
        }
    }

    // Otherwise check direct channel number 1..8
    if (num >= 1 && num <= static_cast<long>(MAX_RELAY_COUNT)) {
        return static_cast<uint8_t>(num);
    }

    return 0;
}

void RelayController::writePhysical(const RelayRuntimeConfig& ch, bool logicalState) const {
    if (_gpioWrite == nullptr) {
        return;
    }
    const bool highLevel = ch.activeLow ? !logicalState : logicalState;
    _gpioWrite(ch.pin, highLevel, _gpioContext);
}

void RelayController::rememberCommandId(const char* commandId) {
    if (wasCommandProcessed(commandId)) {
        return;
    }
    copyCString(_commandHistory[_commandHistoryHead], sizeof(_commandHistory[0]), commandId);
    _commandHistoryHead = (_commandHistoryHead + 1) % MAX_COMMAND_HISTORY;
    if (_commandHistoryCount < MAX_COMMAND_HISTORY) {
        ++_commandHistoryCount;
    }
}
