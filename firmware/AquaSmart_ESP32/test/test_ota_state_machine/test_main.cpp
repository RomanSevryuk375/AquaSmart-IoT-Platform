#include <unity.h>

#include <cstring>

#include <OtaStateMachine.h>

namespace {
constexpr char VALID_SHA[] = "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef";
constexpr char BAD_SHA[]   = "ffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffffff";

OtaCheckInfo makeCheck(bool hasUpdate, const char* version) {
    OtaCheckInfo info{};
    info.hasUpdate = hasUpdate;
    copyCString(info.campaignId, sizeof(info.campaignId), "camp-1111-2222-3333-444444444444");
    copyCString(info.version, sizeof(info.version), version);
    copyCString(info.downloadUrl, sizeof(info.downloadUrl), "/api/firmware/v1/ota/download/camp-1111?controller_id=ctrl-1");
    copyCString(info.fileHash, sizeof(info.fileHash), VALID_SHA);
    return info;
}
}  // namespace

void setUp() {}
void tearDown() {}

void test_check_same_version_or_no_update_is_noop() {
    OtaStateMachine fsm;

    OtaStepAction noUpd = fsm.evaluateCheckResponse(makeCheck(false, "1.1.0"), "1.0.0");
    TEST_ASSERT_FALSE(noUpd.reportStatus);
    TEST_ASSERT_FALSE(noUpd.savePendingInNvs);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(OtaPhase::IDLE), static_cast<int>(fsm.phase()));

    OtaStepAction sameVer = fsm.evaluateCheckResponse(makeCheck(true, "1.0.0"), "1.0.0");
    TEST_ASSERT_FALSE(sameVer.reportStatus);
    TEST_ASSERT_FALSE(sameVer.savePendingInNvs);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(OtaPhase::IDLE), static_cast<int>(fsm.phase()));
}

void test_happy_path_downloading_flashing_reboot_and_post_boot_success() {
    OtaStateMachine fsm;

    // 1. Check finds newer version -> Downloading (1), save pending NVS, enter safeState
    OtaStepAction step1 = fsm.evaluateCheckResponse(makeCheck(true, "1.1.0"), "1.0.0");
    TEST_ASSERT_TRUE(step1.reportStatus);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(OtaTargetStatus::DOWNLOADING), static_cast<int>(step1.statusToReport));
    TEST_ASSERT_TRUE(step1.savePendingInNvs);
    TEST_ASSERT_TRUE(step1.enterSafeState);

    // 2. Download completes with matching SHA-256 (case-insensitive) -> Flashing (2)
    OtaStepAction step2 = fsm.onDownloadComplete(true, "0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF0123456789ABCDEF");
    TEST_ASSERT_TRUE(step2.reportStatus);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(OtaTargetStatus::FLASHING), static_cast<int>(step2.statusToReport));
    TEST_ASSERT_FALSE(step2.rebootRequired);

    // 3. Flash finishes -> Reboot required
    OtaStepAction step3 = fsm.onFlashComplete(true);
    TEST_ASSERT_TRUE(step3.rebootRequired);
    TEST_ASSERT_FALSE(step3.clearPendingInNvs);

    // 4. Post-boot with runningFwVersion == pendingOtaVersion -> markAppValid + Success (3) + clear NVS
    OtaStateMachine postBootFsm;
    OtaStepAction step4 = postBootFsm.evaluatePostBoot(true, "camp-1111-2222-3333-444444444444", "1.1.0", "1.1.0");
    TEST_ASSERT_TRUE(step4.markAppValidCancelRollback);
    TEST_ASSERT_TRUE(step4.reportStatus);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(OtaTargetStatus::SUCCESS), static_cast<int>(step4.statusToReport));
    TEST_ASSERT_TRUE(step4.clearPendingInNvs);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(ErrorCode::NONE), static_cast<int>(step4.errorCode));
}

void test_post_boot_automatic_rollback_reports_failed_and_clears_nvs() {
    OtaStateMachine fsm;

    // Bootloader rolled back to 1.0.0 instead of pending 1.1.0
    OtaStepAction step = fsm.evaluatePostBoot(true, "camp-1111-2222-3333-444444444444", "1.1.0", "1.0.0");
    TEST_ASSERT_FALSE(step.markAppValidCancelRollback);
    TEST_ASSERT_TRUE(step.reportStatus);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(OtaTargetStatus::FAILED), static_cast<int>(step.statusToReport));
    TEST_ASSERT_TRUE(step.clearPendingInNvs);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(ErrorCode::E012_OTA_FAILED), static_cast<int>(step.errorCode));
    TEST_ASSERT_NOT_NULL(std::strstr(step.errorMessage, "Rollback"));
}

void test_sha256_mismatch_sets_e012_clears_nvs_and_reports_failed_without_reboot() {
    OtaStateMachine fsm;
    fsm.evaluateCheckResponse(makeCheck(true, "1.1.0"), "1.0.0");

    OtaStepAction step = fsm.onDownloadComplete(true, BAD_SHA);
    TEST_ASSERT_TRUE(step.reportStatus);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(OtaTargetStatus::FAILED), static_cast<int>(step.statusToReport));
    TEST_ASSERT_TRUE(step.clearPendingInNvs);
    TEST_ASSERT_FALSE(step.rebootRequired);
    TEST_ASSERT_EQUAL_INT(static_cast<int>(ErrorCode::E012_OTA_FAILED), static_cast<int>(step.errorCode));
    TEST_ASSERT_NOT_NULL(std::strstr(step.errorMessage, "E012"));
}

int main(int, char**) {
    UNITY_BEGIN();
    RUN_TEST(test_check_same_version_or_no_update_is_noop);
    RUN_TEST(test_happy_path_downloading_flashing_reboot_and_post_boot_success);
    RUN_TEST(test_post_boot_automatic_rollback_reports_failed_and_clears_nvs);
    RUN_TEST(test_sha256_mismatch_sets_e012_clears_nvs_and_reports_failed_without_reboot);
    return UNITY_END();
}
