#include "LedIndicator.h"

#include "PinMap.h"

void LedIndicator::begin() {
    pinMode(PIN_LED_ONLINE, OUTPUT);
    pinMode(PIN_LED_WIFI, OUTPUT);
    pinMode(PIN_LED_DEGRADED, OUTPUT);
    pinMode(PIN_LED_ERROR, OUTPUT);
    pinMode(PIN_LED_ACTIVITY, OUTPUT);
}

void LedIndicator::update(SystemState state, bool wifiConnected, bool apiHealthy, bool pulseActivity) {
    digitalWrite(PIN_LED_WIFI, wifiConnected ? HIGH : LOW);
    digitalWrite(PIN_LED_ONLINE, apiHealthy && state == SystemState::ONLINE ? HIGH : LOW);
    digitalWrite(PIN_LED_DEGRADED, state == SystemState::DEGRADED_OFFLINE ? HIGH : LOW);
    digitalWrite(PIN_LED_ERROR, state == SystemState::SAFE_MODE ? HIGH : LOW);

    if (pulseActivity) {
        _lastPulseAtMs = millis();
    }
    digitalWrite(PIN_LED_ACTIVITY, millis() - _lastPulseAtMs < 150 ? HIGH : LOW);
}
