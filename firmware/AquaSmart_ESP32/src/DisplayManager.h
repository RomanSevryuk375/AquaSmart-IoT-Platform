#ifndef DISPLAY_MANAGER_H
#define DISPLAY_MANAGER_H

#include <LiquidCrystal_I2C.h>
#include "AppState.h"
#include "PinMap.h"

class TelemetryQueue;

class DisplayManager {
public:
    void begin();
    void render(DisplayPage page,
                SystemState state,
                ErrorCode errorCode,
                const SensorSnapshot& snapshot,
                const TelemetryQueue* queue,
                const BackendRuntimeConfig& runtimeConfig,
                size_t selectedSensorIndex,
                size_t selectedRelayIndex,
                bool wifiConnected,
                bool apiHealthy,
                const String& timeLabel);

private:
    LiquidCrystal_I2C _lcd = LiquidCrystal_I2C(LCD_I2C_ADDRESS, 16, 2);

    void printLine(uint8_t row, const String& text);
    String stateText(SystemState state) const;
    String errorText(ErrorCode errorCode) const;
};

#endif
