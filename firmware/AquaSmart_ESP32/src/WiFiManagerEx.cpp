#include "WiFiManagerEx.h"

void WiFiManagerEx::begin(const DeviceConfig& config) {
    _ssid = config.wifiSsid;
    _password = config.wifiPass;
    WiFi.mode(WIFI_STA);
    WiFi.setAutoReconnect(true);
    WiFi.persistent(true);

    if (_ssid.length() > 0) {
        Serial.printf("[WiFi] Initiating connection to %s...\n", _ssid.c_str());
        WiFi.begin(_ssid.c_str(), _password.c_str());
        _connectStarted = true;
    } else {
        Serial.println("[WiFi] No SSID configured.");
    }

    _connectStartedAtMs = millis();
    _lastLogAtMs = millis();
    _lastReconnectAtMs = millis();
}

bool WiFiManagerEx::update(SystemState& state, ErrorCode& errorCode) {
    if (WiFi.status() == WL_CONNECTED) {
        if (!_hasEverConnected) {
            Serial.println("[WiFi] Connected!");
            Serial.print("[WiFi] IP: ");
            Serial.println(WiFi.localIP());
            Serial.print("[WiFi] MAC: ");
            Serial.println(WiFi.macAddress());
            _hasEverConnected = true;
        }
        if (state == SystemState::CONNECT_WIFI) {
            state = SystemState::SYNC_TIME;
        }
        return true;
    }

    const uint32_t nowMs = millis();

    // Log status every 5 seconds when disconnected
    if (nowMs - _lastLogAtMs >= 5000) {
        Serial.printf("[WiFi] Status: %d, SSID: %s\n", WiFi.status(), _ssid.c_str());
        _lastLogAtMs = nowMs;
    }

    // Safety fallback: if not connected after 45s, re-trigger WiFi.reconnect()
    if (_connectStarted && nowMs - _lastReconnectAtMs >= 45000) {
        _lastReconnectAtMs = nowMs;
        Serial.println("[WiFi] Re-triggering connection...");
        WiFi.reconnect();
    }

    // Degradation timeout: allow offline operation if WiFi takes longer than 30s
    if (_connectStarted && nowMs - _connectStartedAtMs >= 30000 && WiFi.status() != WL_CONNECTED) {
        if (state == SystemState::CONNECT_WIFI) {
            Serial.println("[WiFi] Connection timeout, transitioning to DEGRADED_OFFLINE");
            state = SystemState::DEGRADED_OFFLINE;
            errorCode = ErrorCode::E002_WIFI_FAILED;
        }
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
