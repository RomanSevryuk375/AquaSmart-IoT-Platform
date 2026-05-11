#include "TelemetryQueue.h"

void TelemetryQueue::begin(uint16_t maxItems) {
    _capacity = maxItems > MAX_QUEUE_CAPACITY ? MAX_QUEUE_CAPACITY : maxItems;
    _count = 0;
}

bool TelemetryQueue::enqueue(const TelemetryRecord& record, ErrorCode& errorCode) {
    if (_count >= _capacity) {
        dropOldestNonCritical();
        if (_count >= _capacity) {
            errorCode = ErrorCode::E009_QUEUE_OVERFLOW;
            return false;
        }
        errorCode = ErrorCode::E009_QUEUE_OVERFLOW;
    }

    _records[_count] = record;
    _records[_count].occupied = true;
    ++_count;
    return true;
}

size_t TelemetryQueue::peekBatch(TelemetryRecord* output, size_t maxItems) const {
    const size_t take = _count < maxItems ? _count : maxItems;
    for (size_t index = 0; index < take; ++index) {
        output[index] = _records[index];
    }
    return take;
}

void TelemetryQueue::removeAccepted(size_t acceptedCount) {
    if (acceptedCount == 0 || _count == 0) {
        return;
    }

    if (acceptedCount >= _count) {
        _count = 0;
        return;
    }

    const size_t remaining = _count - acceptedCount;
    for (size_t index = 0; index < remaining; ++index) {
        _records[index] = _records[index + acceptedCount];
    }

    _count = remaining;
}

size_t TelemetryQueue::count() const {
    return _count;
}

size_t TelemetryQueue::capacity() const {
    return _capacity;
}

void TelemetryQueue::dropOldestNonCritical() {
    size_t dropIndex = _capacity;
    for (size_t index = 0; index < _count; ++index) {
        if (!_records[index].critical) {
            dropIndex = index;
            break;
        }
    }

    if (dropIndex >= _count) {
        return;
    }

    for (size_t index = dropIndex; index + 1 < _count; ++index) {
        _records[index] = _records[index + 1];
    }

    --_count;
}
