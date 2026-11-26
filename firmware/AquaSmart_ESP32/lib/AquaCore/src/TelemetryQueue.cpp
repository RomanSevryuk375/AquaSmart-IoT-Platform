#include "TelemetryQueue.h"

#include <cstdio>
#include <cstring>

void TelemetryQueue::begin(uint16_t maxItems) {
    if (maxItems == 0 || maxItems > MAX_QUEUE_CAPACITY) {
        _capacity = MAX_QUEUE_CAPACITY;
    } else {
        _capacity = maxItems;
    }
    _count = 0;
}

bool TelemetryQueue::enqueue(const TelemetryRecord& record, ErrorCode& errorCode) {
    if (_count >= _capacity) {
        errorCode = ErrorCode::E009_QUEUE_OVERFLOW;
        if (!dropOldestNonCritical()) {
            return false;
        }
    }

    _records[_count] = record;
    _records[_count].occupied = true;
    ++_count;
    return true;
}

size_t TelemetryQueue::peekBatch(TelemetryRecord* output, size_t maxItems) const {
    if (output == nullptr || maxItems == 0) {
        return 0;
    }
    const size_t take = _count < maxItems ? _count : maxItems;
    for (size_t i = 0; i < take; ++i) {
        output[i] = _records[i];
    }
    return take;
}

void TelemetryQueue::removeProcessed(size_t acceptedCount, size_t skippedCount) {
    const size_t totalToRemove = acceptedCount + skippedCount;
    if (totalToRemove == 0 || _count == 0) {
        return;
    }

    if (totalToRemove >= _count) {
        _count = 0;
        return;
    }

    const size_t remaining = _count - totalToRemove;
    for (size_t i = 0; i < remaining; ++i) {
        _records[i] = _records[i + totalToRemove];
    }
    _count = remaining;
}

void TelemetryQueue::removeAccepted(size_t acceptedCount) {
    removeProcessed(acceptedCount, 0);
}

size_t TelemetryQueue::count() const {
    return _count;
}

size_t TelemetryQueue::capacity() const {
    return _capacity;
}

size_t TelemetryQueue::computeBatchLimit(size_t configuredMaxBatchSize, bool isMqttTransport) {
    const size_t transportCap = isMqttTransport ? MAX_MQTT_BATCH_SIZE : MAX_HTTP_BATCH_SIZE;
    if (configuredMaxBatchSize == 0) {
        return transportCap;
    }
    return configuredMaxBatchSize < transportCap ? configuredMaxBatchSize : transportCap;
}

bool TelemetryQueue::formatExternalMessageId(
    char* out,
    size_t outSize,
    const char* compactMac,
    uint32_t bootCounter,
    uint32_t sequence,
    const char* sensorKey) {
    if (out == nullptr || outSize == 0) {
        return false;
    }

    const size_t effectiveMaxLen = (outSize - 1) < MAX_EXTERNAL_MESSAGE_ID_LEN
        ? (outSize - 1)
        : MAX_EXTERNAL_MESSAGE_ID_LEN;

    const char* safeMac = (compactMac != nullptr && compactMac[0] != '\0') ? compactMac : "000000000000";
    const char* safeKey = (sensorKey != nullptr && sensorKey[0] != '\0') ? sensorKey : "sensor";

    char prefix[64] = {};
    const int prefixLen = std::snprintf(
        prefix,
        sizeof(prefix),
        "%s-%lu-%lu-",
        safeMac,
        static_cast<unsigned long>(bootCounter),
        static_cast<unsigned long>(sequence));

    if (prefixLen <= 0 || static_cast<size_t>(prefixLen) >= effectiveMaxLen) {
        std::snprintf(out, outSize, "%.*s", static_cast<int>(effectiveMaxLen), prefix);
        return false;
    }

    const size_t allowedKeyLen = effectiveMaxLen - static_cast<size_t>(prefixLen);
    std::snprintf(out, outSize, "%s%.*s", prefix, static_cast<int>(allowedKeyLen), safeKey);
    return true;
}

bool TelemetryQueue::isCriticalRole(SensorRole role) {
    return role == SensorRole::WATER_LEVEL || role == SensorRole::WATER_TEMPERATURE;
}

bool TelemetryQueue::dropOldestNonCritical() {
    size_t dropIndex = _count;
    for (size_t i = 0; i < _count; ++i) {
        if (!_records[i].critical) {
            dropIndex = i;
            break;
        }
    }

    if (dropIndex >= _count) {
        return false;
    }

    for (size_t i = dropIndex; i + 1 < _count; ++i) {
        _records[i] = _records[i + 1];
    }
    --_count;
    return true;
}
