package application

import (
	"context"
	"io"

	"github.com/google/uuid"
	"github.com/romansevryuk375/aquasmart-firmware/internal/domain/entities"
)

type FirmwareRepository interface {
	GetById(ctx context.Context, id uuid.UUID) (*entities.Firmware, error)
	Save(ctx context.Context, fw *entities.Firmware) error
	Delete(ctx context.Context, fw *entities.Firmware) error
}

type HardwareProfileRepository interface {
	GetById(ctx context.Context, id uuid.UUID) (*entities.HardwareProfile, error)
	Save(ctx context.Context, hp *entities.HardwareProfile) error
}

type CampaignRepository interface {
	GetById(ctx context.Context, id uuid.UUID) (*entities.RolloutCampaign, error)
	GetActiveCampaignForController(ctx context.Context, controllerId uuid.UUID) (*entities.RolloutCampaign, error)
	Save(ctx context.Context, rc *entities.RolloutCampaign) error
	Delete(ctx context.Context, rc *entities.RolloutCampaign) error
}

type BlobObject struct {
	Body          io.ReadCloser
	ContentLength int64
	ContentType   string
}

type BlobStorage interface {
	UploadFile(ctx context.Context, fileStream io.Reader, expectedSizeBytes int) (string, error)
	GeneratePreSignedURL(ctx context.Context, storageKey string) (string, error)
	GetFile(ctx context.Context, storageKey string) (*BlobObject, error)
}

type DeviceAuthResult struct {
	IsValid      bool
	ControllerID uuid.UUID
	UserID       uuid.UUID
}

type DeviceTokenValidator interface {
	ValidateTokenByControllerID(ctx context.Context, controllerID uuid.UUID, deviceToken string) (*DeviceAuthResult, error)
}
