#include "UiController.h"

void UiController::begin() {
    _currentPage = DisplayPage::STATUS;
}

void UiController::handleInput(const InputEvent& event, const BackendRuntimeConfig& runtimeConfig, bool& toggleRelayRequested) {
    toggleRelayRequested = false;

    switch (event.type) {
        case InputEventType::NEXT:
            if (_currentPage == DisplayPage::SENSOR && runtimeConfig.sensorCount > 0) {
                _selectedSensorIndex = (_selectedSensorIndex + 1) % runtimeConfig.sensorCount;
            } else if (_currentPage == DisplayPage::RELAY && runtimeConfig.relayCount > 0) {
                _selectedRelayIndex = (_selectedRelayIndex + 1) % runtimeConfig.relayCount;
            } else {
                _currentPage = nextPage(_currentPage);
            }
            break;
        case InputEventType::PREVIOUS:
            if (_currentPage == DisplayPage::SENSOR && runtimeConfig.sensorCount > 0) {
                _selectedSensorIndex = (_selectedSensorIndex == 0) ? runtimeConfig.sensorCount - 1 : _selectedSensorIndex - 1;
            } else if (_currentPage == DisplayPage::RELAY && runtimeConfig.relayCount > 0) {
                _selectedRelayIndex = (_selectedRelayIndex == 0) ? runtimeConfig.relayCount - 1 : _selectedRelayIndex - 1;
            } else {
                _currentPage = previousPage(_currentPage);
            }
            break;
        case InputEventType::SELECT:
            if (event.longPress) {
                _currentPage = DisplayPage::ERROR;
            } else if (_currentPage == DisplayPage::RELAY && runtimeConfig.relayCount > 0) {
                toggleRelayRequested = true;
            }
            break;
        case InputEventType::BACK:
            _currentPage = event.longPress ? DisplayPage::NETWORK : DisplayPage::STATUS;
            break;
        default:
            break;
    }
}

DisplayPage UiController::currentPage() const {
    return _currentPage;
}

size_t UiController::selectedSensorIndex() const {
    return _selectedSensorIndex;
}

size_t UiController::selectedRelayIndex() const {
    return _selectedRelayIndex;
}

DisplayPage UiController::nextPage(DisplayPage current) const {
    switch (current) {
        case DisplayPage::STATUS: return DisplayPage::NETWORK;
        case DisplayPage::NETWORK: return DisplayPage::QUEUE;
        case DisplayPage::QUEUE: return DisplayPage::SENSOR;
        case DisplayPage::SENSOR: return DisplayPage::RELAY;
        case DisplayPage::RELAY: return DisplayPage::ERROR;
        case DisplayPage::ERROR: return DisplayPage::STATUS;
    }
    return DisplayPage::STATUS;
}

DisplayPage UiController::previousPage(DisplayPage current) const {
    switch (current) {
        case DisplayPage::STATUS: return DisplayPage::ERROR;
        case DisplayPage::NETWORK: return DisplayPage::STATUS;
        case DisplayPage::QUEUE: return DisplayPage::NETWORK;
        case DisplayPage::SENSOR: return DisplayPage::QUEUE;
        case DisplayPage::RELAY: return DisplayPage::SENSOR;
        case DisplayPage::ERROR: return DisplayPage::RELAY;
    }
    return DisplayPage::STATUS;
}
