#ifndef AQUA_CORE_TELEMETRY_QUEUE_H
#define AQUA_CORE_TELEMETRY_QUEUE_H

#include "CoreTypes.h"

class TelemetryQueue {
public:
    void begin(uint16_t maxItems = MAX_QUEUE_CAPACITY);
    bool enqueue(const TelemetryRecord& record, ErrorCode& errorCode);
    size_t peekBatch(TelemetryRecord* output, size_t maxItems) const;
    void removeProcessed(size_t acceptedCount, size_t skippedCount);
    void removeAccepted(size_t acceptedCount);
    size_t count() const;
    size_t capacity() const;

    static size_t computeBatchLimit(size_t configuredMaxBatchSize, bool isMqttTransport);

    static bool formatExternalMessageId(
        char* out,
        size_t outSize,
        const char* compactMac,
        uint32_t bootCounter,
        uint32_t sequence,
        const char* sensorKey);

    static bool isCriticalRole(SensorRole role);

private:
    TelemetryRecord _records[MAX_QUEUE_CAPACITY] = {};
    size_t _count = 0;
    uint16_t _capacity = MAX_QUEUE_CAPACITY;

    bool dropOldestNonCritical();
};

#endif
