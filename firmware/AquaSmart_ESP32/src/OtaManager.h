#ifndef OTA_MANAGER_H
#define OTA_MANAGER_H

#include <OtaStateMachine.h>
#include "ApiClient.h"
#include "AppState.h"
#include "DisplayManager.h"
#include "RelayManager.h"

class OtaManager {
public:
    void begin(ApiClient* apiClient, RelayManager* relayManager, DisplayManager* displayManager);

    bool handlePostBootConfirmation(DeviceConfig& config, ErrorCode& errorOut);
    bool checkAndPerformUpdate(DeviceConfig& config, SystemState& systemState, ErrorCode& errorOut);

    bool isPostBootConfirmed() const { return _postBootConfirmed; }

private:
    ApiClient* _apiClient = nullptr;
    RelayManager* _relayManager = nullptr;
    DisplayManager* _displayManager = nullptr;
    OtaStateMachine _stateMachine;
    bool _postBootConfirmed = false;
    uint8_t _postBootRetryCount = 0;

    bool streamDownloadAndWrite(
        const char* downloadPath,
        char* outSha256Hex,
        size_t outSha256Size,
        char* outError,
        size_t outErrorSize);
};

#endif
