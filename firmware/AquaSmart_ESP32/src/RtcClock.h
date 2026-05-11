#ifndef RTC_CLOCK_H
#define RTC_CLOCK_H

#include <RTClib.h>
#include "AppState.h"

class RtcClock {
public:
    bool begin();
    bool update(ErrorCode& errorCode, bool wifiConnected);
    bool isTimeValid() const;
    bool isRtcAvailable() const;
    String getIsoTimestampUtc() const;
    String getDisplayTime() const;

private:
    RTC_DS1307 _rtc;
    bool _rtcAvailable = false;
    bool _timeValid = false;
    bool _ntpRequestStarted = false;
    uint32_t _lastSyncAtMs = 0;
    uint32_t _ntpRequestAtMs = 0;

    bool syncFromNtp();
    bool syncFromRtc();
    bool isDateValid(const DateTime& dt) const;
};

#endif
