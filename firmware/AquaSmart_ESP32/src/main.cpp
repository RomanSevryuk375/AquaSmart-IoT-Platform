#include <Arduino.h>
#include <Wire.h>
#include <LiquidCrystal_I2C.h>
#include "PinMap.h"

LiquidCrystal_I2C lcd(0x27, 16, 2);

void scanI2C() {
    Serial.println("\n[I2C Scanner] Start scanning...");
    byte error, address;
    int nDevices = 0;

    for (address = 1; address < 127; address++) {
        Wire.beginTransmission(address);
        error = Wire.endTransmission();

        if (error == 0) {
            Serial.printf("Found device at 0x%02X\n", address);
            nDevices++;
        }
    }
    if (nDevices == 0) Serial.println("No I2C devices found\n");
    else Serial.println("[I2C Scanner] Done.\n");
}

void setup() {
    for (uint8_t pin : RELAY_PINS) {
        pinMode(pin, OUTPUT);
        digitalWrite(pin, HIGH); 
    }

    pinMode(PIN_LED_ACTIVITY, OUTPUT);
    digitalWrite(PIN_LED_ACTIVITY, HIGH); 

    Serial.begin(115200);
    delay(1000);
    Serial.println("AquaSmart ESP32 Booting...");

    Wire.begin(PIN_I2C_SDA, PIN_I2C_SCL);

    lcd.init();
    lcd.backlight();
    lcd.setCursor(0, 0);
    lcd.print("AquaSmart Boot");
    lcd.setCursor(0, 1);
    lcd.print("Scanning I2C...");

    scanI2C();
}

void loop() {
    static uint32_t lastTick = 0;
    if (millis() - lastTick > 1000) {
        digitalWrite(PIN_LED_ACTIVITY, !digitalRead(PIN_LED_ACTIVITY));
        lastTick = millis();
        Serial.println("Heartbeat...");
     scanI2C();
    }
}