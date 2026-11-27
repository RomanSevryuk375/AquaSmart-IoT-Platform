#include <unity.h>

#include <cstring>

#include <RelayController.h>
#include <UiController.h>

void setUp() {}
void tearDown() {}

void test_two_level_navigation_and_clean_back_from_every_section() {
    UiController ui;
    ui.begin();
    TEST_ASSERT_EQUAL_INT(static_cast<int>(UiLevel::DASHBOARD), static_cast<int>(ui.level()));

    // Dashboard -> Root Menu via SELECT
    ui.handleInput({InputEventType::SELECT, false}, 0, 0);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(UiLevel::ROOT_MENU), static_cast<int>(ui.level()));
    TEST_ASSERT_EQUAL_UINT32(0, ui.rootIndex());

    // Enter all 6 sections and verify BACK always returns cleanly to Root Menu
    for (size_t sec = 0; sec < ROOT_MENU_ITEM_COUNT; ++sec) {
        ui.handleInput({InputEventType::SELECT, false}, 2, 1);
        TEST_ASSERT_EQUAL_INT(static_cast<int>(UiLevel::SUBMENU), static_cast<int>(ui.level()));
        TEST_ASSERT_EQUAL_UINT32(sec, static_cast<size_t>(ui.selectedSection()));

        ui.handleInput({InputEventType::BACK, false}, 2, 1);
        TEST_ASSERT_EQUAL_INT(static_cast<int>(UiLevel::ROOT_MENU), static_cast<int>(ui.level()));

        ui.handleInput({InputEventType::NEXT, false}, 2, 1);
    }

    // Wrapped back to section 0; BACK from Root Menu -> Dashboard
    TEST_ASSERT_EQUAL_UINT32(0, ui.rootIndex());
    ui.handleInput({InputEventType::BACK, false}, 2, 1);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(UiLevel::DASHBOARD), static_cast<int>(ui.level()));
}

void test_empty_config_shows_no_sensors_and_8_unnamed_relay_channels() {
    DeviceConfig devCfg{};
    RelayController relays;
    relays.begin(devCfg);

    BackendRuntimeConfig emptyCfg{};
    emptyCfg.isLoaded = false;

    LcdViewContext ctx{};
    ctx.runtimeConfig = &emptyCfg;
    ctx.relays = relays.getChannelByIndex(0);

    UiController ui;
    ui.begin();
    ui.handleInput({InputEventType::SELECT, false}, 0, 0);  // Root Menu (Sensors)
    ui.handleInput({InputEventType::SELECT, false}, 0, 0);  // 1. Sensors

    LcdLines lines{};
    LcdViewFormatter::format(ui, ctx, lines);
    TEST_ASSERT_NOT_NULL(std::strstr(lines.line0, "No sensors cfg"));

    // Go to 2. Relays
    ui.handleInput({InputEventType::BACK, false}, 0, 0);
    ui.handleInput({InputEventType::NEXT, false}, 0, 0);    // 2. Relays
    ui.handleInput({InputEventType::SELECT, false}, 0, 0);  // Enter Relays

    LcdViewFormatter::format(ui, ctx, lines);
    TEST_ASSERT_NOT_NULL(std::strstr(lines.line0, "[1/8] CH1"));
    TEST_ASSERT_NOT_NULL(std::strstr(lines.line1, "State:OFF [MAN]"));

    // Navigate to CH8 and toggle
    ui.handleInput({InputEventType::PREVIOUS, false}, 0, 0);
    TEST_ASSERT_EQUAL_UINT32(7, ui.selectedRelayIndex());
    LcdViewFormatter::format(ui, ctx, lines);
    TEST_ASSERT_NOT_NULL(std::strstr(lines.line0, "[8/8] CH8"));

    UiAction act = ui.handleInput({InputEventType::SELECT, false}, 0, 0);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(UiActionType::TOGGLE_RELAY_STATE), static_cast<int>(act.type));
    TEST_ASSERT_EQUAL_UINT8(8, act.relayChannel);
}

void test_loaded_config_displays_sensor_items_and_named_relays() {
    DeviceConfig devCfg{};
    BackendRuntimeConfig cfg{};
    cfg.isLoaded = true;
    cfg.sensorCount = 2;
    copyCString(cfg.sensors[0].name, sizeof(cfg.sensors[0].name), "Water Temp");
    copyCString(cfg.sensors[0].unit, sizeof(cfg.sensors[0].unit), "C");
    cfg.sensors[0].hasValue = true;
    cfg.sensors[0].lastValue = 25.4f;
    copyCString(cfg.sensors[1].name, sizeof(cfg.sensors[1].name), "Air Humid");
    copyCString(cfg.sensors[1].unit, sizeof(cfg.sensors[1].unit), "%");
    cfg.sensors[1].hasValue = true;
    cfg.sensors[1].lastValue = 62.0f;

    cfg.relayCount = 1;
    copyCString(cfg.relays[0].relayId, sizeof(cfg.relays[0].relayId), "r-1");
    copyCString(cfg.relays[0].name, sizeof(cfg.relays[0].name), "Filter");
    copyCString(cfg.relays[0].connectionAddress, sizeof(cfg.relays[0].connectionAddress), "1");
    cfg.relays[0].backendState = true;
    cfg.relays[0].isManual = false;

    RelayController relays;
    relays.begin(devCfg);
    relays.applyServerConfig(cfg, devCfg);

    LcdViewContext ctx{};
    ctx.runtimeConfig = &cfg;
    ctx.relays = relays.getChannelByIndex(0);

    UiController ui;
    ui.begin();
    ui.handleInput({InputEventType::SELECT, false}, cfg.sensorCount, 0);
    ui.handleInput({InputEventType::SELECT, false}, cfg.sensorCount, 0);  // Sensors [1/2]

    LcdLines lines{};
    LcdViewFormatter::format(ui, ctx, lines);
    TEST_ASSERT_NOT_NULL(std::strstr(lines.line0, "[1/2] Water Temp"));
    TEST_ASSERT_NOT_NULL(std::strstr(lines.line1, "25.4 C"));

    ui.handleInput({InputEventType::NEXT, false}, cfg.sensorCount, 0);  // Sensors [2/2]
    LcdViewFormatter::format(ui, ctx, lines);
    TEST_ASSERT_NOT_NULL(std::strstr(lines.line0, "[2/2] Air Humid"));

    // Relays [1/8] CH1 Filter
    ui.handleInput({InputEventType::BACK, false}, cfg.sensorCount, 0);
    ui.handleInput({InputEventType::NEXT, false}, cfg.sensorCount, 0);
    ui.handleInput({InputEventType::SELECT, false}, cfg.sensorCount, 0);
    LcdViewFormatter::format(ui, ctx, lines);
    TEST_ASSERT_NOT_NULL(std::strstr(lines.line0, "[1/8] CH1 Filter"));
    TEST_ASSERT_NOT_NULL(std::strstr(lines.line1, "State:ON  [AUTO]"));
}

void test_select_short_fires_on_release_and_long_hold_does_not_fire_short() {
    ButtonHoldDetector detector(SELECT_LONG_PRESS_MS);

    // Case 1: Short press (press at t=100, release at t=400 -> short fires on release!)
    TEST_ASSERT_EQUAL_UINT8(0, detector.update(true, 100));
    TEST_ASSERT_EQUAL_UINT8(0, detector.update(true, 350));
    TEST_ASSERT_EQUAL_UINT8(1, detector.update(false, 400));

    // Case 2: Long press (press at t=1000, held past 2200 -> long fires at 2200, NOTHING on release at 2500!)
    TEST_ASSERT_EQUAL_UINT8(0, detector.update(true, 1000));
    TEST_ASSERT_EQUAL_UINT8(0, detector.update(true, 2199));
    TEST_ASSERT_EQUAL_UINT8(2, detector.update(true, 2200));
    TEST_ASSERT_EQUAL_UINT8(0, detector.update(true, 2400));
    TEST_ASSERT_EQUAL_UINT8(0, detector.update(false, 2500));

    // Verify long press in Relays submenu produces TOGGLE_RELAY_MODE (not TOGGLE_RELAY_STATE)
    UiController ui;
    ui.begin();
    ui.handleInput({InputEventType::SELECT, false}, 0, 0);
    ui.handleInput({InputEventType::NEXT, false}, 0, 0);
    ui.handleInput({InputEventType::SELECT, false}, 0, 0);

    UiAction longAct = ui.handleInput({InputEventType::SELECT, true}, 0, 0);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(UiActionType::TOGGLE_RELAY_MODE), static_cast<int>(longAct.type));
    TEST_ASSERT_EQUAL_UINT8(1, longAct.relayChannel);
}

void test_factory_reset_only_triggers_on_dedicated_screen_after_5s_back_hold() {
    ButtonHoldDetector backDetector(BACK_FACTORY_RESET_HOLD_MS);
    TEST_ASSERT_EQUAL_UINT8(0, backDetector.update(true, 0));
    TEST_ASSERT_EQUAL_UINT8(0, backDetector.update(true, 4999));
    TEST_ASSERT_EQUAL_UINT8(2, backDetector.update(true, 5000));
    TEST_ASSERT_EQUAL_UINT8(0, backDetector.update(false, 5200));

    UiController ui;
    ui.begin();

    // Dashboard: FACTORY_RESET event must be ignored!
    UiAction ignored1 = ui.handleInput({InputEventType::FACTORY_RESET, true}, 0, 0);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(UiActionType::NONE), static_cast<int>(ignored1.type));

    // Navigate to 6. System -> OTA Check: still ignored!
    ui.handleInput({InputEventType::SELECT, false}, 0, 0);   // Root Menu
    ui.handleInput({InputEventType::PREVIOUS, false}, 0, 0); // 6. System
    ui.handleInput({InputEventType::SELECT, false}, 0, 0);   // Enter System (index 0: OTA Check)
    UiAction ignored2 = ui.handleInput({InputEventType::FACTORY_RESET, true}, 0, 0);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(UiActionType::NONE), static_cast<int>(ignored2.type));

    // Navigate to index 3: Factory Reset
    ui.handleInput({InputEventType::NEXT, false}, 0, 0);  // Safe State All
    ui.handleInput({InputEventType::NEXT, false}, 0, 0);  // Reboot
    ui.handleInput({InputEventType::NEXT, false}, 0, 0);  // Factory Reset
    TEST_ASSERT_TRUE(ui.isOnFactoryResetScreen());

    // Short SELECT does NOT trigger Factory Reset
    UiAction shortSelect = ui.handleInput({InputEventType::SELECT, false}, 0, 0);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(UiActionType::NONE), static_cast<int>(shortSelect.type));

    // Hold BACK >= 5s (InputEventType::FACTORY_RESET) triggers FACTORY_RESET!
    UiAction resetAct = ui.handleInput({InputEventType::FACTORY_RESET, true}, 0, 0);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(UiActionType::FACTORY_RESET), static_cast<int>(resetAct.type));
}

int main(int, char**) {
    UNITY_BEGIN();
    RUN_TEST(test_two_level_navigation_and_clean_back_from_every_section);
    RUN_TEST(test_empty_config_shows_no_sensors_and_8_unnamed_relay_channels);
    RUN_TEST(test_loaded_config_displays_sensor_items_and_named_relays);
    RUN_TEST(test_select_short_fires_on_release_and_long_hold_does_not_fire_short);
    RUN_TEST(test_factory_reset_only_triggers_on_dedicated_screen_after_5s_back_hold);
    return UNITY_END();
}
