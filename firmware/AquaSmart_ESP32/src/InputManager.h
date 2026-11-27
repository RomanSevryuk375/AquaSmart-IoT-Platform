#ifndef INPUT_MANAGER_H
#define INPUT_MANAGER_H

#include <Bounce2.h>
#include <RotaryEncoder.h>
#include <UiController.h>
#include "AppState.h"

class InputManager {
public:
    InputManager();

    void begin();
    InputEvent update();

private:
    RotaryEncoder _encoder;
    Bounce2::Button _encoderButton;
    Bounce2::Button _backButton;
    ButtonHoldDetector _selectDetector{SELECT_LONG_PRESS_MS};
    ButtonHoldDetector _backDetector{BACK_FACTORY_RESET_HOLD_MS};
    long _lastEncoderPosition = 0;
};

#endif
