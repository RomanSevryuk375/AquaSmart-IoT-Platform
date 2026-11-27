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

void DisplayManager::showBanner(const char* line0, const char* line1) {
    char padded0[17];
    char padded1[17];
    snprintf(padded0, sizeof(padded0), "%-16.16s", line0 != nullptr ? line0 : "");
    snprintf(padded1, sizeof(padded1), "%-16.16s", line1 != nullptr ? line1 : "");
    writeRowIfChanged(0, padded0, _lastLine0);
    writeRowIfChanged(1, padded1, _lastLine1);
}

void DisplayManager::writeRowIfChanged(uint8_t row, const char* text, char (&cache)[17]) {
    if (std::strncmp(cache, text, 16) == 0 && cache[16] == '\0' && cache[0] != '\0') {
        return;
    }
    copyCString(cache, sizeof(cache), text);
    _lcd.setCursor(0, row);
    _lcd.print(cache);
}
