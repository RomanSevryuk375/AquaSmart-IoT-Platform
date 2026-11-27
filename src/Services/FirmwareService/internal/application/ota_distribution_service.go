package application

import (
	"context"
	"errors"
	"fmt"
	"io"

	"github.com/google/uuid"
	"github.com/romansevryuk375/aquasmart-firmware/internal/domain/entities"
)

var (
	ErrCampaignNotFound              = errors.New("campaign not found")
	ErrCampaignNotActive             = errors.New("campaign is not active")
	ErrControllerNotTarget           = errors.New("controller is not a target of this campaign")
	ErrTargetAlreadyCompleted        = errors.New("target has already completed this campaign")
	ErrFirmwareNotFound              = errors.New("firmware not found")
	ErrInvalidTargetStatusTransition = errors.New("invalid target status transition")
)

type UpdateMetadata struct {
	HasUpdate   bool
	CampaignID  uuid.UUID
	Version     string
	DownloadURL string
	FileHash    string
}

type FirmwareDownload struct {
	Body          io.ReadCloser
	ContentLength int64
	FileHash      string
	Version       string
}

type OtaDistributionService struct {
	campaignRepository CampaignRepository
	firmwareRepository FirmwareRepository
	blobStorage        BlobStorage
}

func NewOtaDistributionService(campaignRepository CampaignRepository, firmwareRepository FirmwareRepository, blobStorage BlobStorage) *OtaDistributionService {
	return &OtaDistributionService{
		campaignRepository: campaignRepository,
		firmwareRepository: firmwareRepository,
		blobStorage:        blobStorage,
	}
}

func (s *OtaDistributionService) CheckForUpdate(ctx context.Context, controllerId uuid.UUID) (*UpdateMetadata, error) {
	camp, err := s.campaignRepository.GetActiveCampaignForController(ctx, controllerId)
	if err != nil {
		return nil, fmt.Errorf("failed to get campaign: %w", err)
	}
	if camp == nil {
		return &UpdateMetadata{
			HasUpdate: false,
		}, nil
	}

	val, exists := camp.FindTargets(controllerId)
	if !exists || val == nil {
		return nil, fmt.Errorf("target controller %v not found: %w", controllerId, ErrControllerNotTarget)
	}
	if val.Status() == entities.TargetSuccess || val.Status() == entities.TargetFailed {
		return &UpdateMetadata{
			HasUpdate: false,
		}, nil
	}

	fw, err := s.firmwareRepository.GetById(ctx, camp.FirmwareId())
	if err != nil {
		return nil, fmt.Errorf("failed to get firmware: %w", err)
	}
	if fw == nil {
		return nil, ErrFirmwareNotFound
	}

	downloadURL := fmt.Sprintf("/api/firmware/v1/ota/download/%s?controller_id=%s", camp.ID().String(), controllerId.String())
	version := fw.Version()

	return &UpdateMetadata{
		HasUpdate:   true,
		CampaignID:  camp.ID(),
		Version:     version.Value(),
		DownloadURL: downloadURL,
		FileHash:    fw.FileHash(),
	}, nil
}

func (s *OtaDistributionService) DownloadFirmware(ctx context.Context, campaignID uuid.UUID, controllerID uuid.UUID) (*FirmwareDownload, error) {
	camp, err := s.campaignRepository.GetById(ctx, campaignID)
	if err != nil {
		return nil, fmt.Errorf("failed to get campaign: %w", err)
	}
	if camp == nil {
		return nil, ErrCampaignNotFound
	}
	if camp.Status() != entities.RolloutCampaignActive {
		return nil, ErrCampaignNotActive
	}

	target, exists := camp.FindTargets(controllerID)
	if !exists || target == nil {
		return nil, ErrControllerNotTarget
	}
	if target.Status() == entities.TargetSuccess || target.Status() == entities.TargetFailed {
		return nil, ErrTargetAlreadyCompleted
	}

	fw, err := s.firmwareRepository.GetById(ctx, camp.FirmwareId())
	if err != nil {
		return nil, fmt.Errorf("failed to get firmware: %w", err)
	}
	if fw == nil {
		return nil, ErrFirmwareNotFound
	}

	blob, err := s.blobStorage.GetFile(ctx, fw.StorageKey())
	if err != nil {
		return nil, fmt.Errorf("failed to fetch firmware binary: %w", err)
	}

	contentLength := blob.ContentLength
	if contentLength < 0 && fw.SizeBytes() > 0 {
		contentLength = int64(fw.SizeBytes())
	}

	version := fw.Version()
	return &FirmwareDownload{
		Body:          blob.Body,
		ContentLength: contentLength,
		FileHash:      fw.FileHash(),
		Version:       version.Value(),
	}, nil
}
