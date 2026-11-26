#include <unity.h>

#include <ArduinoJson.h>
#include <cstdio>
#include <cstring>

#include <ProtocolCodecs.h>
#include <TelemetryQueue.h>

void setUp() {}
void tearDown() {}

void test_encode_telemetry_1_and_20_records_under_4096_bytes() {
    TelemetryRecord records[MAX_MQTT_BATCH_SIZE];
    for (size_t i = 0; i < MAX_MQTT_BATCH_SIZE; ++i) {
        records[i] = TelemetryRecord{};
        copyCString(records[i].sensorId, sizeof(records[i].sensorId), "a3c2f9c1-1111-2222-3333-444444444444");
        TelemetryQueue::formatExternalMessageId(
            records[i].externalMessageId,
            sizeof(records[i].externalMessageId),
            "AABBCCDDEEFF",
            7,
            static_cast<uint32_t>(i + 1),
            "water_temperature");
        copyCString(records[i].recordedAt, sizeof(records[i].recordedAt), "2026-11-26T12:00:00Z");
        records[i].value = 25.4f;
    }

    char buf[MQTT_PACKET_BUFFER_SIZE] = {};
    const size_t len1 = ProtocolCodecs::encodeTelemetryPayload("AA:BB:CC:DD:EE:FF", records, 1, buf, sizeof(buf));
    TEST_ASSERT_GREATER_THAN_UINT32(0, len1);
    TEST_ASSERT_LESS_THAN_UINT32(MQTT_PACKET_BUFFER_SIZE, len1);

    const size_t len20 = ProtocolCodecs::encodeTelemetryPayload("AA:BB:CC:DD:EE:FF", records, MAX_MQTT_BATCH_SIZE, buf, sizeof(buf));
    TEST_ASSERT_GREATER_THAN_UINT32(len1, len20);
    TEST_ASSERT_LESS_THAN_UINT32(MQTT_PACKET_BUFFER_SIZE, len20);

    JsonDocument doc;
    TEST_ASSERT_FALSE(deserializeJson(doc, buf));
    TEST_ASSERT_EQUAL_STRING("AA:BB:CC:DD:EE:FF", doc["macAddress"] | "");
    TEST_ASSERT_EQUAL_UINT32(20, doc["items"].as<JsonArray>().size());
}

void test_encode_ack_relays_state_and_status_payloads() {
    char buf[512] = {};

    // commands/ack completed
    TEST_ASSERT_GREATER_THAN_UINT32(0, ProtocolCodecs::encodeCommandAckPayload("cmd-1", true, nullptr, "2026-11-26T12:00:01Z", buf, sizeof(buf)));
    JsonDocument ackDoc;
    TEST_ASSERT_FALSE(deserializeJson(ackDoc, buf));
    TEST_ASSERT_EQUAL_STRING("completed", ackDoc["status"] | "");
    TEST_ASSERT_TRUE(ackDoc["errorMessage"].isNull());

    // relays/state manual
    RelaySyncItem item{};
    copyCString(item.relayId, sizeof(item.relayId), "b1a4c2b0-1111-2222-3333-444444444444");
    item.state = false;
    item.isManual = true;
    TEST_ASSERT_GREATER_THAN_UINT32(0, ProtocolCodecs::encodeRelaysStatePayload("AA:BB:CC:DD:EE:FF", "manual", "2026-11-26T12:02:15Z", &item, 1, buf, sizeof(buf)));
    JsonDocument rsDoc;
    TEST_ASSERT_FALSE(deserializeJson(rsDoc, buf));
    TEST_ASSERT_EQUAL_STRING("manual", rsDoc["source"] | "");
    TEST_ASSERT_EQUAL_UINT32(1, rsDoc["relays"].as<JsonArray>().size());
    TEST_ASSERT_TRUE(rsDoc["relays"][0]["isManual"].as<bool>());

    // status online + LWT offline
    TEST_ASSERT_GREATER_THAN_UINT32(0, ProtocolCodecs::encodeStatusOnlinePayload("1.0.0", "192.168.1.45", -58, 1240, "2026-11-26T12:05:00Z", buf, sizeof(buf)));
    JsonDocument stDoc;
    TEST_ASSERT_FALSE(deserializeJson(stDoc, buf));
    TEST_ASSERT_EQUAL_STRING("online", stDoc["status"] | "");
    TEST_ASSERT_EQUAL_INT(-58, stDoc["rssi"].as<int>());

    TEST_ASSERT_GREATER_THAN_UINT32(0, ProtocolCodecs::encodeStatusOfflineLwt(buf, sizeof(buf)));
    TEST_ASSERT_EQUAL_STRING("{\"status\":\"offline\"}", buf);
}

void test_decode_command_config_notify_and_controller_config() {
    const char* cmdJson = R"({"commandId":"d1a4c2b0-1111-2222-3333-444444444444","relayId":"b1a4c2b0-1111-2222-3333-444444444444","targetState":true,"expireAt":"2026-11-26T12:10:00Z","createdAt":"2026-11-26T12:00:00Z"})";
    MqttRelayCommand cmd{};
    TEST_ASSERT_TRUE(ProtocolCodecs::decodeCommandPayload(cmdJson, cmd));
    TEST_ASSERT_EQUAL_STRING("d1a4c2b0-1111-2222-3333-444444444444", cmd.commandId);
    TEST_ASSERT_EQUAL_STRING("b1a4c2b0-1111-2222-3333-444444444444", cmd.relayId);
    TEST_ASSERT_TRUE(cmd.targetState);
    TEST_ASSERT_GREATER_THAN_INT64(1700000000LL, cmd.expireAtEpochSec);

    const char* notifyJson = R"({"reason":"relay_or_sensor_updated","version":4,"timestamp":"2026-11-26T12:06:00Z"})";
    ConfigNotifyMessage notify{};
    TEST_ASSERT_TRUE(ProtocolCodecs::decodeConfigNotifyPayload(notifyJson, notify));
    TEST_ASSERT_EQUAL_UINT64(4, notify.version);
    TEST_ASSERT_TRUE(ProtocolCodecs::shouldTriggerConfigSync(notify.version, 3));
    TEST_ASSERT_FALSE(ProtocolCodecs::shouldTriggerConfigSync(notify.version, 4));
    TEST_ASSERT_FALSE(ProtocolCodecs::shouldTriggerConfigSync(notify.version, 5));

    const char* cfgJson = R"({
        "configVersion": 4,
        "sendIntervalMs": 10000,
        "maxBatchSize": 50,
        "relays": [{"relayId":"r-1","name":"Filter","connectionProtocol":1,"connectionAddress":"16","isNormallyOpen":true,"purpose":0,"isActive":true,"isManual":false}],
        "sensors": [{"sensorId":"s-1","name":"Water Temp","connectionProtocol":4,"connectionAddress":"28-000000000001","type":1,"unit":"C"}]
    })";
    BackendRuntimeConfig cfg{};
    TEST_ASSERT_TRUE(ProtocolCodecs::decodeControllerConfig(cfgJson, cfg));
    TEST_ASSERT_TRUE(cfg.isLoaded);
    TEST_ASSERT_EQUAL_UINT64(4, cfg.configVersion);
    TEST_ASSERT_EQUAL_UINT32(1, cfg.sensorCount);
    TEST_ASSERT_EQUAL_UINT32(1, cfg.relayCount);
    TEST_ASSERT_EQUAL_STRING("Filter", cfg.relays[0].name);
    TEST_ASSERT_EQUAL_STRING("16", cfg.relays[0].connectionAddress);
    TEST_ASSERT_EQUAL_STRING("Water Temp", cfg.sensors[0].name);
}

void test_mac_normalization_and_topic_builder() {
    char compact[13] = {};
    char canonical[18] = {};
    char topic[64] = {};
    char clientId[32] = {};

    TEST_ASSERT_TRUE(ProtocolCodecs::normalizeMacCompact("aa:bb:cc:dd:ee:ff", compact, sizeof(compact)));
    TEST_ASSERT_EQUAL_STRING("AABBCCDDEEFF", compact);

    TEST_ASSERT_TRUE(ProtocolCodecs::normalizeMacCanonical("aabbccddeeff", canonical, sizeof(canonical)));
    TEST_ASSERT_EQUAL_STRING("AA:BB:CC:DD:EE:FF", canonical);

    TEST_ASSERT_TRUE(ProtocolCodecs::buildDeviceTopic(topic, sizeof(topic), "AA:BB:CC:DD:EE:FF", "telemetry"));
    TEST_ASSERT_EQUAL_STRING("aquasmart/devices/AABBCCDDEEFF/telemetry", topic);

    TEST_ASSERT_TRUE(ProtocolCodecs::buildClientId(clientId, sizeof(clientId), "aa-bb-cc-dd-ee-ff"));
    TEST_ASSERT_EQUAL_STRING("aqua-esp32-AABBCCDDEEFF", clientId);

    TEST_ASSERT_FALSE(ProtocolCodecs::normalizeMacCompact("invalid-mac", compact, sizeof(compact)));
}

void test_ota_and_ping_protocol_codecs() {
    const char* checkJson = R"({"has_update":true,"campaign_id":"c1111111-1111-1111-1111-111111111111","version":"1.1.0","download_url":"/api/firmware/v1/ota/download/c1111111-1111-1111-1111-111111111111?controller_id=ctrl-1","file_hash":"0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"})";
    OtaCheckInfo check{};
    TEST_ASSERT_TRUE(ProtocolCodecs::decodeOtaCheckPayload(checkJson, check));
    TEST_ASSERT_TRUE(check.hasUpdate);
    TEST_ASSERT_EQUAL_STRING("c1111111-1111-1111-1111-111111111111", check.campaignId);
    TEST_ASSERT_EQUAL_STRING("1.1.0", check.version);
    TEST_ASSERT_EQUAL_STRING("/api/firmware/v1/ota/download/c1111111-1111-1111-1111-111111111111?controller_id=ctrl-1", check.downloadUrl);
    TEST_ASSERT_EQUAL_UINT32(64, std::strlen(check.fileHash));

    char statusBuf[256] = {};
    const size_t len = ProtocolCodecs::encodeOtaStatusPayload(
        "c1111111-1111-1111-1111-111111111111",
        "ctrl-1",
        OtaTargetStatus::DOWNLOADING,
        "",
        statusBuf,
        sizeof(statusBuf));
    TEST_ASSERT_GREATER_THAN_UINT32(0, len);

    JsonDocument statusDoc;
    TEST_ASSERT_FALSE(deserializeJson(statusDoc, statusBuf));
    TEST_ASSERT_EQUAL_UINT32(4, statusDoc.as<JsonObject>().size());
    TEST_ASSERT_EQUAL_STRING("c1111111-1111-1111-1111-111111111111", statusDoc["campaign_id"] | "");
    TEST_ASSERT_EQUAL_STRING("ctrl-1", statusDoc["controller_id"] | "");
    TEST_ASSERT_EQUAL_INT(1, statusDoc["status"].as<int>());
    TEST_ASSERT_EQUAL_STRING("", statusDoc["error_message"] | "missing");

    int64_t pingEpoch = 0;
    TEST_ASSERT_TRUE(ProtocolCodecs::decodePingResponsePayload(R"({"serverTimeUtc":"2026-11-26T14:30:00Z"})", pingEpoch));
    TEST_ASSERT_GREATER_THAN_INT64(1700000000LL, pingEpoch);

    // Zero/oversized sendIntervalMs and maxBatchSize must be clamped to safe defaults
    BackendRuntimeConfig clampedCfg{};
    TEST_ASSERT_TRUE(ProtocolCodecs::decodeControllerConfig(R"({"configVersion":2,"sendIntervalMs":0,"maxBatchSize":999,"sensors":[],"relays":[]})", clampedCfg));
    TEST_ASSERT_EQUAL_UINT32(10000, clampedCfg.sendIntervalMs);
    TEST_ASSERT_EQUAL_UINT16(MAX_HTTP_BATCH_SIZE, clampedCfg.maxBatchSize);
}

int main(int, char**) {
    UNITY_BEGIN();
    RUN_TEST(test_encode_telemetry_1_and_20_records_under_4096_bytes);
    RUN_TEST(test_encode_ack_relays_state_and_status_payloads);
    RUN_TEST(test_decode_command_config_notify_and_controller_config);
    RUN_TEST(test_mac_normalization_and_topic_builder);
    RUN_TEST(test_ota_and_ping_protocol_codecs);
    return UNITY_END();
}
