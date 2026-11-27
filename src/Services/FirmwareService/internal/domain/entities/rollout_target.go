package entities

import (
	"errors"
	"time"

	"github.com/google/uuid"
)

type TargetStatus int

const (
	TargetPending TargetStatus = iota
	TargetDownloading
	TargetFlashing
	TargetSuccess
	TargetFailed
)

func (s TargetStatus) String() string {
	switch s {
	case TargetPending:
		return "Pending"
	case TargetDownloading:
		return "Downloading"
	case TargetFlashing:
		return "Flashing"
	case TargetSuccess:
		return "Success"
	case TargetFailed:
		return "Failed"
	default:
		return "Unknown"
	}
}

type RolloutTarget struct {
	id           uuid.UUID
	controllerId uuid.UUID
	status       TargetStatus
	errorMessage string
	updatedAt    time.Time
}

func NewRolloutTarget(controllerId uuid.UUID) *RolloutTarget {
	return &RolloutTarget{
		id:           uuid.New(),
		controllerId: controllerId,
		status:       TargetPending,
		updatedAt:    time.Now().UTC(),
	}
}

func LoadTargetFromDB(id uuid.UUID, controllerId uuid.UUID, status TargetStatus, errorMsg string) *RolloutTarget {
	return &RolloutTarget{
		id:           id,
		controllerId: controllerId,
		status:       status,
		errorMessage: errorMsg,
	}
}

func (t *RolloutTarget) MarkAsDownloading() error {
	if t.status != TargetPending {
		return errors.New("can only start downloading from pending state")
	}
	t.status = TargetDownloading
	t.updatedAt = time.Now().UTC()
	return nil
}

func (t *RolloutTarget) MarkAsFlashing() error {
	if t.status != TargetDownloading {
		return errors.New("can only start flashing from downloading state")
	}
	t.status = TargetFlashing
	t.updatedAt = time.Now().UTC()
	return nil
}

func (t *RolloutTarget) MarkAsSuccess() error {
	if t.status == TargetFailed {
		return errors.New("cannot mark failed target as success")
	}
	t.status = TargetSuccess
	t.updatedAt = time.Now().UTC()
	return nil
}

func (t *RolloutTarget) MarkAsFailed(err string) {
	t.status = TargetFailed
	t.errorMessage = err
	t.updatedAt = time.Now().UTC()
}

func (t *RolloutTarget) ID() uuid.UUID           { return t.id }
func (t *RolloutTarget) ControllerId() uuid.UUID { return t.controllerId }
func (t *RolloutTarget) Status() TargetStatus    { return t.status }
func (t *RolloutTarget) ErrorMessage() string    { return t.errorMessage }
func (t *RolloutTarget) UpdatedAt() time.Time    { return t.updatedAt }
