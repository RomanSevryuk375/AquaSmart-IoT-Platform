#ifndef INPUT_MANAGER_H
#define INPUT_MANAGER_H

#include <Bounce2.h>
#include <RotaryEncoder.h>
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
    long _lastEncoderPosition = 0;
    bool _encoderLongPressHandled = false;
    bool _backLongPressHandled = false;
};

#endif
