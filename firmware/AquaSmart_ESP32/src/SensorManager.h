#ifndef SENSOR_MANAGER_H
#define SENSOR_MANAGER_H

#include <Adafruit_AHTX0.h>
#include <DallasTemperature.h>
#include <OneWire.h>
#include "AppState.h"

class SensorManager {
public:
    SensorManager();

    bool begin(const DeviceConfig& deviceConfig);
    void applyRuntimeConfig(BackendRuntimeConfig& runtimeConfig, const DeviceConfig& deviceConfig);
    bool update(ErrorCode& errorCode);
    const SensorSnapshot& getSnapshot() const;

private:
    BackendRuntimeConfig* _runtimeConfig = nullptr;
    const DeviceConfig* _deviceConfig = nullptr;
    Adafruit_AHTX0 _aht;
    OneWire _oneWire;
    DallasTemperature _ds18b20;
    SensorSnapshot _snapshot;
    bool _ahtAvailable = false;
    bool _dsAvailable = false;
    bool _waterLevelAvailable = false;
    bool _soilAvailable = false;
    bool _waterLevelStable = true;
    bool _waterLevelLastRaw = true;
    uint32_t _lastWaterLevelChangeMs = 0;
    uint32_t _lastDsRequestAtMs = 0;
    bool _dsConversionRequested = false;

    bool handleSensor(SensorRuntimeConfig& sensor, uint32_t nowMs);
    bool readAht20(SensorRuntimeConfig& sensor);
    bool readDs18b20(SensorRuntimeConfig& sensor, uint32_t nowMs);
    bool readWaterLevel(SensorRuntimeConfig& sensor, uint32_t nowMs);
    bool readSoilMoisture(SensorRuntimeConfig& sensor);
    SensorRole inferRole(const SensorRuntimeConfig& sensor) const;
    String toLower(const String& value) const;
    uint8_t parseI2cAddress(const String& address) const;
    bool readDsAddressByRole(SensorRole role, DeviceAddress address) const;
    bool tryReadSoilRaw(uint16_t& rawValue);
    float calibrateSoilMoisture(uint16_t rawValue) const;
    String normalizeAddress(const String& value) const;
};

#endif
