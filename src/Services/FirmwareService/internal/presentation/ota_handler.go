package presentation

import (
	"encoding/json"
	"errors"
	"fmt"
	"io"
	"log"
	"net/http"
	"strconv"

	"github.com/google/uuid"
	"github.com/romansevryuk375/aquasmart-firmware/internal/application"
	"github.com/romansevryuk375/aquasmart-firmware/internal/domain/entities"
)

type OTAResponse struct {
	HasUpdate   bool   `json:"has_update"`
	CampaignID  string `json:"campaign_id,omitempty"`
	Version     string `json:"version,omitempty"`
	DownloadURL string `json:"download_url,omitempty"`
	FileHash    string `json:"file_hash,omitempty"`
}

type UpdateTargetStatusRequest struct {
	CampaignId   uuid.UUID `json:"campaign_id"`
	ControllerId uuid.UUID `json:"controller_id"`
	Status       int       `json:"status"`
	ErrorMessage string    `json:"error_message"`
}

type OTAHandler struct {
	otaService  *application.OtaDistributionService
	campService *application.CampaignService
}

func NewOTAHandler(otaService *application.OtaDistributionService, campService *application.CampaignService) *OTAHandler {
	return &OTAHandler{
		otaService:  otaService,
		campService: campService,
	}
}

// @Summary      Проверка обновлений (OTA)
// @Description  Ищет активную кампанию обновления для переданного контроллера. Возвращает относительный путь прокси-скачивания бинарника.
// @Tags         Device API
// @Produce      json
// @Param        controller_id query string true "UUID контроллера"
// @Success      200 {object} OTAResponse "Возвращает статус обновления и метаданные"
// @Failure      400 {string} string "Неверный формат controller_id"
// @Failure      401 {string} string "Отсутствует X-Device-Token"
// @Failure      403 {string} string "Токен не соответствует контроллеру"
// @Failure      500 {string} string "Внутренняя ошибка сервера"
// @Router       /api/firmware/v1/ota/check [get]
func (h *OTAHandler) CheckUpdate(w http.ResponseWriter, r *http.Request) {
	controllerIdStr := r.URL.Query().Get("controller_id")
	if controllerIdStr == "" {
		http.Error(w, "missing controller_id parameter", http.StatusBadRequest)
		return
	}

	controllerId, err := uuid.Parse(controllerIdStr)
	if err != nil {
		http.Error(w, "invalid controller_id format", http.StatusBadRequest)
		return
	}

	meta, err := h.otaService.CheckForUpdate(r.Context(), controllerId)
	if err != nil {
		log.Printf("[ERROR] Failed to check OTA for controller %s: %v", controllerId, err)
		http.Error(w, "internal server error", http.StatusInternalServerError)
		return
	}

	response := OTAResponse{
		HasUpdate:   meta.HasUpdate,
		Version:     meta.Version,
		DownloadURL: meta.DownloadURL,
		FileHash:    meta.FileHash,
	}
	if meta.CampaignID != uuid.Nil {
		response.CampaignID = meta.CampaignID.String()
	}

	w.Header().Set("Content-Type", "application/json")
	w.WriteHeader(http.StatusOK)

	if err := json.NewEncoder(w).Encode(response); err != nil {
		log.Printf("[ERROR] Failed to encode OTA response: %v", err)
	}
}

// @Summary      Скачать бинарник прошивки (OTA Proxy)
// @Description  Проверяет, что контроллер является активным таргетом кампании, и стримит бинарник прошивки из S3/MinIO.
// @Tags         Device API
// @Produce      octet-stream
// @Param        campaignId path string true "UUID кампании"
// @Param        controller_id query string true "UUID контроллера"
// @Success      200 {file} binary "Бинарный файл прошивки"
// @Failure      400 {string} string "Неверный формат UUID"
// @Failure      401 {string} string "Отсутствует X-Device-Token"
// @Failure      403 {string} string "Контроллер не является активным таргетом кампании"
// @Failure      404 {string} string "Кампания или прошивка не найдены"
// @Failure      500 {string} string "Внутренняя ошибка сервера"
// @Router       /api/firmware/v1/ota/download/{campaignId} [get]
func (h *OTAHandler) HandleDownloadFirmware(w http.ResponseWriter, r *http.Request) {
	campaignIdStr := r.PathValue("campaignId")
	if campaignIdStr == "" {
		http.Error(w, "missing campaignId parameter", http.StatusBadRequest)
		return
	}

	campaignID, err := uuid.Parse(campaignIdStr)
	if err != nil {
		http.Error(w, "invalid campaignId format", http.StatusBadRequest)
		return
	}

	controllerIdStr := r.URL.Query().Get("controller_id")
	if controllerIdStr == "" {
		http.Error(w, "missing controller_id parameter", http.StatusBadRequest)
		return
	}

	controllerID, err := uuid.Parse(controllerIdStr)
	if err != nil {
		http.Error(w, "invalid controller_id format", http.StatusBadRequest)
		return
	}

	download, err := h.otaService.DownloadFirmware(r.Context(), campaignID, controllerID)
	if err != nil {
		switch {
		case errors.Is(err, application.ErrCampaignNotFound), errors.Is(err, application.ErrFirmwareNotFound):
			http.Error(w, err.Error(), http.StatusNotFound)
		case errors.Is(err, application.ErrControllerNotTarget),
			errors.Is(err, application.ErrCampaignNotActive),
			errors.Is(err, application.ErrTargetAlreadyCompleted):
			http.Error(w, err.Error(), http.StatusForbidden)
		default:
			log.Printf("[ERROR] Failed to download firmware for campaign %s controller %s: %v", campaignID, controllerID, err)
			http.Error(w, "internal server error", http.StatusInternalServerError)
		}
		return
	}
	defer download.Body.Close()

	w.Header().Set("Content-Type", "application/octet-stream")
	w.Header().Set("Content-Disposition", `attachment; filename="firmware.bin"`)
	if download.ContentLength >= 0 {
		w.Header().Set("Content-Length", strconv.FormatInt(download.ContentLength, 10))
	}
	if download.FileHash != "" {
		w.Header().Set("X-Firmware-Sha256", download.FileHash)
	}
	w.WriteHeader(http.StatusOK)

	if _, err := io.Copy(w, download.Body); err != nil {
		log.Printf("[ERROR] Failed while streaming firmware binary for campaign %s controller %s: %v", campaignID, controllerID, err)
	}
}

// @Summary      Обновить статус прошивки устройства
// @Description  Вызывается ESP32 для сообщения о начале скачивания, успешной прошивке или ошибке обновления.
// @Tags         Device API
// @Accept       json
// @Produce      json
// @Param        request body UpdateTargetStatusRequest true "Данные о статусе обновления"
// @Success      204 "Статус успешно обновлен"
// @Failure      400 {string} string "Неверный формат запроса"
// @Failure      401 {string} string "Отсутствует X-Device-Token"
// @Failure      403 {string} string "Токен не соответствует контроллеру"
// @Failure      404 {string} string "Кампания не найдена"
// @Failure      409 {string} string "Недопустимый переход статуса"
// @Failure      500 {string} string "Внутренняя ошибка сервера"
// @Router       /api/firmware/v1/ota/status [post]
func (h *OTAHandler) HandleUpdateTargetStatus(w http.ResponseWriter, r *http.Request) {
	r.Body = http.MaxBytesReader(w, r.Body, 1*1024*1024)

	var req UpdateTargetStatusRequest
	decoder := json.NewDecoder(r.Body)
	decoder.DisallowUnknownFields()
	if err := decoder.Decode(&req); err != nil {
		http.Error(w, fmt.Sprintf("invalid request body: %v", err), http.StatusBadRequest)
		return
	}

	if authID, ok := AuthenticatedControllerIDFromContext(r.Context()); ok && authID != req.ControllerId {
		http.Error(w, "forbidden: controller_id mismatch", http.StatusForbidden)
		return
	}

	if err := h.campService.HandleTargetStatusUpdate(
		r.Context(),
		req.CampaignId,
		req.ControllerId,
		entities.TargetStatus(req.Status),
		req.ErrorMessage); err != nil {
		switch {
		case errors.Is(err, application.ErrCampaignNotFound):
			http.Error(w, err.Error(), http.StatusNotFound)
		case errors.Is(err, application.ErrControllerNotTarget):
			http.Error(w, err.Error(), http.StatusForbidden)
		case errors.Is(err, application.ErrCampaignNotActive), errors.Is(err, application.ErrInvalidTargetStatusTransition):
			http.Error(w, err.Error(), http.StatusConflict)
		default:
			log.Printf("[ERROR] Failed to update target %s: %v", req.ControllerId, err)
			http.Error(w, "internal server error", http.StatusInternalServerError)
		}
		return
	}

	w.WriteHeader(http.StatusNoContent)
}
