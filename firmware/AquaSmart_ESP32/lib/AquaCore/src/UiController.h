#ifndef AQUA_CORE_UI_CONTROLLER_H
#define AQUA_CORE_UI_CONTROLLER_H

#include "CoreTypes.h"

enum class UiLevel : uint8_t {
    DASHBOARD = 0,
    ROOT_MENU = 1,
    SUBMENU = 2
};

enum class MenuSection : uint8_t {
    SENSORS = 0,
    RELAYS = 1,
    NETWORK = 2,
    QUEUE_TIME = 3,
    ERRORS = 4,
    SYSTEM = 5
};

constexpr size_t ROOT_MENU_ITEM_COUNT = 6;
constexpr size_t NETWORK_SUBSCREEN_COUNT = 3;
constexpr size_t QUEUE_TIME_SUBSCREEN_COUNT = 4;
constexpr size_t SYSTEM_SUBSCREEN_COUNT = 4;

enum class NetworkSubScreen : uint8_t {
    WIFI = 0,
    MQTT = 1,
    HTTP = 2
};

enum class QueueTimeSubScreen : uint8_t {
    TIME = 0,
    QUEUE = 1,
    SYNC_CONFIG = 2,
    FLUSH_QUEUE = 3
};

enum class SystemSubScreen : uint8_t {
    OTA_CHECK = 0,
    SAFE_STATE_ALL = 1,
    REBOOT = 2,
    FACTORY_RESET = 3
};

enum class UiActionType : uint8_t {
    NONE = 0,
    TOGGLE_RELAY_STATE,
    TOGGLE_RELAY_MODE,
    SYNC_CONFIG,
    FLUSH_QUEUE,
    CLEAR_NON_CRITICAL_ERRORS,
    OTA_CHECK,
    SAFE_STATE_ALL,
    REBOOT,
    FACTORY_RESET
};

struct UiAction {
    UiActionType type = UiActionType::NONE;
    uint8_t relayChannel = 0;  // 1..8 for relay actions
};

// Pure button hold state machine: short fires on RELEASE (before threshold),
// long fires ONCE when held >= longThresholdMs — prevents false short press on long hold!
class ButtonHoldDetector {
public:
    explicit ButtonHoldDetector(uint32_t longThresholdMs)
        : _longThresholdMs(longThresholdMs) {}

    void reset() {
        _pressed = false;
        _longFired = false;
        _pressedAtMs = 0;
    }

    // Returns: 0 = nothing, 1 = short press (on release), 2 = long press (at threshold)
    uint8_t update(bool isPressedNow, uint32_t nowMs) {
        if (isPressedNow && !_pressed) {
            _pressed = true;
            _longFired = false;
            _pressedAtMs = nowMs;
            return 0;
        }

        if (isPressedNow && _pressed) {
            if (!_longFired && (nowMs - _pressedAtMs >= _longThresholdMs)) {
                _longFired = true;
                return 2;
            }
            return 0;
        }

        if (!isPressedNow && _pressed) {
            _pressed = false;
            const bool wasLong = _longFired;
            _longFired = false;
            return wasLong ? 0 : 1;
        }

        return 0;
    }

private:
    uint32_t _longThresholdMs;
    bool _pressed = false;
    bool _longFired = false;
    uint32_t _pressedAtMs = 0;
};

class UiController {
public:
    void begin();

    UiAction handleInput(
        const InputEvent& event,
        size_t sensorCount,
        size_t activeErrorCount);

    UiLevel level() const { return _level; }
    MenuSection selectedSection() const { return static_cast<MenuSection>(_rootIndex); }
    size_t rootIndex() const { return _rootIndex; }
    size_t selectedSensorIndex() const { return _sensorIndex; }
    size_t selectedRelayIndex() const { return _relayIndex; }
    size_t networkSubIndex() const { return _networkIndex; }
    size_t queueTimeSubIndex() const { return _queueTimeIndex; }
    size_t errorSubIndex() const { return _errorIndex; }
    size_t systemSubIndex() const { return _systemIndex; }

    bool isOnFactoryResetScreen() const;

private:
    UiLevel _level = UiLevel::DASHBOARD;
    size_t _rootIndex = 0;
    size_t _sensorIndex = 0;
    size_t _relayIndex = 0;
    size_t _networkIndex = 0;
    size_t _queueTimeIndex = 0;
    size_t _errorIndex = 0;
    size_t _systemIndex = 0;
};

struct LcdLines {
    char line0[17] = {};
    char line1[17] = {};
};

struct LcdViewContext {
    SystemState systemState = SystemState::BOOT;
    bool wifiConnected = false;
    bool mqttConnected = false;
    bool httpHealthy = false;
    int lastHttpStatus = 0;
    int wifiRssi = 0;
    uint32_t mqttReconnectCount = 0;
    char wifiSsid[32] = {};
    char ipAddress[20] = {};
    char mqttHost[32] = {};
    char shortTime[6] = "00:00";          // HH:MM
    char fullDateTime[17] = "---- -- -- --:--";
    bool timeFromNtp = false;
    bool timeValid = false;
    size_t queueCount = 0;
    size_t queueCapacity = MAX_QUEUE_CAPACITY;
    uint32_t telemetrySentCount = 0;
    SensorSnapshot snapshot{};
    const BackendRuntimeConfig* runtimeConfig = nullptr;
    const RelayRuntimeConfig* relays = nullptr;  // Array of MAX_RELAY_COUNT
    const ErrorRegistry* errors = nullptr;
};

namespace LcdViewFormatter {
void format(const UiController& ui, const LcdViewContext& ctx, LcdLines& out);
}

#endif
