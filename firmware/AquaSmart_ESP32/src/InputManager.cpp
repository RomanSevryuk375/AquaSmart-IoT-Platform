#include "InputManager.h"

#include "PinMap.h"

InputManager::InputManager()
    : _encoder(PIN_ENC_CLK, PIN_ENC_DT, RotaryEncoder::LatchMode::TWO03) {
}

void InputManager::begin() {
    // GPIO 33 supports internal pull-up.
    pinMode(PIN_ENC_CLK, INPUT_PULLUP);
    // GPIO 35, 36, 39 are input-only on ESP32 and have NO internal pull-up/pull-down;
    // external 10k pull-up resistors to 3.3V are used.
    pinMode(PIN_ENC_DT, INPUT);
    pinMode(PIN_ENC_SW, INPUT);
    pinMode(PIN_BTN_EXT, INPUT);

    _encoderButton.attach(PIN_ENC_SW);
    _encoderButton.interval(15);
    _encoderButton.setPressedState(LOW);

    _backButton.attach(PIN_BTN_EXT);
    _backButton.interval(15);
    _backButton.setPressedState(LOW);

    _selectDetector.reset();
    _backDetector.reset();
}

InputEvent InputManager::update() {
    _encoder.tick();
    _encoderButton.update();
    _backButton.update();

    const long position = _encoder.getPosition();
    if (position > _lastEncoderPosition) {
        _lastEncoderPosition = position;
        return {InputEventType::NEXT, false};
    }
    if (position < _lastEncoderPosition) {
        _lastEncoderPosition = position;
        return {InputEventType::PREVIOUS, false};
    }

    const uint32_t nowMs = millis();

    const uint8_t selectState = _selectDetector.update(_encoderButton.isPressed(), nowMs);
    if (selectState == 2) {
        return {InputEventType::SELECT, true};
    }
    if (selectState == 1) {
        return {InputEventType::SELECT, false};
    }

    const uint8_t backState = _backDetector.update(_backButton.isPressed(), nowMs);
    if (backState == 2) {
        return {InputEventType::FACTORY_RESET, true};
    }
    if (backState == 1) {
        return {InputEventType::BACK, false};
    }

    return {};
}
