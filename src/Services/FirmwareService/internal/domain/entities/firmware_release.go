package entities

import (
	"errors"
	"fmt"

	"github.com/google/uuid"
	"github.com/romansevryuk375/aquasmart-firmware/internal/domain/vo"
)

type FirmwareStatus int

const (
	FirmwareDraft FirmwareStatus = iota
	FirmwarePublished
	FirmwareRevoked
)

func (s FirmwareStatus) String() string {
	switch s {
	case FirmwareDraft:
		return "Draft"
	case FirmwarePublished:
		return "Published"
	case FirmwareRevoked:
		return "Revoked"
	default:
		return "Unknown"
	}
}

const (
	FileHashLength   = 64
	MinFileSizeBytes = 0
	MaxFileSizeBytes = 4194304
)

type Firmware struct {
	id                uuid.UUID
	hardwareProfileId uuid.UUID
	version           vo.Version
	fileHash          string
	sizeBytes         int
	storageKey        string
	status            FirmwareStatus
	revokeReason      string
}

func NewFirmwareRelease(id uuid.UUID, hardwareProfileId uuid.UUID, version vo.Version, fileHash string, sizeBytes int) (*Firmware, error) {

	if sizeBytes <= MinFileSizeBytes {
		return nil, errors.New("sizeBytes must be greater than zero")
	}

	if sizeBytes >= MaxFileSizeBytes {
		return nil, fmt.Errorf("sizeBytes must be less then than %d", MaxFileSizeBytes)
	}

	if fileHash == "" {
		return nil, errors.New("fileHash cannot be empty")
	}

	if len(fileHash) != FileHashLength {
		return nil, fmt.Errorf("fileHash should have %d symbols", FileHashLength)
	}

	return &Firmware{
		id:                id,
		hardwareProfileId: hardwareProfileId,
		version:           version,
		fileHash:          fileHash,
		sizeBytes:         sizeBytes,
		status:            FirmwareDraft,
	}, nil
}

func LoadFirmwareFromDB(id, hwId uuid.UUID, version vo.Version, fileHash string, sizeBytes int, storageKey string, status FirmwareStatus, revokeReason string) *Firmware {
	return &Firmware{
		id:                id,
		hardwareProfileId: hwId,
		version:           version,
		fileHash:          fileHash,
		sizeBytes:         sizeBytes,
		storageKey:        storageKey,
		status:            status,
		revokeReason:      revokeReason,
	}
}

func (fw *Firmware) Publish() error {
	if fw.status != FirmwareDraft {
		return errors.New("only draft firmware can be published")
	}
	fw.status = FirmwarePublished
	return nil
}

func (fw *Firmware) Revoke(reason string) error {
	if fw.status == FirmwareRevoked {
		return errors.New("firmware is already revoked")
	}
	if fw.status == FirmwareDraft {
		return errors.New("cannot revoke a draft firmware")
	}
	if reason == "" {
		return errors.New("revoke reason is required")
	}

	fw.revokeReason = reason
	fw.status = FirmwareRevoked
	return nil
}

func (fw *Firmware) SetStorageKey(key string) error {
	if key == "" {
		return errors.New("key cannot be empty")
	}

	fw.storageKey = key

	return nil
}

func (fw *Firmware) ID() uuid.UUID                { return fw.id }
func (fw *Firmware) HardwareProfileId() uuid.UUID { return fw.hardwareProfileId }
func (fw *Firmware) Version() vo.Version          { return fw.version }
func (fw *Firmware) FileHash() string             { return fw.fileHash }
func (fw *Firmware) SizeBytes() int               { return fw.sizeBytes }
func (fw *Firmware) StorageKey() string           { return fw.storageKey }
func (fw *Firmware) Status() FirmwareStatus       { return fw.status }
func (fw *Firmware) RevokeReason() string         { return fw.revokeReason }
