#ifndef DISPLAY_MANAGER_H
#define DISPLAY_MANAGER_H

#include <LiquidCrystal_I2C.h>
#include <UiController.h>
#include "AppState.h"
#include "PinMap.h"

class DisplayManager {
public:
    void begin();
    void render(const UiController& ui, const LcdViewContext& ctx);

private:
    LiquidCrystal_I2C _lcd = LiquidCrystal_I2C(LCD_I2C_ADDRESS, 16, 2);
    char _lastLine0[17] = {};
    char _lastLine1[17] = {};

    void writeRowIfChanged(uint8_t row, const char* text, char (&cache)[17]);
};

#endif
