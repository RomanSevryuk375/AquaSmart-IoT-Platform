#include "OtaStateMachine.h"

#include <cctype>
#include <cstring>

void OtaStateMachine::reset() {
    _phase = OtaPhase::IDLE;
    _campaignId[0] = '\0';
    _targetVersion[0] = '\0';
    _expectedHash[0] = '\0';
}

OtaStepAction OtaStateMachine::evaluateCheckResponse(const OtaCheckInfo& check, const char* currentFwVersion) {
    OtaStepAction action{};

    if (!check.hasUpdate) {
        _phase = OtaPhase::IDLE;
        return action;
    }

    if (check.campaignId[0] == '\0' || check.version[0] == '\0' || check.downloadUrl[0] == '\0') {
        _phase = OtaPhase::FAILED;
        action.errorCode = ErrorCode::E012_OTA_FAILED;
        copyCString(action.errorMessage, sizeof(action.errorMessage), "Incomplete OTA metadata");
        return action;
    }

    if (currentFwVersion != nullptr && std::strcmp(check.version, currentFwVersion) == 0) {
        _phase = OtaPhase::IDLE;
        return action;
    }

    copyCString(_campaignId, sizeof(_campaignId), check.campaignId);
    copyCString(_targetVersion, sizeof(_targetVersion), check.version);
    copyCString(_expectedHash, sizeof(_expectedHash), check.fileHash);
    _phase = OtaPhase::DOWNLOADING;

    action.savePendingInNvs = true;
    action.enterSafeState = true;
    action.reportStatus = true;
    action.statusToReport = OtaTargetStatus::DOWNLOADING;
    copyCString(action.statusCampaignId, sizeof(action.statusCampaignId), _campaignId);
    return action;
}

OtaStepAction OtaStateMachine::onDownloadComplete(bool downloadOk, const char* computedSha256, const char* downloadError) {
    OtaStepAction action{};
    copyCString(action.statusCampaignId, sizeof(action.statusCampaignId), _campaignId);

    if (_phase != OtaPhase::DOWNLOADING) {
        return action;
    }

    if (!downloadOk) {
        _phase = OtaPhase::FAILED;
        action.reportStatus = true;
        action.statusToReport = OtaTargetStatus::FAILED;
        action.clearPendingInNvs = true;
        action.errorCode = ErrorCode::E012_OTA_FAILED;
        copyCString(
            action.errorMessage,
            sizeof(action.errorMessage),
            (downloadError != nullptr && downloadError[0] != '\0') ? downloadError : "E012 OTA download failed");
        return action;
    }

    if (_expectedHash[0] != '\0' && !sha256EqualsIgnoreCase(_expectedHash, computedSha256)) {
        _phase = OtaPhase::FAILED;
        action.reportStatus = true;
        action.statusToReport = OtaTargetStatus::FAILED;
        action.clearPendingInNvs = true;
        action.rebootRequired = false;
        action.errorCode = ErrorCode::E012_OTA_FAILED;
        copyCString(action.errorMessage, sizeof(action.errorMessage), "E012 SHA-256 mismatch");
        return action;
    }

    _phase = OtaPhase::FLASHING;
    action.reportStatus = true;
    action.statusToReport = OtaTargetStatus::FLASHING;
    return action;
}

OtaStepAction OtaStateMachine::onFlashComplete(bool flashOk, const char* flashError) {
    OtaStepAction action{};
    copyCString(action.statusCampaignId, sizeof(action.statusCampaignId), _campaignId);

    if (_phase != OtaPhase::FLASHING) {
        return action;
    }

    if (!flashOk) {
        _phase = OtaPhase::FAILED;
        action.reportStatus = true;
        action.statusToReport = OtaTargetStatus::FAILED;
        action.clearPendingInNvs = true;
        action.rebootRequired = false;
        action.errorCode = ErrorCode::E012_OTA_FAILED;
        copyCString(
            action.errorMessage,
            sizeof(action.errorMessage),
            (flashError != nullptr && flashError[0] != '\0') ? flashError : "E012 Flash write failed");
        return action;
    }

    _phase = OtaPhase::REBOOT_REQUIRED;
    action.rebootRequired = true;
    return action;
}

OtaStepAction OtaStateMachine::evaluatePostBoot(
    bool hasPendingOta,
    const char* pendingCampaignId,
    const char* pendingVersion,
    const char* runningFwVersion) {
    OtaStepAction action{};

    if (!hasPendingOta || pendingCampaignId == nullptr || pendingCampaignId[0] == '\0') {
        _phase = OtaPhase::IDLE;
        return action;
    }

    copyCString(_campaignId, sizeof(_campaignId), pendingCampaignId);
    copyCString(action.statusCampaignId, sizeof(action.statusCampaignId), pendingCampaignId);
    action.clearPendingInNvs = true;
    action.reportStatus = true;

    const bool versionMatched =
        pendingVersion != nullptr &&
        runningFwVersion != nullptr &&
        pendingVersion[0] != '\0' &&
        std::strcmp(runningFwVersion, pendingVersion) == 0;

    if (versionMatched) {
        _phase = OtaPhase::COMPLETED;
        action.markAppValidCancelRollback = true;
        action.statusToReport = OtaTargetStatus::SUCCESS;
        return action;
    }

    _phase = OtaPhase::FAILED;
    action.statusToReport = OtaTargetStatus::FAILED;
    action.errorCode = ErrorCode::E012_OTA_FAILED;
    copyCString(action.errorMessage, sizeof(action.errorMessage), "Rollback triggered after boot failure");
    return action;
}

bool OtaStateMachine::sha256EqualsIgnoreCase(const char* a, const char* b) {
    if (a == nullptr || b == nullptr) {
        return false;
    }
    const size_t lenA = std::strlen(a);
    const size_t lenB = std::strlen(b);
    if (lenA != 64 || lenB != 64) {
        return false;
    }
    for (size_t i = 0; i < 64; ++i) {
        if (std::tolower(static_cast<unsigned char>(a[i])) !=
            std::tolower(static_cast<unsigned char>(b[i]))) {
            return false;
        }
    }
    return true;
}
