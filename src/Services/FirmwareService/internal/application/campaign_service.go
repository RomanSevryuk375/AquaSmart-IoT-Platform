package application

import (
	"context"
	"errors"
	"fmt"

	"github.com/google/uuid"
	"github.com/romansevryuk375/aquasmart-firmware/internal/domain/entities"
)

type CampaignService struct {
	firmwareRepo FirmwareRepository
	campaignRepo CampaignRepository
}

func NewCampaignService(fwRepo FirmwareRepository, campRepo CampaignRepository) *CampaignService {
	return &CampaignService{
		firmwareRepo: fwRepo,
		campaignRepo: campRepo,
	}
}

func (s *CampaignService) CreateCampaign(ctx context.Context, releaseId uuid.UUID, name string) (*entities.RolloutCampaign, error) {
	fw, err := s.firmwareRepo.GetById(ctx, releaseId)
	if err != nil {
		return nil, fmt.Errorf("failed to get firmware: %w", err)
	}
	if fw == nil {
		return nil, errors.New("firmware not found")
	}

	if fw.Status() != entities.FirmwarePublished {
		return nil, errors.New("cannot create campaign for unpublished firmware")
	}

	rc, err := entities.NewRolloutCampaign(uuid.New(), releaseId, name)
	if err != nil {
		return nil, fmt.Errorf("failed to initialize rollout campaign: %w", err)
	}

	if err := s.campaignRepo.Save(ctx, rc); err != nil {
		return nil, fmt.Errorf("failed to save campaign: %w", err)
	}

	return rc, nil
}

func (s *CampaignService) AddTargetsToCampaign(ctx context.Context, campaignId uuid.UUID, controllerIds []uuid.UUID) error {
	rc, err := s.campaignRepo.GetById(ctx, campaignId)
	if err != nil {
		return fmt.Errorf("failed to get campaign: %w", err)
	}
	if rc == nil {
		return errors.New("campaign not found")
	}

	if len(controllerIds) == 0 {
		return errors.New("controller ids is empty")
	}

	for _, val := range controllerIds {
		if err = rc.AddTarget(val); err != nil {
			return fmt.Errorf("failed to add target controller: %w", err)
		}
	}

	if err = s.campaignRepo.Save(ctx, rc); err != nil {
		return fmt.Errorf("failed to save campaign: %w:", err)
	}
	return nil
}

func (s *CampaignService) StartCampaign(ctx context.Context, campaignId uuid.UUID) error {
	rc, err := s.campaignRepo.GetById(ctx, campaignId)
	if err != nil {
		return fmt.Errorf("failed to get campaign: %w", err)
	}
	if rc == nil {
		return errors.New("campaign not found")
	}

	if err = rc.Start(); err != nil {
		return fmt.Errorf("failed to start campaign: %w", err)
	}

	if err = s.campaignRepo.Save(ctx, rc); err != nil {
		return fmt.Errorf("failed to save campaign: %w:", err)
	}
	return nil
}

func (s *CampaignService) CancelCampaign(ctx context.Context, campaignId uuid.UUID, reason string) error {
	rc, err := s.campaignRepo.GetById(ctx, campaignId)
	if err != nil {
		return fmt.Errorf("failed to get campaign: %w", err)
	}
	if rc == nil {
		return errors.New("campaign not found")
	}

	if err = rc.Cancel(reason); err != nil {
		return fmt.Errorf("failed to cancel campaign: %w", err)
	}

	if err = s.campaignRepo.Save(ctx, rc); err != nil {
		return fmt.Errorf("failed to save campaign: %w:", err)
	}
	return nil
}

func (s *CampaignService) HandleTargetStatusUpdate(ctx context.Context, campaignId uuid.UUID, controllerId uuid.UUID, status entities.TargetStatus, errorMsg string) error {
	rc, err := s.campaignRepo.GetById(ctx, campaignId)
	if err != nil {
		return fmt.Errorf("failed to get campaign: %w", err)
	}
	if rc == nil {
		return ErrCampaignNotFound
	}
	if rc.Status() != entities.RolloutCampaignActive {
		return ErrCampaignNotActive
	}
	if _, exists := rc.FindTargets(controllerId); !exists {
		return ErrControllerNotTarget
	}

	if err := rc.UpdateTargetStatus(controllerId, status, errorMsg); err != nil {
		return fmt.Errorf("%w: %v", ErrInvalidTargetStatusTransition, err)
	}

	if err = s.campaignRepo.Save(ctx, rc); err != nil {
		return fmt.Errorf("failed to save campaign: %w", err)
	}
	return nil
}

func (s *CampaignService) DeleteCampaign(ctx context.Context, campaignId uuid.UUID) error {
	rc, err := s.campaignRepo.GetById(ctx, campaignId)
	if err != nil {
		return fmt.Errorf("failed to get campaign: %w", err)
	}
	if rc == nil {
		return nil
	}

	if err = s.campaignRepo.Delete(ctx, rc); err != nil {
		return fmt.Errorf("failed to delete campaign: %w", err)
	}
	return nil
}

func (s *CampaignService) PauseCampaign(ctx context.Context, campaignId uuid.UUID) error {
	rc, err := s.campaignRepo.GetById(ctx, campaignId)
	if err != nil {
		return fmt.Errorf("failed to get campaign: %w", err)
	}
	if rc == nil {
		return nil
	}

	if err := rc.Pause(); err != nil {
		return fmt.Errorf("failed to pause target status: %w", err)
	}

	if err = s.campaignRepo.Save(ctx, rc); err != nil {
		return fmt.Errorf("failed to save campaign: %w:", err)
	}
	return nil
}
