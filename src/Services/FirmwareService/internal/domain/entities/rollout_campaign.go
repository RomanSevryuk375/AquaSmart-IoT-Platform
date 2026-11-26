package entities

import (
	"errors"
	"fmt"

	"github.com/google/uuid"
)

type RolloutCampaignStatus int

const (
	RolloutCampaignPending RolloutCampaignStatus = iota
	RolloutCampaignActive
	RolloutCampaignPaused
	RolloutCampaignCompleted
	RolloutCampaignCanceled
)

func (s RolloutCampaignStatus) String() string {
	switch s {
	case RolloutCampaignPending:
		return "Pending"
	case RolloutCampaignActive:
		return "Active"
	case RolloutCampaignPaused:
		return "Paused"
	case RolloutCampaignCompleted:
		return "Completed"
	case RolloutCampaignCanceled:
		return "Canceled"
	default:
		return "Unknown"
	}
}

type RolloutCampaign struct {
	id           uuid.UUID
	firmwareId   uuid.UUID
	name         string
	status       RolloutCampaignStatus
	cancelReason string
	targets      map[uuid.UUID]*RolloutTarget
}

func NewRolloutCampaign(id uuid.UUID, firmwareId uuid.UUID, name string) (*RolloutCampaign, error) {
	if name == "" {
		return nil, errors.New("name cannot be empty")
	}

	if len(name) > MaxNameLength {
		return nil, fmt.Errorf("name cannot exceed %d symbols", MaxNameLength)
	}

	return &RolloutCampaign{
		id:         id,
		firmwareId: firmwareId,
		name:       name,
		status:     RolloutCampaignPending,
		targets:    make(map[uuid.UUID]*RolloutTarget),
	}, nil
}

func LoadCampaignFromDB(id, fwId uuid.UUID, name string, status RolloutCampaignStatus, cancelReason string, targets map[uuid.UUID]*RolloutTarget) *RolloutCampaign {
	return &RolloutCampaign{
		id:           id,
		firmwareId:   fwId,
		name:         name,
		status:       status,
		cancelReason: cancelReason,
		targets:      targets,
	}
}

func (rc *RolloutCampaign) AddTarget(controllerId uuid.UUID) error {
	if rc.status != RolloutCampaignPending {
		return errors.New("can only add targets to pending campaign")
	}

	if _, exists := rc.targets[controllerId]; exists {
		return errors.New("controller is already in the campaign")
	}

	rc.targets[controllerId] = NewRolloutTarget(controllerId)
	return nil
}

func (rc *RolloutCampaign) Start() error {
	if rc.status != RolloutCampaignPending && rc.status != RolloutCampaignPaused {
		return fmt.Errorf("cannot start campaign from %s status", rc.status)
	}

	if len(rc.targets) == 0 {
		return errors.New("cannot start campaign without targets")
	}

	rc.status = RolloutCampaignActive
	return nil
}

func (rc *RolloutCampaign) Pause() error {
	if rc.status != RolloutCampaignActive {
		return fmt.Errorf("cannot pause campaign from %s status", rc.status)
	}

	rc.status = RolloutCampaignPaused
	return nil
}

func (rc *RolloutCampaign) Cancel(reason string) error {
	if rc.status == RolloutCampaignCompleted || rc.status == RolloutCampaignCanceled {
		return errors.New("campaign is already completed or canceled")
	}

	rc.status = RolloutCampaignCanceled
	rc.cancelReason = reason
	return nil
}

func (rc *RolloutCampaign) UpdateTargetStatus(controllerId uuid.UUID, newStatus TargetStatus, errMsg string) error {
	if rc.status != RolloutCampaignActive {
		return errors.New("can only update targets in active campaign")
	}

	target, exists := rc.targets[controllerId]
	if !exists {
		return errors.New("target not found in campaign")
	}

	if target == nil {
		return errors.New("target not found in campaign")
	}

	var err error
	switch newStatus {
	case TargetDownloading:
		err = target.MarkAsDownloading()
	case TargetFlashing:
		err = target.MarkAsFlashing()
	case TargetSuccess:
		err = target.MarkAsSuccess()
	case TargetFailed:
		target.MarkAsFailed(errMsg)
	default:
		return errors.New("invalid target status transition")
	}

	if err != nil {
		return err
	}

	rc.checkCompletion()
	return nil
}

func (rc *RolloutCampaign) checkCompletion() {
	allDone := true
	for _, t := range rc.targets {
		if t.Status() != TargetSuccess && t.Status() != TargetFailed {
			allDone = false
			break
		}
	}

	if allDone {
		rc.status = RolloutCampaignCompleted
	}
}

func (rc *RolloutCampaign) ID() uuid.UUID                 { return rc.id }
func (rc *RolloutCampaign) FirmwareId() uuid.UUID         { return rc.firmwareId }
func (rc *RolloutCampaign) Status() RolloutCampaignStatus { return rc.status }
func (rc *RolloutCampaign) Name() string                  { return rc.name }
func (rc *RolloutCampaign) CancelReason() string          { return rc.cancelReason }

func (rc *RolloutCampaign) FindTargets(controllerId uuid.UUID) (*RolloutTarget, bool) {
	target, exists := rc.targets[controllerId]
	return target, exists
}

func (rc *RolloutCampaign) ListTargets() []*RolloutTarget {
	list := make([]*RolloutTarget, 0, len(rc.targets))
	for _, value := range rc.targets {
		list = append(list, value)
	}
	return list
}
