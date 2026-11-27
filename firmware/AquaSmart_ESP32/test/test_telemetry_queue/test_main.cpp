#include <unity.h>

#include <cstdio>
#include <cstring>
#include <type_traits>

#include <TelemetryQueue.h>

namespace {
TelemetryRecord makeRecord(const char* extId, float value, bool critical) {
    TelemetryRecord r{};
    copyCString(r.sensorId, sizeof(r.sensorId), "a3c2f9c1-1111-2222-3333-444444444444");
    copyCString(r.localSensorKey, sizeof(r.localSensorKey), critical ? "water_temperature" : "air_humidity");
    copyCString(r.externalMessageId, sizeof(r.externalMessageId), extId);
    copyCString(r.recordedAt, sizeof(r.recordedAt), "2026-11-26T12:00:00Z");
    r.value = value;
    r.critical = critical;
    return r;
}
}  // namespace

void setUp() {}
void tearDown() {}

void test_telemetry_record_has_fixed_char_buffers_and_no_arduino_string() {
    TEST_ASSERT_TRUE(std::is_trivially_copyable<TelemetryRecord>::value);
    TEST_ASSERT_EQUAL_UINT32(37, sizeof(TelemetryRecord::sensorId));
    TEST_ASSERT_EQUAL_UINT32(101, sizeof(TelemetryRecord::externalMessageId));
    TEST_ASSERT_EQUAL_UINT32(25, sizeof(TelemetryRecord::recordedAt));
    TEST_ASSERT_LESS_OR_EQUAL_UINT32(220, sizeof(TelemetryRecord));
    TEST_ASSERT_EQUAL_UINT32(200, MAX_QUEUE_CAPACITY);
}

void test_enqueue_and_fifo_dequeue_up_to_200_capacity() {
    TelemetryQueue queue;
    queue.begin(MAX_QUEUE_CAPACITY);
    TEST_ASSERT_EQUAL_UINT32(200, queue.capacity());

    ErrorCode err = ErrorCode::NONE;
    for (int i = 0; i < 200; ++i) {
        char id[32];
        std::snprintf(id, sizeof(id), "AABBCCDDEEFF-1-%d-0", i);
        TEST_ASSERT_TRUE(queue.enqueue(makeRecord(id, static_cast<float>(i), false), err));
    }

    TEST_ASSERT_EQUAL_UINT32(200, queue.count());
    TEST_ASSERT_EQUAL_INT(static_cast<int>(ErrorCode::NONE), static_cast<int>(err));

    TelemetryRecord batch[5];
    const size_t taken = queue.peekBatch(batch, 5);
    TEST_ASSERT_EQUAL_UINT32(5, taken);
    TEST_ASSERT_EQUAL_STRING("AABBCCDDEEFF-1-0-0", batch[0].externalMessageId);
    TEST_ASSERT_EQUAL_STRING("AABBCCDDEEFF-1-4-0", batch[4].externalMessageId);

    queue.removeProcessed(3, 0);
    TEST_ASSERT_EQUAL_UINT32(197, queue.count());
    queue.peekBatch(batch, 1);
    TEST_ASSERT_EQUAL_STRING("AABBCCDDEEFF-1-3-0", batch[0].externalMessageId);
}

void test_overflow_evicts_oldest_non_critical_and_preserves_critical() {
    TelemetryQueue queue;
    queue.begin(3);

    ErrorCode err = ErrorCode::NONE;
    TEST_ASSERT_TRUE(queue.enqueue(makeRecord("CRIT-1", 25.0f, true), err));
    TEST_ASSERT_TRUE(queue.enqueue(makeRecord("NONCRIT-1", 55.0f, false), err));
    TEST_ASSERT_TRUE(queue.enqueue(makeRecord("CRIT-2", 1.0f, true), err));
    TEST_ASSERT_EQUAL_INT(static_cast<int>(ErrorCode::NONE), static_cast<int>(err));

    // Enqueue 4th item -> must evict NONCRIT-1 and set E009
    TEST_ASSERT_TRUE(queue.enqueue(makeRecord("CRIT-3", 26.0f, true), err));
    TEST_ASSERT_EQUAL_INT(static_cast<int>(ErrorCode::E009_QUEUE_OVERFLOW), static_cast<int>(err));
    TEST_ASSERT_EQUAL_UINT32(3, queue.count());

    TelemetryRecord out[3];
    queue.peekBatch(out, 3);
    TEST_ASSERT_EQUAL_STRING("CRIT-1", out[0].externalMessageId);
    TEST_ASSERT_EQUAL_STRING("CRIT-2", out[1].externalMessageId);
    TEST_ASSERT_EQUAL_STRING("CRIT-3", out[2].externalMessageId);

    // Now all 3 records are critical -> next enqueue must fail and keep all 3 critical records
    err = ErrorCode::NONE;
    TEST_ASSERT_FALSE(queue.enqueue(makeRecord("NONCRIT-2", 60.0f, false), err));
    TEST_ASSERT_EQUAL_INT(static_cast<int>(ErrorCode::E009_QUEUE_OVERFLOW), static_cast<int>(err));
    TEST_ASSERT_EQUAL_UINT32(3, queue.count());
}

void test_batch_slicing_limits_for_mqtt_and_http_fallback() {
    // Even if server config maxBatchSize == 50, MQTT must cap at 20, HTTP allows 50.
    TEST_ASSERT_EQUAL_UINT32(20, TelemetryQueue::computeBatchLimit(50, true));
    TEST_ASSERT_EQUAL_UINT32(15, TelemetryQueue::computeBatchLimit(15, true));
    TEST_ASSERT_EQUAL_UINT32(50, TelemetryQueue::computeBatchLimit(50, false));
    TEST_ASSERT_EQUAL_UINT32(50, TelemetryQueue::computeBatchLimit(80, false));
    TEST_ASSERT_EQUAL_UINT32(20, TelemetryQueue::computeBatchLimit(0, true));
    TEST_ASSERT_EQUAL_UINT32(50, TelemetryQueue::computeBatchLimit(0, false));
}

void test_remove_processed_dequeues_accepted_plus_skipped_preventing_head_block() {
    TelemetryQueue queue;
    queue.begin(10);

    ErrorCode err = ErrorCode::NONE;
    queue.enqueue(makeRecord("DUP-1", 21.0f, false), err);
    queue.enqueue(makeRecord("DUP-2", 22.0f, false), err);
    queue.enqueue(makeRecord("FRESH-3", 23.0f, false), err);

    // Backend reports accepted=0, skipped=2 (duplicates) -> head MUST advance by 2!
    queue.removeProcessed(0, 2);
    TEST_ASSERT_EQUAL_UINT32(1, queue.count());

    TelemetryRecord out[1];
    queue.peekBatch(out, 1);
    TEST_ASSERT_EQUAL_STRING("FRESH-3", out[0].externalMessageId);
}

void test_external_message_id_format_bounds_and_immutability_on_retry() {
    char id[MAX_EXTERNAL_MESSAGE_ID_LEN + 1] = {};
    TEST_ASSERT_TRUE(TelemetryQueue::formatExternalMessageId(id, sizeof(id), "AABBCCDDEEFF", 7, 42, "water_temp"));
    TEST_ASSERT_EQUAL_STRING("AABBCCDDEEFF-7-42-water_temp", id);
    TEST_ASSERT_LESS_OR_EQUAL_UINT32(100, std::strlen(id));

    // Very long sensor key must be truncated cleanly to <= 100 chars
    const char* longKey = "extremely_long_sensor_key_name_that_exceeds_the_one_hundred_character_backend_validation_limit_by_a_lot";
    char boundedId[MAX_EXTERNAL_MESSAGE_ID_LEN + 1] = {};
    TEST_ASSERT_TRUE(TelemetryQueue::formatExternalMessageId(boundedId, sizeof(boundedId), "AABBCCDDEEFF", 1234, 999999, longKey));
    TEST_ASSERT_EQUAL_UINT32(100, std::strlen(boundedId));

    // Retrying peekBatch must preserve the exact same externalMessageId
    TelemetryQueue queue;
    queue.begin(10);
    ErrorCode err = ErrorCode::NONE;
    queue.enqueue(makeRecord(id, 24.5f, true), err);

    TelemetryRecord firstTry[1];
    TelemetryRecord secondTry[1];
    queue.peekBatch(firstTry, 1);
    queue.peekBatch(secondTry, 1);
    TEST_ASSERT_EQUAL_STRING(firstTry[0].externalMessageId, secondTry[0].externalMessageId);
}

void test_critical_role_classification() {
    TEST_ASSERT_TRUE(TelemetryQueue::isCriticalRole(SensorRole::WATER_LEVEL));
    TEST_ASSERT_TRUE(TelemetryQueue::isCriticalRole(SensorRole::WATER_TEMPERATURE));
    TEST_ASSERT_FALSE(TelemetryQueue::isCriticalRole(SensorRole::AIR_HUMIDITY));
    TEST_ASSERT_FALSE(TelemetryQueue::isCriticalRole(SensorRole::SOIL_MOISTURE));
}

int main(int, char**) {
    UNITY_BEGIN();
    RUN_TEST(test_telemetry_record_has_fixed_char_buffers_and_no_arduino_string);
    RUN_TEST(test_enqueue_and_fifo_dequeue_up_to_200_capacity);
    RUN_TEST(test_overflow_evicts_oldest_non_critical_and_preserves_critical);
    RUN_TEST(test_batch_slicing_limits_for_mqtt_and_http_fallback);
    RUN_TEST(test_remove_processed_dequeues_accepted_plus_skipped_preventing_head_block);
    RUN_TEST(test_external_message_id_format_bounds_and_immutability_on_retry);
    RUN_TEST(test_critical_role_classification);
    return UNITY_END();
}
