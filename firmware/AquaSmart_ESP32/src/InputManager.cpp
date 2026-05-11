#include "InputManager.h"

#include "PinMap.h"

InputManager::InputManager() : _encoder(PIN_ENC_CLK, PIN_ENC_DT, RotaryEncoder::LatchMode::TWO03) {
}

void InputManager::begin() {
    pinMode(PIN_ENC_CLK, INPUT_PULLUP);
    pinMode(PIN_ENC_DT, INPUT_PULLUP);
    pinMode(PIN_ENC_SW, INPUT_PULLUP);
    pinMode(PIN_BTN_EXT, INPUT_PULLUP);

    _encoderButton.attach(PIN_ENC_SW);
    _encoderButton.interval(15);
    _encoderButton.setPressedState(LOW);

    _backButton.attach(PIN_BTN_EXT);
    _backButton.interval(15);
    _backButton.setPressedState(LOW);
}

InputEvent InputManager::update() {
    _encoder.tick();
    _encoderButton.update();
    _backButton.update();

    if (_encoderButton.released()) {
        _encoderLongPressHandled = false;
    }
    if (_backButton.released()) {
        _backLongPressHandled = false;
    }

    const long position = _encoder.getPosition();
    if (position > _lastEncoderPosition) {
        _lastEncoderPosition = position;
        return {InputEventType::NEXT};
    }
    if (position < _lastEncoderPosition) {
        _lastEncoderPosition = position;
        return {InputEventType::PREVIOUS};
    }

    if (_encoderButton.pressed()) {
        return {InputEventType::SELECT};
    }

    if (!_encoderLongPressHandled && _encoderButton.currentDuration() >= 1200 && _encoderButton.isPressed()) {
        _encoderLongPressHandled = true;
        return {InputEventType::SELECT, true};
    }

    if (_backButton.pressed()) {
        return {InputEventType::BACK};
    }

    if (!_backLongPressHandled && _backButton.currentDuration() >= 1500 && _backButton.isPressed()) {
        _backLongPressHandled = true;
        return {InputEventType::BACK, true};
    }

    return {};
}
