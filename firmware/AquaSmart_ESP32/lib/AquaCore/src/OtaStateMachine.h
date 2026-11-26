#ifndef AQUA_CORE_OTA_STATE_MACHINE_H
#define AQUA_CORE_OTA_STATE_MACHINE_H

#include "CoreTypes.h"

enum class OtaTargetStatus : uint8_t {
    PENDING = 0,
    DOWNLOADING = 1,
    FLASHING = 2,
    SUCCESS = 3,
    FAILED = 4
};

enum class OtaPhase : uint8_t {
    IDLE = 0,
    DOWNLOADING = 1,
    FLASHING = 2,
    REBOOT_REQUIRED = 3,
    COMPLETED = 4,
    FAILED = 5
};

struct OtaCheckInfo {
    bool hasUpdate = false;
    char campaignId[40] = {};
    char version[24] = {};
    char downloadUrl[160] = {};
    char fileHash[65] = {};
};

struct OtaStepAction {
    bool reportStatus = false;
    OtaTargetStatus statusToReport = OtaTargetStatus::PENDING;
    char statusCampaignId[40] = {};
    char errorMessage[96] = {};
    bool savePendingInNvs = false;
    bool clearPendingInNvs = false;
    bool enterSafeState = false;
    bool rebootRequired = false;
    bool markAppValidCancelRollback = false;
    ErrorCode errorCode = ErrorCode::NONE;
};

class OtaStateMachine {
public:
    void reset();

    OtaStepAction evaluateCheckResponse(const OtaCheckInfo& check, const char* currentFwVersion);
    OtaStepAction onDownloadComplete(bool downloadOk, const char* computedSha256, const char* downloadError = "");
    OtaStepAction onFlashComplete(bool flashOk, const char* flashError = "");

    OtaStepAction evaluatePostBoot(
        bool hasPendingOta,
        const char* pendingCampaignId,
        const char* pendingVersion,
        const char* runningFwVersion);

    OtaPhase phase() const { return _phase; }
    const char* activeCampaignId() const { return _campaignId; }
    const char* targetVersion() const { return _targetVersion; }
    const char* expectedSha256() const { return _expectedHash; }

    static bool sha256EqualsIgnoreCase(const char* a, const char* b);

private:
    OtaPhase _phase = OtaPhase::IDLE;
    char _campaignId[40] = {};
    char _targetVersion[24] = {};
    char _expectedHash[65] = {};
};

#endif
