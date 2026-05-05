#ifndef PIN_MAP_H
#define PIN_MAP_H

#pragma once

#include <Arduino.h>

// I2C Bus (LCD1602, DS3231, AHT20, Soil Moisture)
constexpr uint8_t PIN_I2C_SDA = 21;
constexpr uint8_t PIN_I2C_SCL = 22;

// OneWire Bus (DS18B20 - Water & Surface)

constexpr uint8_t PIN_ONEWIRE_BUS = 4;

// Digital Sensors

constexpr uint8_t PIN_WATER_LEVEL = 34; 

// Relays (8 channels) 

const uint8_t RELAY_PINS[8] = {16, 17, 18, 19, 25, 26, 27, 32};

// Input / UI

constexpr uint8_t PIN_ENC_CLK = 33;
constexpr uint8_t PIN_ENC_DT  = 35; 
constexpr uint8_t PIN_ENC_SW  = 36; 
constexpr uint8_t PIN_BTN_EXT = 39;

// LED Indicators 

constexpr uint8_t PIN_LED_ONLINE   = 13; // Green 
constexpr uint8_t PIN_LED_WIFI     = 14; // Blue  
constexpr uint8_t PIN_LED_DEGRADED = 23; // Yellow
constexpr uint8_t PIN_LED_ERROR    = 12; // Red 
constexpr uint8_t PIN_LED_ACTIVITY = 2;  // White 

#endif