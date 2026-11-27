#include "OtaManager.h"

#include <HTTPClient.h>
#include <Update.h>
#include <esp_ota_ops.h>
#include <mbedtls/sha256.h>
#include <mbedtls/version.h>
#include <cstdio>
#include <cstring>

#include "ConfigStore.h"

namespace {
void formatSha256Hex(const uint8_t digest[32], char* outHex, size_t outSize) {
    if (outHex == nullptr || outSize < 65) {
        return;
    }
    static constexpr char HEX_DIGITS[] = "0123456789abcdef";
    for (size_t i = 0; i < 32; ++i) {
        outHex[i * 2] = HEX_DIGITS[(digest[i] >> 4) & 0x0F];
        outHex[i * 2 + 1] = HEX_DIGITS[digest[i] & 0x0F];
    }
    outHex[64] = '\0';
}
}  // namespace

void OtaManager::begin(ApiClient* apiClient, RelayManager* relayManager, DisplayManager* displayManager) {
    _apiClient = apiClient;
    _relayManager = relayManager;
    _displayManager = displayManager;
    _stateMachine.reset();
    _postBootConfirmed = false;
    _postBootRetryCount = 0;
}

bool OtaManager::handlePostBootConfirmation(DeviceConfig& config, ErrorCode& errorOut) {
    errorOut = ErrorCode::NONE;
    if (_postBootConfirmed || _apiClient == nullptr) {
        return true;
    }

    const bool hasPending = (config.pendingOtaCampaignId[0] != '\0');
    if (!hasPending) {
        _postBootConfirmed = true;
        return true;
    }

    const OtaStepAction action = _stateMachine.evaluatePostBoot(
        true,
        config.pendingOtaCampaignId,
        config.pendingOtaVersion,
        FIRMWARE_VERSION);

    if (action.markAppValidCancelRollback) {
        const esp_err_t err = esp_ota_mark_app_valid_cancel_rollback();
        Serial.printf("[OTA] esp_ota_mark_app_valid_cancel_rollback -> %d\n", static_cast<int>(err));
    }

    if (action.errorCode != ErrorCode::NONE) {
        errorOut = action.errorCode;
    }

    bool statusReported = true;
    if (action.reportStatus) {
        const ApiResult res = _apiClient->reportOtaStatus(
            action.statusCampaignId,
            config.controllerId,
            action.statusToReport,
            action.errorMessage);
        // 204/2xx = ok; 404/409 = campaign already finalized/closed, clear NVS to avoid endless loop.
        statusReported = res.success || res.statusCode == 404 || res.statusCode == 409;
        Serial.printf("[OTA] Post-boot status=%d report HTTP=%d (%s)\n",
                      static_cast<int>(action.statusToReport),
                      res.statusCode,
                      res.message.c_str());
    }

    if (statusReported || ++_postBootRetryCount >= 3) {
        if (action.clearPendingInNvs) {
            ConfigStore::clearPendingOta();
            config.pendingOtaCampaignId[0] = '\0';
            config.pendingOtaVersion[0] = '\0';
        }
        _postBootConfirmed = true;
        return true;
    }

    return false;
}

bool OtaManager::checkAndPerformUpdate(DeviceConfig& config, SystemState& systemState, ErrorCode& errorOut) {
    errorOut = ErrorCode::NONE;
    if (_apiClient == nullptr || config.controllerId[0] == '\0') {
        return false;
    }

    OtaCheckInfo check{};
    const ApiResult checkRes = _apiClient->checkOtaUpdate(config.controllerId, check);
    if (!checkRes.success) {
        Serial.printf("[OTA] Check failed HTTP=%d: %s\n", checkRes.statusCode, checkRes.message.c_str());
        errorOut = (checkRes.statusCode == 401 || checkRes.statusCode == 403)
            ? ErrorCode::E010_DEVICE_TOKEN_MISSING
            : ErrorCode::E012_OTA_FAILED;
        return false;
    }

    _stateMachine.reset();
    const OtaStepAction checkAction = _stateMachine.evaluateCheckResponse(check, FIRMWARE_VERSION);
    if (checkAction.errorCode != ErrorCode::NONE) {
        errorOut = checkAction.errorCode;
        Serial.printf("[OTA] Invalid check metadata: %s\n", checkAction.errorMessage);
        return false;
    }

    if (!checkAction.reportStatus) {
        Serial.println("[OTA] Firmware is up to date.");
        return true;
    }

    const SystemState previousState = systemState;
    systemState = SystemState::OTA_UPDATING;

    if (checkAction.savePendingInNvs) {
        copyCString(config.pendingOtaCampaignId, sizeof(config.pendingOtaCampaignId), check.campaignId);
        copyCString(config.pendingOtaVersion, sizeof(config.pendingOtaVersion), check.version);
        ConfigStore::savePendingOta(config.pendingOtaCampaignId, config.pendingOtaVersion);
    }

    if (checkAction.enterSafeState && _relayManager != nullptr) {
        _relayManager->setAllSafe();
    }

    if (_displayManager != nullptr) {
        char line1[17];
        snprintf(line1, sizeof(line1), "v%-15.15s", check.version);
        _displayManager->showBanner("OTA Downloading", line1);
    }

    _apiClient->reportOtaStatus(
        checkAction.statusCampaignId,
        config.controllerId,
        checkAction.statusToReport,
        "");

    char computedSha[65] = {};
    char dlError[96] = {};
    const bool dlOk = streamDownloadAndWrite(
        check.downloadUrl,
        computedSha,
        sizeof(computedSha),
        dlError,
        sizeof(dlError));

    const OtaStepAction dlAction = _stateMachine.onDownloadComplete(dlOk, computedSha, dlError);
    if (dlAction.errorCode != ErrorCode::NONE) {
        if (Update.isRunning()) {
            Update.abort();
        }
        errorOut = dlAction.errorCode;
        Serial.printf("[OTA] Download/verify failed: %s (computed=%s expected=%s)\n",
                      dlAction.errorMessage,
                      computedSha,
                      check.fileHash);
        if (dlAction.clearPendingInNvs) {
            ConfigStore::clearPendingOta();
            config.pendingOtaCampaignId[0] = '\0';
            config.pendingOtaVersion[0] = '\0';
        }
        if (dlAction.reportStatus) {
            _apiClient->reportOtaStatus(
                dlAction.statusCampaignId,
                config.controllerId,
                dlAction.statusToReport,
                dlAction.errorMessage);
        }
        if (_displayManager != nullptr) {
            _displayManager->showBanner("OTA Failed E012", dlAction.errorMessage);
        }
        systemState = previousState;
        return false;
    }

    if (dlAction.reportStatus) {
        if (_displayManager != nullptr) {
            _displayManager->showBanner("OTA Flashing...", check.version);
        }
        _apiClient->reportOtaStatus(
            dlAction.statusCampaignId,
            config.controllerId,
            dlAction.statusToReport,
            "");
    }

    const bool flashOk = Update.end(true);
    char flashErr[96] = {};
    if (!flashOk) {
        snprintf(flashErr, sizeof(flashErr), "Update.end failed (%u)", static_cast<unsigned>(Update.getError()));
    }

    const OtaStepAction flashAction = _stateMachine.onFlashComplete(flashOk, flashErr);
    if (!flashOk || flashAction.errorCode != ErrorCode::NONE) {
        errorOut = flashAction.errorCode != ErrorCode::NONE ? flashAction.errorCode : ErrorCode::E012_OTA_FAILED;
        Serial.printf("[OTA] Flash failed: %s\n", flashAction.errorMessage);
        if (flashAction.clearPendingInNvs) {
            ConfigStore::clearPendingOta();
            config.pendingOtaCampaignId[0] = '\0';
            config.pendingOtaVersion[0] = '\0';
        }
        if (flashAction.reportStatus) {
            _apiClient->reportOtaStatus(
                flashAction.statusCampaignId,
                config.controllerId,
                flashAction.statusToReport,
                flashAction.errorMessage);
        }
        systemState = previousState;
        return false;
    }

    if (flashAction.rebootRequired) {
        Serial.println("[OTA] Flash verified and committed. Rebooting...");
        if (_displayManager != nullptr) {
            _displayManager->showBanner("OTA Done!", "Rebooting...");
        }
        delay(300);
        ESP.restart();
    }

    return true;
}

bool OtaManager::streamDownloadAndWrite(
    const char* downloadPath,
    char* outSha256Hex,
    size_t outSha256Size,
    char* outError,
    size_t outErrorSize) {
    if (outSha256Hex != nullptr && outSha256Size > 0) {
        outSha256Hex[0] = '\0';
    }
    if (outError != nullptr && outErrorSize > 0) {
        outError[0] = '\0';
    }

    HTTPClient http;
    const String url = _apiClient->buildUrl(downloadPath);
    if (!_apiClient->prepareRequest(http, url, 15000)) {
        copyCString(outError, outErrorSize, "E012 HTTP begin failed");
        return false;
    }

    const int statusCode = http.GET();
    if (statusCode != HTTP_CODE_OK) {
        snprintf(outError, outErrorSize, "E012 Download HTTP %d", statusCode);
        http.end();
        return false;
    }

    const int contentLength = http.getSize();
    const size_t updateSize = contentLength > 0 ? static_cast<size_t>(contentLength) : UPDATE_SIZE_UNKNOWN;
    if (!Update.begin(updateSize)) {
        snprintf(outError, outErrorSize, "E012 Update.begin err %u", static_cast<unsigned>(Update.getError()));
        http.end();
        return false;
    }

    mbedtls_sha256_context shaCtx;
    mbedtls_sha256_init(&shaCtx);
#if defined(MBEDTLS_VERSION_NUMBER) && MBEDTLS_VERSION_NUMBER >= 0x03000000
    mbedtls_sha256_starts(&shaCtx, 0);
#else
    mbedtls_sha256_starts_ret(&shaCtx, 0);
#endif

    WiFiClient* stream = http.getStreamPtr();
    uint8_t buffer[1024];
    size_t totalWritten = 0;
    int lastPercent = -1;
    uint32_t lastDataAtMs = millis();

    while ((http.connected() || stream->available() > 0) &&
           (contentLength < 0 || totalWritten < static_cast<size_t>(contentLength))) {
        const size_t available = stream->available();
        if (available == 0) {
            if (!http.connected()) {
                break;
            }
            if (millis() - lastDataAtMs > 15000) {
                copyCString(outError, outErrorSize, "E012 Download stream timeout");
                mbedtls_sha256_free(&shaCtx);
                http.end();
                return false;
            }
            delay(5);
            continue;
        }

        const size_t toRead = available > sizeof(buffer) ? sizeof(buffer) : available;
        const int bytesRead = stream->readBytes(buffer, toRead);
        if (bytesRead <= 0) {
            continue;
        }

        lastDataAtMs = millis();
#if defined(MBEDTLS_VERSION_NUMBER) && MBEDTLS_VERSION_NUMBER >= 0x03000000
        mbedtls_sha256_update(&shaCtx, buffer, static_cast<size_t>(bytesRead));
#else
        mbedtls_sha256_update_ret(&shaCtx, buffer, static_cast<size_t>(bytesRead));
#endif

        const size_t written = Update.write(buffer, static_cast<size_t>(bytesRead));
        if (written != static_cast<size_t>(bytesRead)) {
            snprintf(outError, outErrorSize, "E012 Flash write err %u", static_cast<unsigned>(Update.getError()));
            mbedtls_sha256_free(&shaCtx);
            http.end();
            return false;
        }

        totalWritten += written;
        if (contentLength > 0 && _displayManager != nullptr) {
            const int pct = static_cast<int>((totalWritten * 100UL) / static_cast<size_t>(contentLength));
            if (pct / 10 != lastPercent / 10) {
                lastPercent = pct;
                char progressLine[17];
                snprintf(progressLine, sizeof(progressLine), "Progress: %3d%%", pct);
                _displayManager->showBanner("OTA Downloading", progressLine);
            }
        }
        yield();
    }

    uint8_t digest[32] = {};
#if defined(MBEDTLS_VERSION_NUMBER) && MBEDTLS_VERSION_NUMBER >= 0x03000000
    mbedtls_sha256_finish(&shaCtx, digest);
#else
    mbedtls_sha256_finish_ret(&shaCtx, digest);
#endif
    mbedtls_sha256_free(&shaCtx);
    http.end();

    if (contentLength > 0 && totalWritten != static_cast<size_t>(contentLength)) {
        copyCString(outError, outErrorSize, "E012 Truncated firmware");
        return false;
    }

    formatSha256Hex(digest, outSha256Hex, outSha256Size);
    return true;
}
