#ifndef PIN_MAP_H
#define PIN_MAP_H

#include <Arduino.h>
#include "AppState.h"

constexpr uint8_t PIN_I2C_SDA = 21;
constexpr uint8_t PIN_I2C_SCL = 22;
constexpr uint8_t PIN_ONEWIRE_BUS = 4;
constexpr uint8_t PIN_WATER_LEVEL = 34;

constexpr uint8_t RELAY_PINS[MAX_RELAY_COUNT] = {16, 17, 18, 19, 25, 26, 27, 32};

constexpr uint8_t PIN_ENC_CLK = 33;
constexpr uint8_t PIN_ENC_DT = 35;
constexpr uint8_t PIN_ENC_SW = 36;
constexpr uint8_t PIN_BTN_EXT = 39;

constexpr uint8_t PIN_LED_ONLINE = 13;
constexpr uint8_t PIN_LED_WIFI = 14;
constexpr uint8_t PIN_LED_DEGRADED = 23;
constexpr uint8_t PIN_LED_ERROR = 12;
constexpr uint8_t PIN_LED_ACTIVITY = 2;

constexpr uint8_t LCD_I2C_ADDRESS = 0x27;
constexpr uint8_t AHT20_I2C_ADDRESS = 0x38;
constexpr uint8_t RTC_I2C_ADDRESS = 0x68;

#endif
