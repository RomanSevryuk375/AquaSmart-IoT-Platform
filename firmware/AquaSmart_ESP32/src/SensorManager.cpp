#include "SensorManager.h"

#include <Wire.h>

#include "PinMap.h"

namespace {
constexpr uint32_t DS18_CONVERSION_WAIT_MS = 800;
constexpr uint8_t SOIL_READ_ATTEMPTS = 2;

bool isValidTemperature(float value) {
    return value > -40.0f && value < 125.0f && value != 85.0f && value != -127.0f;
}
}

SensorManager::SensorManager() : _oneWire(PIN_ONEWIRE_BUS), _ds18b20(&_oneWire) {
}

bool SensorManager::begin(const DeviceConfig& deviceConfig) {
    _deviceConfig = &deviceConfig;

    pinMode(PIN_WATER_LEVEL, INPUT);
    _waterLevelAvailable = true;

    _ahtAvailable = _aht.begin(&Wire, 0, AHT20_I2C_ADDRESS);

    _ds18b20.begin();
    _ds18b20.setWaitForConversion(false);
    _ds18b20.requestTemperatures();
    _lastDsRequestAtMs = millis();
    _dsConversionRequested = true;
    _dsAvailable = _ds18b20.getDeviceCount() > 0;

    uint16_t ignoredRaw = 0;
    _soilAvailable = tryReadSoilRaw(ignoredRaw);

    _snapshot.ahtAvailable = _ahtAvailable;
    _snapshot.ds18Available = _dsAvailable;
    _snapshot.soilAvailable = _soilAvailable;

    _lastWaterLevelChangeMs = millis();
    _waterLevelLastRaw = digitalRead(PIN_WATER_LEVEL) == HIGH;
    _waterLevelStable = _waterLevelLastRaw;
    _snapshot.waterLevelSafe = _waterLevelStable;

    return _waterLevelAvailable || _ahtAvailable || _dsAvailable || _soilAvailable;
}

void SensorManager::applyRuntimeConfig(BackendRuntimeConfig& runtimeConfig, const DeviceConfig& deviceConfig) {
    _runtimeConfig = &runtimeConfig;
    _deviceConfig = &deviceConfig;

    for (size_t index = 0; index < runtimeConfig.sensorCount; ++index) {
        auto& sensor = runtimeConfig.sensors[index];
        sensor.role = inferRole(sensor);
        sensor.enabled = sensor.role != SensorRole::UNKNOWN;

        switch (sensor.role) {
            case SensorRole::WATER_LEVEL:
                sensor.localKey = "water_level";
                sensor.pollIntervalMs = 1000;
                break;
            case SensorRole::SOIL_MOISTURE:
                sensor.localKey = "soil_moisture";
                sensor.pollIntervalMs = 10000;
                break;
            case SensorRole::WATER_TEMPERATURE:
                sensor.localKey = "water_temperature";
                sensor.pollIntervalMs = 5000;
                break;
            case SensorRole::SURFACE_TEMPERATURE:
                sensor.localKey = "surface_temperature";
                sensor.pollIntervalMs = 5000;
                break;
            case SensorRole::AIR_TEMPERATURE:
                sensor.localKey = "air_temperature";
                sensor.pollIntervalMs = 5000;
                break;
            case SensorRole::AIR_HUMIDITY:
                sensor.localKey = "air_humidity";
                sensor.pollIntervalMs = 5000;
                break;
            default:
                sensor.localKey = "sensor";
                break;
        }
    }
}

bool SensorManager::update(ErrorCode& errorCode) {
    if (_runtimeConfig == nullptr || _deviceConfig == nullptr) {
        return false;
    }

    bool allOk = true;
    const uint32_t nowMs = millis();

    _snapshot.ahtAvailable = _ahtAvailable;
    _snapshot.ds18Available = _dsAvailable;
    _snapshot.soilAvailable = _soilAvailable;

    for (size_t index = 0; index < _runtimeConfig->sensorCount; ++index) {
        auto& sensor = _runtimeConfig->sensors[index];
        if (!sensor.enabled) {
            continue;
        }

        const bool ok = handleSensor(sensor, nowMs);
        if (!ok) {
            allOk = false;
            errorCode = ErrorCode::E007_SENSOR_READ_FAILED;
        }
    }

    return allOk;
}

const SensorSnapshot& SensorManager::getSnapshot() const {
    return _snapshot;
}

bool SensorManager::handleSensor(SensorRuntimeConfig& sensor, uint32_t nowMs) {
    if (nowMs - sensor.lastPollAtMs < sensor.pollIntervalMs) {
        return !sensor.hasError;
    }

    bool ok = false;

    switch (sensor.role) {
        case SensorRole::AIR_TEMPERATURE:
        case SensorRole::AIR_HUMIDITY:
            ok = readAht20(sensor);
            break;
        case SensorRole::WATER_TEMPERATURE:
        case SensorRole::SURFACE_TEMPERATURE:
            ok = readDs18b20(sensor, nowMs);
            break;
        case SensorRole::WATER_LEVEL:
            ok = readWaterLevel(sensor, nowMs);
            break;
        case SensorRole::SOIL_MOISTURE:
            ok = readSoilMoisture(sensor);
            break;
        default:
            ok = false;
            break;
    }

    if (ok) {
        sensor.lastPollAtMs = nowMs;
        sensor.hasError = false;
    } else {
        sensor.hasError = true;
    }

    return ok;
}

bool SensorManager::readAht20(SensorRuntimeConfig& sensor) {
    if (!_ahtAvailable) {
        return false;
    }

    sensors_event_t humidity;
    sensors_event_t temperature;

    for (uint8_t attempt = 0; attempt < 2; ++attempt) {
        if (!_aht.getEvent(&humidity, &temperature)) {
            continue;
        }

        if (sensor.role == SensorRole::AIR_TEMPERATURE) {
            sensor.lastValue = temperature.temperature;
            sensor.lastRawValue = temperature.temperature;
            sensor.hasValue = true;
            _snapshot.airTemperature = sensor.lastValue;
            _snapshot.hasAirTemperature = true;
            return true;
        }

        if (sensor.role == SensorRole::AIR_HUMIDITY) {
            sensor.lastValue = humidity.relative_humidity;
            sensor.lastRawValue = humidity.relative_humidity;
            sensor.hasValue = true;
            _snapshot.humidity = sensor.lastValue;
            _snapshot.hasHumidity = true;
            return true;
        }
    }

    return false;
}

bool SensorManager::readDs18b20(SensorRuntimeConfig& sensor, uint32_t nowMs) {
    if (!_dsAvailable) {
        return false;
    }

    if (!_dsConversionRequested) {
        _ds18b20.requestTemperatures();
        _lastDsRequestAtMs = nowMs;
        _dsConversionRequested = true;
        return !sensor.hasError;
    }

    if (nowMs - _lastDsRequestAtMs < DS18_CONVERSION_WAIT_MS) {
        return !sensor.hasError;
    }

    DeviceAddress address {};
    if (!readDsAddressByRole(sensor.role, address)) {
        return false;
    }

    const float value = _ds18b20.getTempC(address);
    _ds18b20.requestTemperatures();
    _lastDsRequestAtMs = nowMs;
    _dsConversionRequested = true;

    if (!isValidTemperature(value)) {
        return false;
    }

    sensor.lastValue = value;
    sensor.lastRawValue = value;
    sensor.hasValue = true;

    if (sensor.role == SensorRole::WATER_TEMPERATURE) {
        _snapshot.waterTemperature = value;
        _snapshot.hasWaterTemperature = true;
    } else {
        _snapshot.surfaceTemperature = value;
        _snapshot.hasSurfaceTemperature = true;
    }

    return true;
}

bool SensorManager::readWaterLevel(SensorRuntimeConfig& sensor, uint32_t nowMs) {
    if (!_waterLevelAvailable) {
        return false;
    }

    const bool raw = digitalRead(PIN_WATER_LEVEL) == HIGH;
    if (raw != _waterLevelLastRaw) {
        _waterLevelLastRaw = raw;
        _lastWaterLevelChangeMs = nowMs;
    }

    if (nowMs - _lastWaterLevelChangeMs >= _deviceConfig->waterLevelDebounceMs) {
        _waterLevelStable = raw;
    }

    sensor.lastValue = _waterLevelStable ? 1.0f : 0.0f;
    sensor.lastRawValue = raw ? 1.0f : 0.0f;
    sensor.hasValue = true;
    _snapshot.waterLevelSafe = _waterLevelStable;
    return true;
}

bool SensorManager::readSoilMoisture(SensorRuntimeConfig& sensor) {
    uint16_t rawValue = 0;
    if (!tryReadSoilRaw(rawValue)) {
        _soilAvailable = false;
        return false;
    }

    _soilAvailable = true;
    const float calibrated = calibrateSoilMoisture(rawValue);
    sensor.lastValue = calibrated;
    sensor.lastRawValue = static_cast<float>(rawValue);
    sensor.hasValue = true;
    _snapshot.soilMoisture = calibrated;
    _snapshot.soilMoistureRaw = static_cast<float>(rawValue);
    _snapshot.hasSoilMoisture = true;
    return true;
}

SensorRole SensorManager::inferRole(const SensorRuntimeConfig& sensor) const {
    const String name = toLower(sensor.name);
    const String address = toLower(sensor.connectionAddress);

    if (name.indexOf("humidity") >= 0 || sensor.sensorType == 3) {
        return SensorRole::AIR_HUMIDITY;
    }
    if (name.indexOf("water level") >= 0 || name.indexOf("level") >= 0) {
        return SensorRole::WATER_LEVEL;
    }
    if (name.indexOf("soil") >= 0 || name.indexOf("moisture") >= 0) {
        return SensorRole::SOIL_MOISTURE;
    }
    if (name.indexOf("surface") >= 0) {
        return SensorRole::SURFACE_TEMPERATURE;
    }
    if (name.indexOf("water") >= 0 && name.indexOf("temp") >= 0) {
        return SensorRole::WATER_TEMPERATURE;
    }
    if (name.indexOf("air") >= 0 && name.indexOf("temp") >= 0) {
        return SensorRole::AIR_TEMPERATURE;
    }
    if (address.indexOf("0x38") >= 0) {
        return sensor.sensorType == 3 ? SensorRole::AIR_HUMIDITY : SensorRole::AIR_TEMPERATURE;
    }
    if (address.indexOf("28-") >= 0 || address.indexOf("28:") >= 0) {
        if (normalizeAddress(address) == normalizeAddress(_deviceConfig->ds18SurfaceAddress)) {
            return SensorRole::SURFACE_TEMPERATURE;
        }
        return SensorRole::WATER_TEMPERATURE;
    }
    return SensorRole::UNKNOWN;
}

String SensorManager::toLower(const String& value) const {
    String result = value;
    result.toLowerCase();
    return result;
}

uint8_t SensorManager::parseI2cAddress(const String& address) const {
    String normalized = toLower(address);
    normalized.trim();

    if (normalized.startsWith("0x")) {
        return static_cast<uint8_t>(strtol(normalized.c_str(), nullptr, 16));
    }

    return normalized.length() > 0 ? static_cast<uint8_t>(normalized.toInt()) : 0;
}

bool SensorManager::readDsAddressByRole(SensorRole role, DeviceAddress address) const {
    if (_deviceConfig == nullptr) {
        return false;
    }

    const char* raw = role == SensorRole::SURFACE_TEMPERATURE
        ? _deviceConfig->ds18SurfaceAddress
        : _deviceConfig->ds18WaterAddress;

    if (strlen(raw) == 0) {
        return false;
    }

    String normalized = normalizeAddress(raw);
    int outputIndex = 0;
    int tokenStart = 0;

    while (tokenStart < static_cast<int>(normalized.length()) && outputIndex < 8) {
        const int separator = normalized.indexOf('-', tokenStart);
        const String token = separator >= 0
            ? normalized.substring(tokenStart, separator)
            : normalized.substring(tokenStart);

        address[outputIndex++] = static_cast<uint8_t>(strtol(token.c_str(), nullptr, 16));

        if (separator < 0) {
            break;
        }

        tokenStart = separator + 1;
    }

    return outputIndex == 8;
}

bool SensorManager::tryReadSoilRaw(uint16_t& rawValue) {
    uint8_t address = parseI2cAddress("0x36");
    if (_runtimeConfig != nullptr) {
        for (size_t index = 0; index < _runtimeConfig->sensorCount; ++index) {
            if (_runtimeConfig->sensors[index].role == SensorRole::SOIL_MOISTURE) {
                const uint8_t parsed = parseI2cAddress(_runtimeConfig->sensors[index].connectionAddress);
                if (parsed != 0) {
                    address = parsed;
                }
                break;
            }
        }
    }

    for (uint8_t attempt = 0; attempt < SOIL_READ_ATTEMPTS; ++attempt) {
        Wire.beginTransmission(address);
        if (Wire.endTransmission() != 0) {
            continue;
        }

        const uint8_t requested = Wire.requestFrom(static_cast<int>(address), 2);
        if (requested < 2) {
            continue;
        }

        rawValue = static_cast<uint16_t>(Wire.read()) << 8;
        rawValue |= static_cast<uint16_t>(Wire.read());
        return true;
    }

    return false;
}

float SensorManager::calibrateSoilMoisture(uint16_t rawValue) const {
    if (_deviceConfig == nullptr || _deviceConfig->soilDryCalibration <= _deviceConfig->soilWetCalibration) {
        return 0.0f;
    }

    const float dry = static_cast<float>(_deviceConfig->soilDryCalibration);
    const float wet = static_cast<float>(_deviceConfig->soilWetCalibration);
    float percentage = ((dry - static_cast<float>(rawValue)) / (dry - wet)) * 100.0f;

    if (percentage < 0.0f) {
        percentage = 0.0f;
    }
    if (percentage > 100.0f) {
        percentage = 100.0f;
    }

    return percentage;
}

String SensorManager::normalizeAddress(const String& value) const {
    String normalized = value;
    normalized.toLowerCase();
    normalized.replace(":", "-");
    return normalized;
}
