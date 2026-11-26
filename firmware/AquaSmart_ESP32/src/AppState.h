#ifndef APP_STATE_H
#define APP_STATE_H

#include <Arduino.h>
#include <CoreTypes.h>

struct PendingCommand {
    String id;
    String controllerId;
    String relayId;
    bool targetState = false;
    int action = 0;
    int status = 0;
    int attemptCount = 0;
    String expireAt;
    String processedAt;
    String errorMessage;
    String createdAt;
};

struct ApiTelemetryResponse {
    int acceptedCount = 0;
    int skippedCount = 0;
    String validationErrors[MAX_VALIDATION_ERRORS];
    size_t validationErrorCount = 0;
};

struct ApiResult {
    bool success;
    int statusCode;
    String message;
    ApiResult(bool s, int c, String m) : success(s), statusCode(c), message(std::move(m)) {}
};

#endif
