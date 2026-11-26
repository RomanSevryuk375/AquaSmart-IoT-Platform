#include "DisplayManager.h"

#include <cstring>

void DisplayManager::begin() {
    _lcd.init();
    _lcd.backlight();
    _lcd.clear();
    _lastLine0[0] = '\0';
    _lastLine1[0] = '\0';
}

void DisplayManager::render(const UiController& ui, const LcdViewContext& ctx) {
    LcdLines lines{};
    LcdViewFormatter::format(ui, ctx, lines);
    writeRowIfChanged(0, lines.line0, _lastLine0);
    writeRowIfChanged(1, lines.line1, _lastLine1);
}

void DisplayManager::writeRowIfChanged(uint8_t row, const char* text, char (&cache)[17]) {
    if (std::strncmp(cache, text, 16) == 0 && cache[16] == '\0' && cache[0] != '\0') {
        return;
    }
    copyCString(cache, sizeof(cache), text);
    _lcd.setCursor(0, row);
    _lcd.print(cache);
}
