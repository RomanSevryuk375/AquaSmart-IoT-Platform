#include "WiFiManagerEx.h"

void WiFiManagerEx::begin(const DeviceConfig& config) {
    _ssid = config.wifiSsid;
    _password = config.wifiPass;
    WiFi.mode(WIFI_STA);
    WiFi.setAutoReconnect(true);
}

bool WiFiManagerEx::update(SystemState& state, ErrorCode& errorCode) {
    if (WiFi.status() == WL_CONNECTED) {
        if (!_hasEverConnected) {
            Serial.println("[WiFi] Connected!");
            Serial.print("[WiFi] IP: ");
            Serial.println(WiFi.localIP());
            _hasEverConnected = true;
        }
        state = SystemState::SYNC_TIME;
        return true;
    }

    const uint32_t nowMs = millis();
    
    // Если статус не WL_CONNECTED, выводим статус каждые 5 секунд
    if (nowMs - _lastAttemptAtMs >= 5000) {
        Serial.printf("[WiFi] Status: %d, SSID: %s\n", WiFi.status(), _ssid.c_str());
        _lastAttemptAtMs = nowMs;
    }

    if (!_connectStarted || nowMs - _lastAttemptAtMs >= 10000) {
        _lastAttemptAtMs = nowMs;
        if (!_connectStarted) {
            Serial.println("[WiFi] Starting connection...");
            _connectStartedAtMs = nowMs;
        }
        _connectStarted = true;
        WiFi.disconnect(false, false);
        WiFi.begin(_ssid.c_str(), _password.c_str());
    }

    if (_connectStarted && nowMs - _connectStartedAtMs >= 30000 && WiFi.status() != WL_CONNECTED) {
        Serial.println("[WiFi] Connection timeout!");
        state = SystemState::DEGRADED_OFFLINE;
        errorCode = ErrorCode::E002_WIFI_FAILED;
    }

    return false;
}

bool WiFiManagerEx::isConnected() const {
    return WiFi.status() == WL_CONNECTED;
}

bool WiFiManagerEx::hasEverConnected() const {
    return _hasEverConnected;
}

IPAddress WiFiManagerEx::localIp() const {
    return WiFi.localIP();
}

String WiFiManagerEx::macAddress() const {
    return WiFi.macAddress();
}
