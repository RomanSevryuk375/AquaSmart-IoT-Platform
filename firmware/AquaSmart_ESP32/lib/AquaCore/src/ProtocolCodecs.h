#ifndef AQUA_CORE_PROTOCOL_CODECS_H
#define AQUA_CORE_PROTOCOL_CODECS_H

#include "CoreTypes.h"
#include "OtaStateMachine.h"

struct RelaySyncItem {
    char relayId[40] = {};
    bool state = false;
    bool isManual = false;
};

struct MqttRelayCommand {
    char commandId[40] = {};
    char relayId[40] = {};
    bool targetState = false;
    char expireAt[32] = {};
    char createdAt[32] = {};
    int64_t expireAtEpochSec = 0;
};

struct ConfigNotifyMessage {
    char reason[48] = {};
    uint64_t version = 0;
    char timestamp[32] = {};
};

namespace ProtocolCodecs {

bool normalizeMacCompact(const char* rawMac, char* outCompact, size_t outSize);
bool normalizeMacCanonical(const char* rawMac, char* outCanonical, size_t outSize);

bool buildDeviceTopic(char* outTopic, size_t outSize, const char* macAddress, const char* suffix);
bool buildClientId(char* outClientId, size_t outSize, const char* macAddress);

size_t encodeTelemetryPayload(
    const char* canonicalMac,
    const TelemetryRecord* records,
    size_t count,
    char* outJson,
    size_t outSize);

size_t encodeCommandAckPayload(
    const char* commandId,
    bool completed,
    const char* errorMessage,
    const char* executedAtUtc,
    char* outJson,
    size_t outSize);

size_t encodeRelaysStatePayload(
    const char* canonicalMac,
    const char* source,
    const char* recordedAtUtc,
    const RelaySyncItem* items,
    size_t count,
    char* outJson,
    size_t outSize);

size_t encodeStatusOnlinePayload(
    const char* fwVersion,
    const char* ip,
    int rssi,
    uint32_t uptimeSec,
    const char* timestampUtc,
    char* outJson,
    size_t outSize);

size_t encodeStatusOfflineLwt(char* outJson, size_t outSize);

bool decodeCommandPayload(const char* json, MqttRelayCommand& outCmd);
bool decodeConfigNotifyPayload(const char* json, ConfigNotifyMessage& outNotify);
bool shouldTriggerConfigSync(uint64_t incomingVersion, uint64_t localConfigVersion);
bool decodeControllerConfig(const char* json, BackendRuntimeConfig& outConfig);
bool decodePingResponsePayload(const char* json, int64_t& outServerEpochSec);
bool decodeOtaCheckPayload(const char* json, OtaCheckInfo& outCheck);
size_t encodeOtaStatusPayload(
    const char* campaignId,
    const char* controllerId,
    OtaTargetStatus status,
    const char* errorMessage,
    char* outJson,
    size_t outSize);

bool parseIso8601UtcToEpoch(const char* isoUtc, int64_t& outEpochSec);

}  // namespace ProtocolCodecs

#endif
