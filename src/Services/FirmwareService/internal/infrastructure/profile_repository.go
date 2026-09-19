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

type profileDAO struct {
	ID            uuid.UUID `db:"id"`
	Name          string    `db:"name"`
	BoardRevision string    `db:"board_revision"`
	IsDeprecated  bool      `db:"is_deprecated"`
}

type PostgresHardwareProfileRepository struct {
	db *sqlx.DB
}

func NewPostgresHardwareProfileRepository(db *sqlx.DB) *PostgresHardwareProfileRepository {
	return &PostgresHardwareProfileRepository{db: db}
}

func (r *PostgresHardwareProfileRepository) GetById(ctx context.Context, id uuid.UUID) (*entities.HardwareProfile, error) {
	query := `
		SELECT p.id, p.name, p.board_revision, p.is_deprecated
		FROM hardware_profiles p
		WHERE p.id = $1
	`
	var dao profileDAO
	err := r.db.GetContext(ctx, &dao, query, id)
	if errors.Is(err, sql.ErrNoRows) {
		return nil, nil
	}
	if err != nil {
		return nil, fmt.Errorf("failed to query profile by id %w not found", err)
	}

	profile := entities.LoadHardwareProfileFromDB(dao.ID, dao.Name, dao.BoardRevision, dao.IsDeprecated)
	return profile, nil
}

func (r *PostgresHardwareProfileRepository) Save(ctx context.Context, hp *entities.HardwareProfile) error {
	dao := profileDAO{
		ID:            hp.ID(),
		Name:          hp.Name(),
		BoardRevision: hp.BoardRevision(),
		IsDeprecated:  hp.IsDeprecated(),
	}

	query := `
		INSERT INTO hardware_profiles (id, name, board_revision, is_deprecated)
		VALUES (:id, :name, :board_revision, :is_deprecated)
		ON CONFLICT (id) DO UPDATE SET
			name = EXCLUDED.name,
			board_revision = EXCLUDED.board_revision,
			is_deprecated = EXCLUDED.is_deprecated
	`
	if _, err := r.db.NamedExecContext(ctx, query, dao); err != nil {
		return fmt.Errorf("failed to upsert hardware profile: %w", err)
	}

	return nil
}
