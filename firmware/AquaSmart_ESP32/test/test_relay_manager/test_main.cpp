#include <unity.h>

#include <cstring>

#include <RelayController.h>

namespace {
struct MockGpio {
    bool pinHigh[40] = {};
    int writeCount[40] = {};
};

void mockGpioWrite(uint8_t pin, bool highLevel, void* ctx) {
    auto* gpio = static_cast<MockGpio*>(ctx);
    if (pin < 40) {
        gpio->pinHigh[pin] = highLevel;
        ++gpio->writeCount[pin];
    }
}

DeviceConfig makeDefaultDeviceConfig() {
    DeviceConfig cfg{};
    for (size_t i = 0; i < MAX_RELAY_COUNT; ++i) {
        cfg.relayActiveLow[i] = true;
        cfg.relaySafeState[i] = false;
    }
    return cfg;
}
}  // namespace

void setUp() {}
void tearDown() {}

void test_init_all_8_channels_to_safe_state_respecting_active_low() {
    DeviceConfig cfg = makeDefaultDeviceConfig();
    cfg.relaySafeState[0] = true;   // CH1 activeLow=true, safeState=ON -> pin LOW
    cfg.relaySafeState[1] = false;  // CH2 activeLow=true, safeState=OFF -> pin HIGH
    cfg.relayActiveLow[2] = false;  // CH3 activeLow=false, safeState=ON -> pin HIGH
    cfg.relaySafeState[2] = true;
    cfg.relayActiveLow[3] = false;  // CH4 activeLow=false, safeState=OFF -> pin LOW
    cfg.relaySafeState[3] = false;

    MockGpio gpio{};
    RelayController controller;
    controller.begin(cfg, &mockGpioWrite, &gpio);

    TEST_ASSERT_EQUAL_UINT32(8, controller.channelCount());
    TEST_ASSERT_TRUE(controller.getRelayState(1));
    TEST_ASSERT_FALSE(gpio.pinHigh[CORE_RELAY_PINS[0]]);  // LOW for activeLow ON

    TEST_ASSERT_FALSE(controller.getRelayState(2));
    TEST_ASSERT_TRUE(gpio.pinHigh[CORE_RELAY_PINS[1]]);   // HIGH for activeLow OFF

    TEST_ASSERT_TRUE(controller.getRelayState(3));
    TEST_ASSERT_TRUE(gpio.pinHigh[CORE_RELAY_PINS[2]]);   // HIGH for activeHigh ON

    TEST_ASSERT_FALSE(controller.getRelayState(4));
    TEST_ASSERT_FALSE(gpio.pinHigh[CORE_RELAY_PINS[3]]);  // LOW for activeHigh OFF

    // Before server config: channels are unconfigured (CH1..CH8 unnamed)
    TEST_ASSERT_FALSE(controller.getChannel(1)->configured);
}

void test_apply_server_config_binds_by_connection_address() {
    DeviceConfig devCfg = makeDefaultDeviceConfig();
    MockGpio gpio{};
    RelayController controller;
    controller.begin(devCfg, &mockGpioWrite, &gpio);

    BackendRuntimeConfig serverCfg{};
    serverCfg.isLoaded = true;
    serverCfg.relayCount = 2;

    // Bind by GPIO pin number ("18" -> CH3)
    copyCString(serverCfg.relays[0].relayId, sizeof(serverCfg.relays[0].relayId), "r-filter");
    copyCString(serverCfg.relays[0].name, sizeof(serverCfg.relays[0].name), "Filter");
    copyCString(serverCfg.relays[0].connectionAddress, sizeof(serverCfg.relays[0].connectionAddress), "18");
    serverCfg.relays[0].purpose = RelayPurpose::FILTER;
    serverCfg.relays[0].backendState = true;
    serverCfg.relays[0].isManual = false;

    // Bind by channel label ("CH6" -> CH6)
    copyCString(serverCfg.relays[1].relayId, sizeof(serverCfg.relays[1].relayId), "r-heater");
    copyCString(serverCfg.relays[1].name, sizeof(serverCfg.relays[1].name), "Heater");
    copyCString(serverCfg.relays[1].connectionAddress, sizeof(serverCfg.relays[1].connectionAddress), "CH6");
    serverCfg.relays[1].purpose = RelayPurpose::HEATING;
    serverCfg.relays[1].backendState = false;
    serverCfg.relays[1].isManual = true;

    controller.applyServerConfig(serverCfg, devCfg);

    const RelayRuntimeConfig* ch3 = controller.getChannel(3);
    TEST_ASSERT_TRUE(ch3->configured);
    TEST_ASSERT_EQUAL_STRING("r-filter", ch3->relayId);
    TEST_ASSERT_EQUAL_STRING("Filter", ch3->name);
    TEST_ASSERT_TRUE(ch3->currentState);
    TEST_ASSERT_FALSE(ch3->isManual);
    TEST_ASSERT_FALSE(gpio.pinHigh[CORE_RELAY_PINS[2]]);  // activeLow ON -> LOW

    const RelayRuntimeConfig* ch6 = controller.getChannel(6);
    TEST_ASSERT_TRUE(ch6->configured);
    TEST_ASSERT_EQUAL_STRING("r-heater", ch6->relayId);
    TEST_ASSERT_EQUAL_STRING("Heater", ch6->name);
    TEST_ASSERT_FALSE(ch6->currentState);
    TEST_ASSERT_TRUE(ch6->isManual);
}

void test_manual_toggle_sets_is_manual_and_pending_sync_offline() {
    DeviceConfig devCfg = makeDefaultDeviceConfig();
    MockGpio gpio{};
    RelayController controller;
    controller.begin(devCfg, &mockGpioWrite, &gpio);

    BackendRuntimeConfig serverCfg{};
    serverCfg.isLoaded = true;
    serverCfg.relayCount = 1;
    copyCString(serverCfg.relays[0].relayId, sizeof(serverCfg.relays[0].relayId), "r-1");
    copyCString(serverCfg.relays[0].name, sizeof(serverCfg.relays[0].name), "Light");
    copyCString(serverCfg.relays[0].connectionAddress, sizeof(serverCfg.relays[0].connectionAddress), "1");
    serverCfg.relays[0].backendState = false;
    serverCfg.relays[0].isManual = false;
    controller.applyServerConfig(serverCfg, devCfg);

    TEST_ASSERT_FALSE(controller.getRelayState(1));
    TEST_ASSERT_FALSE(controller.getRelayIsManual(1));

    // Offline manual toggle -> flips to ON, isManual=true, pendingSync=true
    TEST_ASSERT_TRUE(controller.manualToggle(1, false));
    TEST_ASSERT_TRUE(controller.getRelayState(1));
    TEST_ASSERT_TRUE(controller.getRelayIsManual(1));
    TEST_ASSERT_TRUE(controller.getChannel(1)->pendingSync);
    TEST_ASSERT_TRUE(controller.hasAnyPendingSync());
    TEST_ASSERT_FALSE(gpio.pinHigh[CORE_RELAY_PINS[0]]);
}

void test_manual_toggle_mode_switches_man_auto_without_changing_physical_contact() {
    DeviceConfig devCfg = makeDefaultDeviceConfig();
    MockGpio gpio{};
    RelayController controller;
    controller.begin(devCfg, &mockGpioWrite, &gpio);

    TEST_ASSERT_TRUE(controller.manualToggle(2, true));  // ON, MAN
    const bool pinLevelBefore = gpio.pinHigh[CORE_RELAY_PINS[1]];
    const int writesBefore = gpio.writeCount[CORE_RELAY_PINS[1]];

    // Toggle MAN -> AUTO offline: physical contact MUST NOT change
    TEST_ASSERT_TRUE(controller.manualToggleMode(2, false));
    TEST_ASSERT_TRUE(controller.getRelayState(2));
    TEST_ASSERT_FALSE(controller.getRelayIsManual(2));
    TEST_ASSERT_TRUE(controller.getChannel(2)->pendingSync);
    TEST_ASSERT_EQUAL(pinLevelBefore, gpio.pinHigh[CORE_RELAY_PINS[1]]);
    TEST_ASSERT_EQUAL_INT(writesBefore, gpio.writeCount[CORE_RELAY_PINS[1]]);

    // Toggle AUTO -> MAN
    TEST_ASSERT_TRUE(controller.manualToggleMode(2, true));
    TEST_ASSERT_TRUE(controller.getRelayIsManual(2));
    TEST_ASSERT_TRUE(controller.getRelayState(2));
}

void test_server_command_apply_deduplication_expiration_and_unknown_relay() {
    DeviceConfig devCfg = makeDefaultDeviceConfig();
    MockGpio gpio{};
    RelayController controller;
    controller.begin(devCfg, &mockGpioWrite, &gpio);

    BackendRuntimeConfig serverCfg{};
    serverCfg.isLoaded = true;
    serverCfg.relayCount = 1;
    copyCString(serverCfg.relays[0].relayId, sizeof(serverCfg.relays[0].relayId), "b1a4c2b0-1111-2222-3333-444444444444");
    copyCString(serverCfg.relays[0].name, sizeof(serverCfg.relays[0].name), "Pump");
    copyCString(serverCfg.relays[0].connectionAddress, sizeof(serverCfg.relays[0].connectionAddress), "16");
    serverCfg.relays[0].backendState = false;
    serverCfg.relays[0].isManual = false;
    controller.applyServerConfig(serverCfg, devCfg);

    char err[64] = {};
    // 1. Valid command -> APPLIED
    TEST_ASSERT_EQUAL_INT(
        static_cast<int>(CommandApplyStatus::APPLIED),
        static_cast<int>(controller.applyServerCommand("cmd-1", "b1a4c2b0-1111-2222-3333-444444444444", true, 2000, 1500, err, sizeof(err))));
    TEST_ASSERT_TRUE(controller.getRelayState(1));

    // 2. Duplicate commandId -> DUPLICATE (does not toggle back)
    TEST_ASSERT_EQUAL_INT(
        static_cast<int>(CommandApplyStatus::DUPLICATE),
        static_cast<int>(controller.applyServerCommand("cmd-1", "b1a4c2b0-1111-2222-3333-444444444444", false, 2000, 1500, err, sizeof(err))));
    TEST_ASSERT_TRUE(controller.getRelayState(1));

    // 3. Expired command -> EXPIRED
    TEST_ASSERT_EQUAL_INT(
        static_cast<int>(CommandApplyStatus::EXPIRED),
        static_cast<int>(controller.applyServerCommand("cmd-2", "b1a4c2b0-1111-2222-3333-444444444444", false, 1400, 1500, err, sizeof(err))));
    TEST_ASSERT_TRUE(controller.getRelayState(1));
    TEST_ASSERT_TRUE(std::strlen(err) > 0);

    // 4. Unknown relayId -> UNKNOWN_RELAY
    TEST_ASSERT_EQUAL_INT(
        static_cast<int>(CommandApplyStatus::UNKNOWN_RELAY),
        static_cast<int>(controller.applyServerCommand("cmd-3", "00000000-0000-0000-0000-000000000099", false, 2000, 1500, err, sizeof(err))));
    TEST_ASSERT_TRUE(std::strlen(err) > 0);
}

void test_safe_state_all_resets_all_8_channels_and_marks_pending_sync() {
    DeviceConfig devCfg = makeDefaultDeviceConfig();
    devCfg.relaySafeState[0] = true;   // CH1 safe = ON
    devCfg.relaySafeState[1] = false;  // CH2 safe = OFF

    MockGpio gpio{};
    RelayController controller;
    controller.begin(devCfg, &mockGpioWrite, &gpio);

    controller.manualToggle(1, true);  // CH1 -> OFF
    controller.manualToggle(2, true);  // CH2 -> ON
    controller.manualToggle(8, true);  // CH8 -> ON
    controller.markAllSynced();
    TEST_ASSERT_FALSE(controller.hasAnyPendingSync());

    controller.safeStateAll();

    TEST_ASSERT_TRUE(controller.getRelayState(1));
    TEST_ASSERT_FALSE(controller.getRelayState(2));
    TEST_ASSERT_FALSE(controller.getRelayState(8));
    for (uint8_t ch = 1; ch <= MAX_RELAY_COUNT; ++ch) {
        TEST_ASSERT_TRUE(controller.getChannel(ch)->pendingSync);
    }
}

int main(int, char**) {
    UNITY_BEGIN();
    RUN_TEST(test_init_all_8_channels_to_safe_state_respecting_active_low);
    RUN_TEST(test_apply_server_config_binds_by_connection_address);
    RUN_TEST(test_manual_toggle_sets_is_manual_and_pending_sync_offline);
    RUN_TEST(test_manual_toggle_mode_switches_man_auto_without_changing_physical_contact);
    RUN_TEST(test_server_command_apply_deduplication_expiration_and_unknown_relay);
    RUN_TEST(test_safe_state_all_resets_all_8_channels_and_marks_pending_sync);
    return UNITY_END();
}
