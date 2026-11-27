package presentation

import (
	"bytes"
	"context"
	"crypto/sha256"
	"encoding/hex"
	"encoding/json"
	"fmt"
	"io"
	"net/http"
	"net/http/httptest"
	"strconv"
	"sync"
	"testing"

	"github.com/google/uuid"
	"github.com/romansevryuk375/aquasmart-firmware/internal/application"
	"github.com/romansevryuk375/aquasmart-firmware/internal/domain/entities"
	"github.com/romansevryuk375/aquasmart-firmware/internal/domain/vo"
)

type inMemoryCampaignRepo struct {
	mu        sync.RWMutex
	campaigns map[uuid.UUID]*entities.RolloutCampaign
}

func newInMemoryCampaignRepo() *inMemoryCampaignRepo {
	return &inMemoryCampaignRepo{campaigns: make(map[uuid.UUID]*entities.RolloutCampaign)}
}

func (r *inMemoryCampaignRepo) GetById(ctx context.Context, id uuid.UUID) (*entities.RolloutCampaign, error) {
	r.mu.RLock()
	defer r.mu.RUnlock()
	return r.campaigns[id], nil
}

func (r *inMemoryCampaignRepo) GetActiveCampaignForController(ctx context.Context, controllerId uuid.UUID) (*entities.RolloutCampaign, error) {
	r.mu.RLock()
	defer r.mu.RUnlock()
	for _, c := range r.campaigns {
		if c.Status() == entities.RolloutCampaignActive {
			if _, ok := c.FindTargets(controllerId); ok {
				return c, nil
			}
		}
	}
	return nil, nil
}

func (r *inMemoryCampaignRepo) Save(ctx context.Context, rc *entities.RolloutCampaign) error {
	r.mu.Lock()
	defer r.mu.Unlock()
	r.campaigns[rc.ID()] = rc
	return nil
}

func (r *inMemoryCampaignRepo) Delete(ctx context.Context, rc *entities.RolloutCampaign) error {
	r.mu.Lock()
	defer r.mu.Unlock()
	delete(r.campaigns, rc.ID())
	return nil
}

type inMemoryFirmwareRepo struct {
	mu        sync.RWMutex
	firmwares map[uuid.UUID]*entities.Firmware
}

func newInMemoryFirmwareRepo() *inMemoryFirmwareRepo {
	return &inMemoryFirmwareRepo{firmwares: make(map[uuid.UUID]*entities.Firmware)}
}

func (r *inMemoryFirmwareRepo) GetById(ctx context.Context, id uuid.UUID) (*entities.Firmware, error) {
	r.mu.RLock()
	defer r.mu.RUnlock()
	return r.firmwares[id], nil
}

func (r *inMemoryFirmwareRepo) Save(ctx context.Context, fw *entities.Firmware) error {
	r.mu.Lock()
	defer r.mu.Unlock()
	r.firmwares[fw.ID()] = fw
	return nil
}

func (r *inMemoryFirmwareRepo) Delete(ctx context.Context, fw *entities.Firmware) error {
	r.mu.Lock()
	defer r.mu.Unlock()
	delete(r.firmwares, fw.ID())
	return nil
}

type mockBlobStorage struct {
	files map[string][]byte
}

func (m *mockBlobStorage) UploadFile(ctx context.Context, fileStream io.Reader, expectedSizeBytes int) (string, error) {
	data, err := io.ReadAll(fileStream)
	if err != nil {
		return "", err
	}
	key := "firmware/" + uuid.New().String() + ".bin"
	m.files[key] = data
	return key, nil
}

func (m *mockBlobStorage) GeneratePreSignedURL(ctx context.Context, storageKey string) (string, error) {
	return "http://minio:9000/bucket/" + storageKey, nil
}

func (m *mockBlobStorage) GetFile(ctx context.Context, storageKey string) (*application.BlobObject, error) {
	data, ok := m.files[storageKey]
	if !ok {
		return nil, fmt.Errorf("object %s not found", storageKey)
	}
	return &application.BlobObject{
		Body:          io.NopCloser(bytes.NewReader(data)),
		ContentLength: int64(len(data)),
		ContentType:   "application/octet-stream",
	}, nil
}

func setupOTATestEnv(t *testing.T, targetControllerIDs ...uuid.UUID) (*OTAHandler, *inMemoryCampaignRepo, uuid.UUID, []byte, string) {
	t.Helper()

	binaryData := []byte("ESP32_FIRMWARE_BINARY_PAYLOAD_V1_2_3_0123456789ABCDEF")
	sum := sha256.Sum256(binaryData)
	fileHash := hex.EncodeToString(sum[:])

	fwID := uuid.New()
	hwID := uuid.New()
	ver, err := vo.NewVersion(1, 2, 3)
	if err != nil {
		t.Fatalf("failed creating version: %v", err)
	}

	storageKey := "firmware/test-v1.2.3.bin"
	fw := entities.LoadFirmwareFromDB(fwID, hwID, *ver, fileHash, len(binaryData), storageKey, entities.FirmwarePublished, "")

	fwRepo := newInMemoryFirmwareRepo()
	_ = fwRepo.Save(context.Background(), fw)

	campRepo := newInMemoryCampaignRepo()
	campID := uuid.New()
	camp, err := entities.NewRolloutCampaign(campID, fwID, "Test Rollout v1.2.3")
	if err != nil {
		t.Fatalf("failed creating campaign: %v", err)
	}
	for _, cid := range targetControllerIDs {
		if err := camp.AddTarget(cid); err != nil {
			t.Fatalf("failed adding target: %v", err)
		}
	}
	if len(targetControllerIDs) > 0 {
		if err := camp.Start(); err != nil {
			t.Fatalf("failed starting campaign: %v", err)
		}
	}
	_ = campRepo.Save(context.Background(), camp)

	blob := &mockBlobStorage{files: map[string][]byte{storageKey: binaryData}}
	otaService := application.NewOtaDistributionService(campRepo, fwRepo, blob)
	campService := application.NewCampaignService(fwRepo, campRepo)

	return NewOTAHandler(otaService, campService), campRepo, campID, binaryData, fileHash
}

func TestOTAHandler_CheckUpdate_ActiveCampaignAndNoUpdate(t *testing.T) {
	targetController := uuid.New()
	otherController := uuid.New()

	handler, _, campID, _, expectedHash := setupOTATestEnv(t, targetController)

	// 1. Target controller with active campaign
	req := httptest.NewRequest(http.MethodGet, "/api/firmware/v1/ota/check?controller_id="+targetController.String(), nil)
	rec := httptest.NewRecorder()
	handler.CheckUpdate(rec, req)

	if rec.Code != http.StatusOK {
		t.Fatalf("expected 200 OK, got %d", rec.Code)
	}

	var resp OTAResponse
	if err := json.NewDecoder(rec.Body).Decode(&resp); err != nil {
		t.Fatalf("failed decoding response: %v", err)
	}
	if !resp.HasUpdate {
		t.Fatal("expected has_update=true")
	}
	if resp.CampaignID != campID.String() {
		t.Fatalf("expected campaign_id=%s, got %s", campID.String(), resp.CampaignID)
	}
	if resp.Version != "1.2.3" {
		t.Fatalf("expected version=1.2.3, got %s", resp.Version)
	}
	if resp.FileHash != expectedHash {
		t.Fatalf("expected file_hash=%s, got %s", expectedHash, resp.FileHash)
	}
	expectedURL := fmt.Sprintf("/api/firmware/v1/ota/download/%s?controller_id=%s", campID.String(), targetController.String())
	if resp.DownloadURL != expectedURL {
		t.Fatalf("expected download_url=%s, got %s", expectedURL, resp.DownloadURL)
	}

	// 2. Controller without active campaign
	req2 := httptest.NewRequest(http.MethodGet, "/api/firmware/v1/ota/check?controller_id="+otherController.String(), nil)
	rec2 := httptest.NewRecorder()
	handler.CheckUpdate(rec2, req2)

	if rec2.Code != http.StatusOK {
		t.Fatalf("expected 200 OK, got %d", rec2.Code)
	}
	var resp2 OTAResponse
	if err := json.NewDecoder(rec2.Body).Decode(&resp2); err != nil {
		t.Fatalf("failed decoding response2: %v", err)
	}
	if resp2.HasUpdate {
		t.Fatal("expected has_update=false for non-target controller")
	}
}

func TestOTAHandler_DownloadFirmware_StreamsBinaryAndBlocksNonTargets(t *testing.T) {
	targetController := uuid.New()
	nonTargetController := uuid.New()

	handler, _, campID, expectedBinary, expectedHash := setupOTATestEnv(t, targetController)

	mux := http.NewServeMux()
	mux.HandleFunc("GET /api/firmware/v1/ota/download/{campaignId}", handler.HandleDownloadFirmware)

	// 1. Valid active target -> streams binary with exact SHA-256 and Content-Length
	req := httptest.NewRequest(
		http.MethodGet,
		fmt.Sprintf("/api/firmware/v1/ota/download/%s?controller_id=%s", campID.String(), targetController.String()),
		nil,
	)
	rec := httptest.NewRecorder()
	mux.ServeHTTP(rec, req)

	if rec.Code != http.StatusOK {
		t.Fatalf("expected 200 OK, got %d (%s)", rec.Code, rec.Body.String())
	}
	if ct := rec.Header().Get("Content-Type"); ct != "application/octet-stream" {
		t.Fatalf("expected Content-Type application/octet-stream, got %q", ct)
	}
	if cl := rec.Header().Get("Content-Length"); cl != strconv.Itoa(len(expectedBinary)) {
		t.Fatalf("expected Content-Length %d, got %q", len(expectedBinary), cl)
	}
	downloaded := rec.Body.Bytes()
	sum := sha256.Sum256(downloaded)
	gotHash := hex.EncodeToString(sum[:])
	if gotHash != expectedHash {
		t.Fatalf("SHA-256 mismatch: expected %s, got %s", expectedHash, gotHash)
	}

	// 2. Non-target controller -> 403 Forbidden
	reqForbidden := httptest.NewRequest(
		http.MethodGet,
		fmt.Sprintf("/api/firmware/v1/ota/download/%s?controller_id=%s", campID.String(), nonTargetController.String()),
		nil,
	)
	recForbidden := httptest.NewRecorder()
	mux.ServeHTTP(recForbidden, reqForbidden)
	if recForbidden.Code != http.StatusForbidden {
		t.Fatalf("expected 403 Forbidden for non-target, got %d", recForbidden.Code)
	}

	// 3. Unknown campaign -> 404 Not Found
	reqNotFound := httptest.NewRequest(
		http.MethodGet,
		fmt.Sprintf("/api/firmware/v1/ota/download/%s?controller_id=%s", uuid.New().String(), targetController.String()),
		nil,
	)
	recNotFound := httptest.NewRecorder()
	mux.ServeHTTP(recNotFound, reqNotFound)
	if recNotFound.Code != http.StatusNotFound {
		t.Fatalf("expected 404 Not Found for unknown campaign, got %d", recNotFound.Code)
	}
}

func TestOTAHandler_UpdateTargetStatus_ValidTransitionsInvalidTransitionAndAutoComplete(t *testing.T) {
	controllerA := uuid.New()
	controllerB := uuid.New()
	handler, campRepo, campID, _, _ := setupOTATestEnv(t, controllerA, controllerB)

	sendStatus := func(controllerID uuid.UUID, status int, errMsg string) int {
		body, _ := json.Marshal(UpdateTargetStatusRequest{
			CampaignId:   campID,
			ControllerId: controllerID,
			Status:       status,
			ErrorMessage: errMsg,
		})
		req := httptest.NewRequest(http.MethodPost, "/api/firmware/v1/ota/status", bytes.NewReader(body))
		rec := httptest.NewRecorder()
		handler.HandleUpdateTargetStatus(rec, req)
		return rec.Code
	}

	// Invalid transition: Pending (0) -> Flashing (2) must return 409 Conflict
	if code := sendStatus(controllerA, int(entities.TargetFlashing), ""); code != http.StatusConflict {
		t.Fatalf("expected 409 Conflict on 0 -> 2 transition, got %d", code)
	}

	// Valid sequence for controllerA: 0 -> 1 -> 2 -> 3
	if code := sendStatus(controllerA, int(entities.TargetDownloading), ""); code != http.StatusNoContent {
		t.Fatalf("expected 204 on 0 -> 1, got %d", code)
	}
	if code := sendStatus(controllerA, int(entities.TargetFlashing), ""); code != http.StatusNoContent {
		t.Fatalf("expected 204 on 1 -> 2, got %d", code)
	}
	if code := sendStatus(controllerA, int(entities.TargetSuccess), ""); code != http.StatusNoContent {
		t.Fatalf("expected 204 on 2 -> 3, got %d", code)
	}

	// Campaign must still be Active while controllerB is Pending
	camp, _ := campRepo.GetById(context.Background(), campID)
	if camp.Status() != entities.RolloutCampaignActive {
		t.Fatalf("expected campaign still Active before all targets finish, got %s", camp.Status())
	}

	// Valid failure transition for controllerB: * -> 4 (Failed) with error_message -> auto-completes campaign
	if code := sendStatus(controllerB, int(entities.TargetFailed), "E012 SHA-256 mismatch"); code != http.StatusNoContent {
		t.Fatalf("expected 204 on * -> 4, got %d", code)
	}

	campAfter, _ := campRepo.GetById(context.Background(), campID)
	if campAfter.Status() != entities.RolloutCampaignCompleted {
		t.Fatalf("expected campaign auto-completed after all targets reached Success/Failed, got %s", campAfter.Status())
	}
	targetB, _ := campAfter.FindTargets(controllerB)
	if targetB.ErrorMessage() != "E012 SHA-256 mismatch" {
		t.Fatalf("expected saved error message, got %q", targetB.ErrorMessage())
	}
}

func TestOTAHandler_FailedTarget_StopsUpdateLoopAndRejectsSuccessTransition(t *testing.T) {
	controllerA := uuid.New()
	controllerB := uuid.New()
	handler, _, campID, _, _ := setupOTATestEnv(t, controllerA, controllerB)

	// Mark controllerA as Failed while controllerB remains Pending (campaign stays Active)
	failBody, _ := json.Marshal(UpdateTargetStatusRequest{
		CampaignId:   campID,
		ControllerId: controllerA,
		Status:       int(entities.TargetFailed),
		ErrorMessage: "E012 SHA-256 mismatch",
	})
	failReq := httptest.NewRequest(http.MethodPost, "/api/firmware/v1/ota/status", bytes.NewReader(failBody))
	failRec := httptest.NewRecorder()
	handler.HandleUpdateTargetStatus(failRec, failReq)
	if failRec.Code != http.StatusNoContent {
		t.Fatalf("expected 204 marking TargetFailed, got %d", failRec.Code)
	}

	// CheckUpdate for failed controllerA must return has_update=false (no infinite download loop)
	checkReq := httptest.NewRequest(http.MethodGet, "/api/firmware/v1/ota/check?controller_id="+controllerA.String(), nil)
	checkRec := httptest.NewRecorder()
	handler.CheckUpdate(checkRec, checkReq)
	if checkRec.Code != http.StatusOK {
		t.Fatalf("expected 200 OK from check, got %d", checkRec.Code)
	}
	var checkResp OTAResponse
	if err := json.NewDecoder(checkRec.Body).Decode(&checkResp); err != nil {
		t.Fatalf("failed decoding check response: %v", err)
	}
	if checkResp.HasUpdate {
		t.Fatal("expected has_update=false after target reached TargetFailed")
	}

	// DownloadFirmware for failed controllerA must return 403 Forbidden
	mux := http.NewServeMux()
	mux.HandleFunc("GET /api/firmware/v1/ota/download/{campaignId}", handler.HandleDownloadFirmware)
	dlReq := httptest.NewRequest(http.MethodGet, fmt.Sprintf("/api/firmware/v1/ota/download/%s?controller_id=%s", campID.String(), controllerA.String()), nil)
	dlRec := httptest.NewRecorder()
	mux.ServeHTTP(dlRec, dlReq)
	if dlRec.Code != http.StatusForbidden {
		t.Fatalf("expected 403 Forbidden for failed target download, got %d", dlRec.Code)
	}

	// Transitioning Failed -> Success must be rejected with 409 Conflict
	successBody, _ := json.Marshal(UpdateTargetStatusRequest{
		CampaignId:   campID,
		ControllerId: controllerA,
		Status:       int(entities.TargetSuccess),
	})
	successReq := httptest.NewRequest(http.MethodPost, "/api/firmware/v1/ota/status", bytes.NewReader(successBody))
	successRec := httptest.NewRecorder()
	handler.HandleUpdateTargetStatus(successRec, successReq)
	if successRec.Code != http.StatusConflict {
		t.Fatalf("expected 409 Conflict on Failed -> Success transition, got %d", successRec.Code)
	}
}

func TestOTAHandler_UpdateTargetStatus_ContextControllerMismatch_Returns403(t *testing.T) {
	controllerA := uuid.New()
	controllerB := uuid.New()
	handler, _, campID, _, _ := setupOTATestEnv(t, controllerA, controllerB)

	validator := &mockDeviceTokenValidator{
		validateFn: func(ctx context.Context, id uuid.UUID, token string) (*application.DeviceAuthResult, error) {
			return &application.DeviceAuthResult{IsValid: true, ControllerID: id}, nil
		},
	}

	wrapped := DeviceAuthMiddleware(validator, handler.HandleUpdateTargetStatus)
	body, _ := json.Marshal(UpdateTargetStatusRequest{
		CampaignId:   campID,
		ControllerId: controllerB,
		Status:       int(entities.TargetFailed),
		ErrorMessage: "spoofed",
	})
	req := httptest.NewRequest(http.MethodPost, "/api/firmware/v1/ota/status?controller_id="+controllerA.String(), bytes.NewReader(body))
	req.Header.Set(DeviceTokenHeader, "ak_token_a")
	rec := httptest.NewRecorder()

	wrapped.ServeHTTP(rec, req)

	if rec.Code != http.StatusForbidden {
		t.Fatalf("expected 403 Forbidden on IDOR mismatch, got %d", rec.Code)
	}
}
