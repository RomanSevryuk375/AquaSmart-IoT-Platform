#ifndef TELEMETRY_QUEUE_H
#define TELEMETRY_QUEUE_H

#include "AppState.h"

class TelemetryQueue {
public:
    void begin(uint16_t maxItems);
    bool enqueue(const TelemetryRecord& record, ErrorCode& errorCode);
    size_t peekBatch(TelemetryRecord* output, size_t maxItems) const;
    void removeAccepted(size_t acceptedCount);
    size_t count() const;
    size_t capacity() const;

private:
    TelemetryRecord _records[MAX_QUEUE_CAPACITY];
    size_t _count = 0;
    uint16_t _capacity = 200;

    void dropOldestNonCritical();
};

#endif
