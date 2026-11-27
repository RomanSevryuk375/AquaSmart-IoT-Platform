#ifndef AQUA_CORE_RELAY_CONTROLLER_H
#define AQUA_CORE_RELAY_CONTROLLER_H

#include "CoreTypes.h"

enum class CommandApplyStatus : uint8_t {
    APPLIED = 0,
    DUPLICATE = 1,
    EXPIRED = 2,
    UNKNOWN_RELAY = 3,
    INVALID_ARGUMENT = 4
};

using GpioWriteCallback = void (*)(uint8_t pin, bool highLevel, void* context);

class RelayController {
public:
    void begin(const DeviceConfig& config, GpioWriteCallback gpioWrite = nullptr, void* gpioContext = nullptr);
    void applyServerConfig(
        const BackendRuntimeConfig& serverConfig,
        const DeviceConfig& deviceConfig,
        bool applyBackendState = true);

    bool manualToggle(uint8_t channel, bool isOnline);
    bool manualToggleMode(uint8_t channel, bool isOnline);
    void safeStateAll();

    bool setRelayByChannel(uint8_t channel, bool state, RelayChangeSource source);
    bool setRelayById(const char* relayId, bool state, RelayChangeSource source);

    CommandApplyStatus applyServerCommand(
        const char* commandId,
        const char* relayId,
        bool targetState,
        int64_t expireAtEpochSec,
        int64_t nowEpochSec,
        char* errorOut = nullptr,
        size_t errorOutSize = 0);

    bool wasCommandProcessed(const char* commandId) const;

    bool getRelayState(uint8_t channel) const;
    bool getRelayIsManual(uint8_t channel) const;
    const RelayRuntimeConfig* getChannel(uint8_t channel) const;
    const RelayRuntimeConfig* getChannelByIndex(size_t index) const;
    size_t channelCount() const { return MAX_RELAY_COUNT; }

    bool hasAnyPendingSync() const;
    void markChannelSynced(uint8_t channel);
    void markAllSynced();

    static uint8_t parseChannelFromAddress(const char* connectionAddress);

private:
    RelayRuntimeConfig _channels[MAX_RELAY_COUNT] = {};
    GpioWriteCallback _gpioWrite = nullptr;
    void* _gpioContext = nullptr;

    char _commandHistory[MAX_COMMAND_HISTORY][40] = {};
    size_t _commandHistoryHead = 0;
    size_t _commandHistoryCount = 0;

    void writePhysical(const RelayRuntimeConfig& ch, bool logicalState) const;
    void rememberCommandId(const char* commandId);
};

#endif
