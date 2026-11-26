#include "UiController.h"

#include <cstdio>
#include <cstring>

void UiController::begin() {
    _level = UiLevel::DASHBOARD;
    _rootIndex = 0;
    _sensorIndex = 0;
    _relayIndex = 0;
    _networkIndex = 0;
    _queueTimeIndex = 0;
    _errorIndex = 0;
    _systemIndex = 0;
}

bool UiController::isOnFactoryResetScreen() const {
    return _level == UiLevel::SUBMENU &&
           selectedSection() == MenuSection::SYSTEM &&
           _systemIndex == static_cast<size_t>(SystemSubScreen::FACTORY_RESET);
}

UiAction UiController::handleInput(
    const InputEvent& event,
    size_t sensorCount,
    size_t activeErrorCount) {
    UiAction action{};

    if (event.type == InputEventType::NONE) {
        return action;
    }

    // Factory Reset ONLY executes when BACK is held >= 5s on the dedicated Factory Reset screen.
    if (event.type == InputEventType::FACTORY_RESET) {
        if (isOnFactoryResetScreen()) {
            action.type = UiActionType::FACTORY_RESET;
        }
        return action;
    }

    // Pure BACK: always exits one level up.
    if (event.type == InputEventType::BACK) {
        if (_level == UiLevel::SUBMENU) {
            _level = UiLevel::ROOT_MENU;
        } else if (_level == UiLevel::ROOT_MENU) {
            _level = UiLevel::DASHBOARD;
        }
        return action;
    }

    if (_level == UiLevel::DASHBOARD) {
        if (event.type == InputEventType::SELECT && !event.longPress) {
            _level = UiLevel::ROOT_MENU;
        }
        return action;
    }

    if (_level == UiLevel::ROOT_MENU) {
        if (event.type == InputEventType::NEXT) {
            _rootIndex = (_rootIndex + 1) % ROOT_MENU_ITEM_COUNT;
        } else if (event.type == InputEventType::PREVIOUS) {
            _rootIndex = (_rootIndex == 0) ? (ROOT_MENU_ITEM_COUNT - 1) : (_rootIndex - 1);
        } else if (event.type == InputEventType::SELECT && !event.longPress) {
            _level = UiLevel::SUBMENU;
            switch (selectedSection()) {
                case MenuSection::SENSORS: _sensorIndex = 0; break;
                case MenuSection::RELAYS: _relayIndex = 0; break;
                case MenuSection::NETWORK: _networkIndex = 0; break;
                case MenuSection::QUEUE_TIME: _queueTimeIndex = 0; break;
                case MenuSection::ERRORS: _errorIndex = 0; break;
                case MenuSection::SYSTEM: _systemIndex = 0; break;
            }
        }
        return action;
    }

    // Level 2: SUBMENU
    switch (selectedSection()) {
        case MenuSection::SENSORS:
            if (sensorCount > 0) {
                if (_sensorIndex >= sensorCount) {
                    _sensorIndex = 0;
                }
                if (event.type == InputEventType::NEXT) {
                    _sensorIndex = (_sensorIndex + 1) % sensorCount;
                } else if (event.type == InputEventType::PREVIOUS) {
                    _sensorIndex = (_sensorIndex == 0) ? (sensorCount - 1) : (_sensorIndex - 1);
                }
            }
            break;

        case MenuSection::RELAYS:
            if (event.type == InputEventType::NEXT) {
                _relayIndex = (_relayIndex + 1) % MAX_RELAY_COUNT;
            } else if (event.type == InputEventType::PREVIOUS) {
                _relayIndex = (_relayIndex == 0) ? (MAX_RELAY_COUNT - 1) : (_relayIndex - 1);
            } else if (event.type == InputEventType::SELECT) {
                action.relayChannel = static_cast<uint8_t>(_relayIndex + 1);
                action.type = event.longPress
                    ? UiActionType::TOGGLE_RELAY_MODE
                    : UiActionType::TOGGLE_RELAY_STATE;
            }
            break;

        case MenuSection::NETWORK:
            if (event.type == InputEventType::NEXT) {
                _networkIndex = (_networkIndex + 1) % NETWORK_SUBSCREEN_COUNT;
            } else if (event.type == InputEventType::PREVIOUS) {
                _networkIndex = (_networkIndex == 0) ? (NETWORK_SUBSCREEN_COUNT - 1) : (_networkIndex - 1);
            }
            break;

        case MenuSection::QUEUE_TIME:
            if (event.type == InputEventType::NEXT) {
                _queueTimeIndex = (_queueTimeIndex + 1) % QUEUE_TIME_SUBSCREEN_COUNT;
            } else if (event.type == InputEventType::PREVIOUS) {
                _queueTimeIndex = (_queueTimeIndex == 0) ? (QUEUE_TIME_SUBSCREEN_COUNT - 1) : (_queueTimeIndex - 1);
            } else if (event.type == InputEventType::SELECT && !event.longPress) {
                if (_queueTimeIndex == static_cast<size_t>(QueueTimeSubScreen::SYNC_CONFIG)) {
                    action.type = UiActionType::SYNC_CONFIG;
                } else if (_queueTimeIndex == static_cast<size_t>(QueueTimeSubScreen::FLUSH_QUEUE)) {
                    action.type = UiActionType::FLUSH_QUEUE;
                }
            }
            break;

        case MenuSection::ERRORS:
            if (activeErrorCount > 0) {
                if (_errorIndex >= activeErrorCount) {
                    _errorIndex = 0;
                }
                if (event.type == InputEventType::NEXT) {
                    _errorIndex = (_errorIndex + 1) % activeErrorCount;
                } else if (event.type == InputEventType::PREVIOUS) {
                    _errorIndex = (_errorIndex == 0) ? (activeErrorCount - 1) : (_errorIndex - 1);
                } else if (event.type == InputEventType::SELECT && !event.longPress) {
                    action.type = UiActionType::CLEAR_NON_CRITICAL_ERRORS;
                }
            }
            break;

        case MenuSection::SYSTEM:
            if (event.type == InputEventType::NEXT) {
                _systemIndex = (_systemIndex + 1) % SYSTEM_SUBSCREEN_COUNT;
            } else if (event.type == InputEventType::PREVIOUS) {
                _systemIndex = (_systemIndex == 0) ? (SYSTEM_SUBSCREEN_COUNT - 1) : (_systemIndex - 1);
            } else if (event.type == InputEventType::SELECT && !event.longPress) {
                switch (static_cast<SystemSubScreen>(_systemIndex)) {
                    case SystemSubScreen::OTA_CHECK:
                        action.type = UiActionType::OTA_CHECK;
                        break;
                    case SystemSubScreen::SAFE_STATE_ALL:
                        action.type = UiActionType::SAFE_STATE_ALL;
                        break;
                    case SystemSubScreen::REBOOT:
                        action.type = UiActionType::REBOOT;
                        break;
                    case SystemSubScreen::FACTORY_RESET:
                        // Short SELECT does NOT trigger Factory Reset; requires Hold BACK >= 5s.
                        break;
                }
            }
            break;
    }

    return action;
}

namespace LcdViewFormatter {

namespace {
void padLine(char (&dest)[17], const char* text) {
    size_t i = 0;
    if (text != nullptr) {
        for (; i < 16 && text[i] != '\0'; ++i) {
            dest[i] = text[i];
        }
    }
    for (; i < 16; ++i) {
        dest[i] = ' ';
    }
    dest[16] = '\0';
}

const char* rootMenuTitle(size_t idx) {
    switch (idx) {
        case 0: return "1. Sensors";
        case 1: return "2. Relays";
        case 2: return "3. Network";
        case 3: return "4. Queue & Time";
        case 4: return "5. Errors";
        case 5: return "6. System";
        default: return "Menu";
    }
}
}  // namespace

void format(const UiController& ui, const LcdViewContext& ctx, LcdLines& out) {
    char buf0[32] = {};
    char buf1[32] = {};

    if (ui.level() == UiLevel::DASHBOARD) {
        const char* connLabel = "OFFLINE";
        if (ctx.systemState == SystemState::OTA_UPDATING) {
            connLabel = "OTA UPD";
        } else if (ctx.systemState == SystemState::SAFE_MODE) {
            connLabel = "SAFE   ";
        } else if (ctx.mqttConnected) {
            connLabel = "MQTT OK";
        } else if (ctx.wifiConnected && ctx.httpHealthy) {
            connLabel = "HTTP FB";
        } else if (ctx.wifiConnected) {
            connLabel = "WIFI OK";
        }

        std::snprintf(buf0, sizeof(buf0), "%-10s %5s", connLabel, ctx.shortTime);

        if (ctx.errors != nullptr && ctx.errors->count() > 0) {
            const ErrorEntry* err = ctx.errors->get(ctx.errors->count() - 1);
            std::snprintf(buf1, sizeof(buf1), "! ERR: %s", ErrorRegistry::codeLabel(err->code));
        } else if (ctx.runtimeConfig == nullptr || !ctx.runtimeConfig->isLoaded || ctx.runtimeConfig->sensorCount == 0) {
            std::snprintf(buf1, sizeof(buf1), "No sensors cfg");
        } else if (ctx.snapshot.hasWaterTemperature && ctx.snapshot.hasHumidity) {
            std::snprintf(buf1, sizeof(buf1), "Tw:%.1fC RH:%.0f%%", ctx.snapshot.waterTemperature, ctx.snapshot.humidity);
        } else if (ctx.snapshot.hasWaterTemperature) {
            std::snprintf(buf1, sizeof(buf1), "Tw:%.1fC L:%s", ctx.snapshot.waterTemperature, ctx.snapshot.waterLevelSafe ? "OK" : "LO");
        } else {
            std::snprintf(buf1, sizeof(buf1), "Sensors:%u Q:%u",
                          static_cast<unsigned>(ctx.runtimeConfig->sensorCount),
                          static_cast<unsigned>(ctx.queueCount));
        }

        padLine(out.line0, buf0);
        padLine(out.line1, buf1);
        return;
    }

    if (ui.level() == UiLevel::ROOT_MENU) {
        std::snprintf(buf0, sizeof(buf0), "> %s", rootMenuTitle(ui.rootIndex()));
        std::snprintf(buf1, sizeof(buf1), "  %s", rootMenuTitle((ui.rootIndex() + 1) % ROOT_MENU_ITEM_COUNT));
        padLine(out.line0, buf0);
        padLine(out.line1, buf1);
        return;
    }

    // Level 2: SUBMENU
    switch (ui.selectedSection()) {
        case MenuSection::SENSORS: {
            if (ctx.runtimeConfig == nullptr || !ctx.runtimeConfig->isLoaded || ctx.runtimeConfig->sensorCount == 0) {
                padLine(out.line0, "No sensors cfg");
                padLine(out.line1, "Run Sync Config");
                return;
            }
            size_t idx = ui.selectedSensorIndex();
            if (idx >= ctx.runtimeConfig->sensorCount) {
                idx = 0;
            }
            const SensorRuntimeConfig& s = ctx.runtimeConfig->sensors[idx];
            std::snprintf(buf0, sizeof(buf0), "[%u/%u] %s",
                          static_cast<unsigned>(idx + 1),
                          static_cast<unsigned>(ctx.runtimeConfig->sensorCount),
                          s.name[0] != '\0' ? s.name : "Sensor");
            if (s.hasError) {
                std::snprintf(buf1, sizeof(buf1), "ERR E007");
            } else if (s.hasValue) {
                std::snprintf(buf1, sizeof(buf1), "%.1f %s OK", s.lastValue, s.unit);
            } else {
                std::snprintf(buf1, sizeof(buf1), "Waiting data...");
            }
            break;
        }

        case MenuSection::RELAYS: {
            const size_t idx = ui.selectedRelayIndex() % MAX_RELAY_COUNT;
            const RelayRuntimeConfig* r = (ctx.relays != nullptr) ? &ctx.relays[idx] : nullptr;
            if (r != nullptr && r->configured && r->name[0] != '\0') {
                std::snprintf(buf0, sizeof(buf0), "[%u/8] CH%u %s",
                              static_cast<unsigned>(idx + 1),
                              static_cast<unsigned>(idx + 1),
                              r->name);
            } else {
                std::snprintf(buf0, sizeof(buf0), "[%u/8] CH%u",
                              static_cast<unsigned>(idx + 1),
                              static_cast<unsigned>(idx + 1));
            }
            const bool isOn = (r != nullptr) && r->currentState;
            const bool isManual = (r == nullptr) || r->isManual;
            std::snprintf(buf1, sizeof(buf1), "State:%-3s [%s]",
                          isOn ? "ON" : "OFF",
                          isManual ? "MAN" : "AUTO");
            break;
        }

        case MenuSection::NETWORK: {
            switch (static_cast<NetworkSubScreen>(ui.networkSubIndex())) {
                case NetworkSubScreen::WIFI:
                    std::snprintf(buf0, sizeof(buf0), "WiFi:%s %d",
                                  ctx.wifiConnected ? "OK" : "OFF",
                                  ctx.wifiRssi);
                    std::snprintf(buf1, sizeof(buf1), "%s", ctx.ipAddress[0] != '\0' ? ctx.ipAddress : "No IP");
                    break;
                case NetworkSubScreen::MQTT:
                    std::snprintf(buf0, sizeof(buf0), "MQTT:%s R:%lu",
                                  ctx.mqttConnected ? "CONN" : "DISC",
                                  static_cast<unsigned long>(ctx.mqttReconnectCount));
                    std::snprintf(buf1, sizeof(buf1), "%s", ctx.mqttHost[0] != '\0' ? ctx.mqttHost : "No broker");
                    break;
                case NetworkSubScreen::HTTP:
                    std::snprintf(buf0, sizeof(buf0), "HTTP: %s", ctx.httpHealthy ? "OK" : "ERR");
                    std::snprintf(buf1, sizeof(buf1), "Last code: %d", ctx.lastHttpStatus);
                    break;
            }
            break;
        }

        case MenuSection::QUEUE_TIME: {
            switch (static_cast<QueueTimeSubScreen>(ui.queueTimeSubIndex())) {
                case QueueTimeSubScreen::TIME:
                    std::snprintf(buf0, sizeof(buf0), "%s", ctx.fullDateTime);
                    std::snprintf(buf1, sizeof(buf1), "SRC: %s",
                                  !ctx.timeValid ? "INVALID" : (ctx.timeFromNtp ? "NTP" : "DS1307"));
                    break;
                case QueueTimeSubScreen::QUEUE:
                    std::snprintf(buf0, sizeof(buf0), "Queue: %03u/%03u",
                                  static_cast<unsigned>(ctx.queueCount),
                                  static_cast<unsigned>(ctx.queueCapacity));
                    std::snprintf(buf1, sizeof(buf1), "Sent: %lu", static_cast<unsigned long>(ctx.telemetrySentCount));
                    break;
                case QueueTimeSubScreen::SYNC_CONFIG:
                    std::snprintf(buf0, sizeof(buf0), "Sync Config");
                    std::snprintf(buf1, sizeof(buf1), "Press SELECT");
                    break;
                case QueueTimeSubScreen::FLUSH_QUEUE:
                    std::snprintf(buf0, sizeof(buf0), "Flush Queue");
                    std::snprintf(buf1, sizeof(buf1), "Press SELECT");
                    break;
            }
            break;
        }

        case MenuSection::ERRORS: {
            if (ctx.errors == nullptr || ctx.errors->count() == 0) {
                padLine(out.line0, "No active errors");
                padLine(out.line1, "System OK");
                return;
            }
            size_t idx = ui.errorSubIndex();
            if (idx >= ctx.errors->count()) {
                idx = 0;
            }
            const ErrorEntry* e = ctx.errors->get(idx);
            std::snprintf(buf0, sizeof(buf0), "[%u/%u] %s",
                          static_cast<unsigned>(idx + 1),
                          static_cast<unsigned>(ctx.errors->count()),
                          ErrorRegistry::codeLabel(e->code));
            std::snprintf(buf1, sizeof(buf1), "%s",
                          e->detail[0] != '\0' ? e->detail : ErrorRegistry::defaultMessage(e->code));
            break;
        }

        case MenuSection::SYSTEM: {
            switch (static_cast<SystemSubScreen>(ui.systemSubIndex())) {
                case SystemSubScreen::OTA_CHECK:
                    std::snprintf(buf0, sizeof(buf0), "System: OTA");
                    std::snprintf(buf1, sizeof(buf1), "SEL: OTA Check");
                    break;
                case SystemSubScreen::SAFE_STATE_ALL:
                    std::snprintf(buf0, sizeof(buf0), "Safe State All");
                    std::snprintf(buf1, sizeof(buf1), "SEL: Apply all");
                    break;
                case SystemSubScreen::REBOOT:
                    std::snprintf(buf0, sizeof(buf0), "System: Reboot");
                    std::snprintf(buf1, sizeof(buf1), "Press SELECT");
                    break;
                case SystemSubScreen::FACTORY_RESET:
                    std::snprintf(buf0, sizeof(buf0), "Factory Reset");
                    std::snprintf(buf1, sizeof(buf1), "Hold BACK 5s");
                    break;
            }
            break;
        }
    }

    padLine(out.line0, buf0);
    padLine(out.line1, buf1);
}

}  // namespace LcdViewFormatter
