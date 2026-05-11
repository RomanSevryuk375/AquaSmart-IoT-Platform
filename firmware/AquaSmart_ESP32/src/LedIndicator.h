#ifndef LED_INDICATOR_H
#define LED_INDICATOR_H

#include "AppState.h"

class LedIndicator {
public:
    void begin();
    void update(SystemState state, bool wifiConnected, bool apiHealthy, bool pulseActivity);

private:
    uint32_t _lastPulseAtMs = 0;
};

#endif
