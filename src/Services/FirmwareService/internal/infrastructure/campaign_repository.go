package infrastructure

import (
	"context"
	"database/sql"
	"errors"
	"fmt"

	"github.com/google/uuid"
	"github.com/jmoiron/sqlx"
	"github.com/romansevryuk375/aquasmart-firmware/internal/domain/entities"
)

type campaignDAO struct {
	ID           uuid.UUID `db:"id"`
	FirmwareID   uuid.UUID `db:"firmware_id"`
	Name         string    `db:"name"`
	Status       int       `db:"status"`
	CancelReason *string   `db:"cancel_reason"`
}

type targetDAO struct {
	ID           uuid.UUID `db:"id"`
	CampaignID   uuid.UUID `db:"campaign_id"`
	ControllerID uuid.UUID `db:"controller_id"`
	Status       int       `db:"status"`
	ErrorMessage *string   `db:"error_message"`
}

type PostgresCampaignRepository struct {
	db *sqlx.DB
}

func NewPostgresCampaignRepository(db *sqlx.DB) *PostgresCampaignRepository {
	return &PostgresCampaignRepository{db: db}
}

func (r *PostgresCampaignRepository) GetActiveCampaignForController(ctx context.Context, controllerId uuid.UUID) (*entities.RolloutCampaign, error) {
	queryCamp := `
		SELECT c.id, c.firmware_id, c.name, c.status, c.cancel_reason
		FROM rollout_campaigns c
		JOIN rollout_targets t ON t.campaign_id = c.id
		WHERE c.status = $1 
		  AND t.controller_id = $2
		LIMIT 1
	`
	var cDAO campaignDAO
	err := r.db.GetContext(ctx, &cDAO, queryCamp, entities.RolloutCampaignActive, controllerId)
	if errors.Is(err, sql.ErrNoRows) {
		return nil, nil
	}
	if err != nil {
		return nil, fmt.Errorf("failed to query active campaign: %w", err)
	}

	queryTargets := `
		SELECT id, campaign_id, controller_id, status, error_message
		FROM rollout_targets
		WHERE campaign_id = $1
	`
	var tDAOs []targetDAO
	if err := r.db.SelectContext(ctx, &tDAOs, queryTargets, cDAO.ID); err != nil {
		return nil, fmt.Errorf("failed to query targets: %w", err)
	}

	targetsMap := make(map[uuid.UUID]*entities.RolloutTarget)
	for _, t := range tDAOs {
		errMsg := ""
		if t.ErrorMessage != nil {
			errMsg = *t.ErrorMessage
		}

		targetsMap[t.ControllerID] = entities.LoadTargetFromDB(
			t.ID,
			t.ControllerID,
			entities.TargetStatus(t.Status),
			errMsg,
		)
	}

	cancelReason := ""
	if cDAO.CancelReason != nil {
		cancelReason = *cDAO.CancelReason
	}

	camp := entities.LoadCampaignFromDB(
		cDAO.ID,
		cDAO.FirmwareID,
		cDAO.Name,
		entities.RolloutCampaignStatus(cDAO.Status),
		cancelReason,
		targetsMap,
	)

	return camp, nil
}

func (r *PostgresCampaignRepository) GetById(ctx context.Context, id uuid.UUID) (*entities.RolloutCampaign, error) {
	queryCamp := `
		SELECT id, firmware_id, name, status, cancel_reason
		FROM rollout_campaigns
		WHERE id = $1
	`
	var cDAO campaignDAO
	err := r.db.GetContext(ctx, &cDAO, queryCamp, id)
	if errors.Is(err, sql.ErrNoRows) {
		return nil, nil
	}
	if err != nil {
		return nil, fmt.Errorf("failed to query campaign by id: %w", err)
	}

	queryTargets := `
		SELECT id, campaign_id, controller_id, status, error_message 
		FROM rollout_targets 
		WHERE campaign_id = $1`
	var tDAOs []targetDAO
	if err := r.db.SelectContext(ctx, &tDAOs, queryTargets, id); err != nil {
		return nil, fmt.Errorf("failed to query targets: %w", err)
	}

	targetsMap := make(map[uuid.UUID]*entities.RolloutTarget)
	for _, t := range tDAOs {
		errMsg := ""
		if t.ErrorMessage != nil {
			errMsg = *t.ErrorMessage
		}
		targetsMap[t.ControllerID] = entities.LoadTargetFromDB(t.ID, t.ControllerID, entities.TargetStatus(t.Status), errMsg)
	}

	cancelReason := ""
	if cDAO.CancelReason != nil {
		cancelReason = *cDAO.CancelReason
	}

	camp := entities.LoadCampaignFromDB(cDAO.ID, cDAO.FirmwareID, cDAO.Name, entities.RolloutCampaignStatus(cDAO.Status), cancelReason, targetsMap)
	return camp, nil
}

func (r *PostgresCampaignRepository) Delete(ctx context.Context, rc *entities.RolloutCampaign) error {
	tx, err := r.db.BeginTxx(ctx, nil)
	if err != nil {
		return fmt.Errorf("failed to begin transaction: %w", err)
	}

	defer tx.Rollback()

	queryTarget := `
		DELETE FROM rollout_targets
		WHERE campaign_id = $1
	`
	if _, err := tx.ExecContext(ctx, queryTarget, rc.ID()); err != nil {
		return fmt.Errorf("failed to delete targets: %w", err)
	}

	queryCamp := `
		DELETE FROM rollout_campaigns 
		WHERE id = $1;	
	`
	if _, err := tx.ExecContext(ctx, queryCamp, rc.ID()); err != nil {
		return fmt.Errorf("failed to delete campaign: %w", err)
	}

	if err := tx.Commit(); err != nil {
		return fmt.Errorf("failed to commit transaction: %w", err)
	}
	return nil
}

func (r *PostgresCampaignRepository) Save(ctx context.Context, campaign *entities.RolloutCampaign) error {
	tx, err := r.db.BeginTxx(ctx, nil)
	if err != nil {
		return fmt.Errorf("failed to begin transaction: %w", err)
	}

	defer tx.Rollback()

	cDAO := campaignDAO{
		ID:         campaign.ID(),
		FirmwareID: campaign.FirmwareId(),
		Name:       campaign.Name(),
		Status:     int(campaign.Status()),
	}

	cancelReason := campaign.CancelReason()
	if cancelReason != "" {
		cDAO.CancelReason = &cancelReason
	}

	queryCamp := `
		INSERT INTO rollout_campaigns (id, firmware_id, name, status, cancel_reason)
		VALUES (:id, :firmware_id, :name, :status, :cancel_reason)
		ON CONFLICT (id) DO UPDATE SET
			name = EXCLUDED.name,
			status = EXCLUDED.status,
			cancel_reason = EXCLUDED.cancel_reason,
			name = EXCLUDED.name
	`
	if _, err := tx.NamedExecContext(ctx, queryCamp, cDAO); err != nil {
		return fmt.Errorf("failed to upsert campaign: %w", err)
	}

	targetsMap := campaign.ListTargets()
	if len(targetsMap) > 0 {
		var tDAOs []targetDAO
		for _, t := range targetsMap {
			tDAO := targetDAO{
				ID:           t.ID(),
				CampaignID:   campaign.ID(),
				ControllerID: t.ControllerId(),
				Status:       int(t.Status()),
			}

			errMsg := t.ErrorMessage()
			if errMsg != "" {
				tDAO.ErrorMessage = &errMsg
			}
			tDAOs = append(tDAOs, tDAO)
		}

		queryTargets := `
			INSERT INTO rollout_targets (id, campaign_id, controller_id, status, error_message)
			VALUES (:id, :campaign_id, :controller_id, :status, :error_message)
			ON CONFLICT (id) DO UPDATE SET
				status = EXCLUDED.status,
				error_message = EXCLUDED.error_message
		`
		if _, err := tx.NamedExecContext(ctx, queryTargets, tDAOs); err != nil {
			return fmt.Errorf("failed to bulk upsert targets: %w", err)
		}
	}

	if err := tx.Commit(); err != nil {
		return fmt.Errorf("failed to commit transaction: %w", err)
	}
	return nil
}
