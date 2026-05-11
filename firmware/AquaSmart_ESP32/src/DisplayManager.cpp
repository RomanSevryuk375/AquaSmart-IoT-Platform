#include "DisplayManager.h"

#include "PinMap.h"
#include "TelemetryQueue.h"

void DisplayManager::begin() {
    _lcd.init();
    _lcd.backlight();
    _lcd.clear();
}

void DisplayManager::render(DisplayPage page,
                            SystemState state,
                            ErrorCode errorCode,
                            const SensorSnapshot& snapshot,
                            const TelemetryQueue* queue,
                            const BackendRuntimeConfig& runtimeConfig,
                            size_t selectedSensorIndex,
                            size_t selectedRelayIndex,
                            bool wifiConnected,
                            bool apiHealthy,
                            const String& timeLabel) {
    switch (page) {
        case DisplayPage::STATUS:
            printLine(0, "Aqua " + stateText(state));
            if (snapshot.hasWaterTemperature) {
                printLine(1, "Tw " + String(snapshot.waterTemperature, 1) +
                                 "C L:" + String(snapshot.waterLevelSafe ? "OK" : "LOW"));
            } else {
                printLine(1, "Lvl:" + String(snapshot.waterLevelSafe ? "OK" : "LOW") +
                                 " Q:" + String(queue ? queue->count() : 0));
            }
            break;
        case DisplayPage::NETWORK:
            printLine(0, wifiConnected ? "WiFi CONNECTED" : "WiFi OFFLINE");
            printLine(1, apiHealthy ? "API OK" : "API DEGRADED");
            break;
        case DisplayPage::QUEUE:
            printLine(0, "Queue " + String(queue ? queue->count() : 0) + "/" + String(queue ? queue->capacity() : 0));
            printLine(1, timeLabel);
            break;
        case DisplayPage::SENSOR:
            if (runtimeConfig.sensorCount == 0) {
                printLine(0, "No sensors");
                printLine(1, "");
                break;
            }
            if (selectedSensorIndex >= runtimeConfig.sensorCount) {
                selectedSensorIndex = 0;
            }
            printLine(0, runtimeConfig.sensors[selectedSensorIndex].name);
            if (runtimeConfig.sensors[selectedSensorIndex].hasValue) {
                if (runtimeConfig.sensors[selectedSensorIndex].role == SensorRole::SOIL_MOISTURE) {
                    printLine(1, String(runtimeConfig.sensors[selectedSensorIndex].lastValue, 0) + "% raw " +
                                     String(runtimeConfig.sensors[selectedSensorIndex].lastRawValue, 0));
                } else {
                    printLine(1, String(runtimeConfig.sensors[selectedSensorIndex].lastValue, 1) + " " +
                                     runtimeConfig.sensors[selectedSensorIndex].unit);
                }
            } else {
                printLine(1, runtimeConfig.sensors[selectedSensorIndex].hasError ? "Sensor error" : "No data");
            }
            break;
        case DisplayPage::RELAY:
            if (runtimeConfig.relayCount == 0) {
                printLine(0, "No relays");
                printLine(1, "");
                break;
            }
            if (selectedRelayIndex >= runtimeConfig.relayCount) {
                selectedRelayIndex = 0;
            }
            printLine(0, runtimeConfig.relays[selectedRelayIndex].name);
            printLine(1, String("State ") + (runtimeConfig.relays[selectedRelayIndex].currentState ? "ON" : "OFF"));
            break;
        case DisplayPage::ERROR:
            printLine(0, "Error");
            printLine(1, errorText(errorCode));
            break;
    }
}

void DisplayManager::printLine(uint8_t row, const String& text) {
    String padded = text;
    if (padded.length() > 16) {
        padded = padded.substring(0, 16);
    }
    while (padded.length() < 16) {
        padded += ' ';
    }

    _lcd.setCursor(0, row);
    _lcd.print(padded);
}

String DisplayManager::stateText(SystemState state) const {
    switch (state) {
        case SystemState::ONLINE:
            return "ONLINE";
        case SystemState::SYNC_TIME:
            return "TIME";
        case SystemState::CONNECT_WIFI:
            return "WIFI";
        case SystemState::DEGRADED_OFFLINE:
            return "OFFLINE";
        case SystemState::SAFE_MODE:
            return "SAFE";
        case SystemState::FACTORY_RESET:
            return "RESET";
        default:
            return "BOOT";
    }
}

String DisplayManager::errorText(ErrorCode errorCode) const {
    switch (errorCode) {
        case ErrorCode::E001_CONFIG_INVALID: return "E001 CONFIG";
        case ErrorCode::E002_WIFI_FAILED: return "E002 WIFI";
        case ErrorCode::E003_RTC_UNAVAILABLE: return "E003 RTC";
        case ErrorCode::E004_TIME_INVALID: return "E004 TIME";
        case ErrorCode::E005_BACKEND_UNAVAILABLE: return "E005 API";
        case ErrorCode::E006_BACKEND_VALIDATION: return "E006 VALID";
        case ErrorCode::E007_SENSOR_READ_FAILED: return "E007 SENSOR";
        case ErrorCode::E008_RELAY_INIT_FAILED: return "E008 RELAY";
        case ErrorCode::E009_QUEUE_OVERFLOW: return "E009 QUEUE";
        case ErrorCode::E010_DEVICE_TOKEN_MISSING: return "E010 TOKEN";
        default: return "NO ERROR";
    }
}
