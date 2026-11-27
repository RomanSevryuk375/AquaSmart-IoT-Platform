#include "RtcClock.h"

#include <time.h>

bool RtcClock::begin() {
    _rtcAvailable = _rtc.begin();
    return _rtcAvailable;
}

bool RtcClock::update(ErrorCode& errorCode, bool wifiConnected) {
    const uint32_t nowMs = millis();

    if (wifiConnected && (nowMs - _lastSyncAtMs >= 60000 || !_timeValid)) {
        if (syncFromNtp()) {
            _lastSyncAtMs = nowMs;
            _timeValid = true;
            _syncedFromNtp = true;
            return true;
        }
    }

    if (!_timeValid && syncFromRtc()) {
        _timeValid = true;
        _syncedFromNtp = false;
        return true;
    }

    if (!_timeValid) {
        errorCode = _rtcAvailable ? ErrorCode::E004_TIME_INVALID : ErrorCode::E003_RTC_UNAVAILABLE;
    }

    return _timeValid;
}

bool RtcClock::syncFromServerEpoch(int64_t epochSec) {
    if (epochSec <= 1704067200LL) {  // 2024-01-01T00:00:00Z
        return false;
    }

    timeval tv{};
    tv.tv_sec = static_cast<time_t>(epochSec);
    tv.tv_usec = 0;
    settimeofday(&tv, nullptr);

    if (_rtcAvailable) {
        _rtc.adjust(DateTime(static_cast<uint32_t>(epochSec)));
    }

    _timeValid = true;
    _lastSyncAtMs = millis();
    return true;
}

bool RtcClock::isTimeValid() const {
    return _timeValid;
}

bool RtcClock::isRtcAvailable() const {
    return _rtcAvailable;
}

bool RtcClock::isSyncedFromNtp() const {
    return _syncedFromNtp;
}

void RtcClock::getShortTime(char* out, size_t outSize) const {
    if (!_timeValid) {
        snprintf(out, outSize, "--:--");
        return;
    }
    time_t now = time(nullptr);
    struct tm utcTime {};
    gmtime_r(&now, &utcTime);
    snprintf(out, outSize, "%02d:%02d", utcTime.tm_hour, utcTime.tm_min);
}

void RtcClock::getFullDateTime(char* out, size_t outSize) const {
    if (!_timeValid) {
        snprintf(out, outSize, "---- -- -- --:--");
        return;
    }
    time_t now = time(nullptr);
    struct tm utcTime {};
    gmtime_r(&now, &utcTime);
    snprintf(out, outSize, "%04d-%02d-%02d %02d:%02d",
             utcTime.tm_year + 1900,
             utcTime.tm_mon + 1,
             utcTime.tm_mday,
             utcTime.tm_hour,
             utcTime.tm_min);
}

String RtcClock::getIsoTimestampUtc() const {
    time_t now = time(nullptr);
    struct tm utcTime {};
    gmtime_r(&now, &utcTime);

    char buffer[25];
    snprintf(buffer,
             sizeof(buffer),
             "%04d-%02d-%02dT%02d:%02d:%02dZ",
             utcTime.tm_year + 1900,
             utcTime.tm_mon + 1,
             utcTime.tm_mday,
             utcTime.tm_hour,
             utcTime.tm_min,
             utcTime.tm_sec);
    return String(buffer);
}

String RtcClock::getDisplayTime() const {
    time_t now = time(nullptr);
    struct tm utcTime {};
    gmtime_r(&now, &utcTime);

    char buffer[17];
    snprintf(buffer,
             sizeof(buffer),
             "%02d:%02d:%02d UTC",
             utcTime.tm_hour,
             utcTime.tm_min,
             utcTime.tm_sec);
    return String(buffer);
}

bool RtcClock::syncFromNtp() {
    if (!_ntpRequestStarted) {
        configTime(0, 0, "pool.ntp.org", "time.nist.gov");
        _ntpRequestStarted = true;
        _ntpRequestAtMs = millis();
    }

    const time_t now = time(nullptr);
    if (now > 1735689600) {
        _ntpRequestStarted = false;
        if (_rtcAvailable) {
            _rtc.adjust(DateTime(static_cast<uint32_t>(now)));
        }
        return true;
    }

    if (_ntpRequestStarted && millis() - _ntpRequestAtMs >= 5000) {
        _ntpRequestStarted = false;
    }

    return false;
}

bool RtcClock::syncFromRtc() {
    if (!_rtcAvailable) {
        return false;
    }

    const DateTime now = _rtc.now();
    if (!isDateValid(now)) {
        return false;
    }

    timeval tv {};
    tv.tv_sec = static_cast<time_t>(now.unixtime());
    tv.tv_usec = 0;
    settimeofday(&tv, nullptr);
    return true;
}

bool RtcClock::isDateValid(const DateTime& dt) const {
    return dt.year() >= 2024 && dt.year() < 2100;
}
