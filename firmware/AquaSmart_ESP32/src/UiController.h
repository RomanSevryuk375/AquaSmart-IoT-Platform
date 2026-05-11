#ifndef UI_CONTROLLER_H
#define UI_CONTROLLER_H

#include "AppState.h"

class UiController {
public:
    void begin();
    void handleInput(const InputEvent& event, const BackendRuntimeConfig& runtimeConfig, bool& toggleRelayRequested);
    DisplayPage currentPage() const;
    size_t selectedSensorIndex() const;
    size_t selectedRelayIndex() const;

private:
    DisplayPage _currentPage = DisplayPage::STATUS;
    size_t _selectedSensorIndex = 0;
    size_t _selectedRelayIndex = 0;

    DisplayPage nextPage(DisplayPage current) const;
    DisplayPage previousPage(DisplayPage current) const;
};

#endif
