package presentation

import (
	"bytes"
	"context"
	"encoding/json"
	"errors"
	"io"
	"log/slog"
	"net/http"
	"strings"

	"github.com/google/uuid"
	"github.com/romansevryuk375/aquasmart-firmware/internal/application"
)

const DeviceTokenHeader = "X-Device-Token"

var errControllerIDMismatch = errors.New("controller_id mismatch between query and body")

type contextKey string

const authenticatedControllerIDKey contextKey = "authenticated_controller_id"

func AuthenticatedControllerIDFromContext(ctx context.Context) (uuid.UUID, bool) {
	id, ok := ctx.Value(authenticatedControllerIDKey).(uuid.UUID)
	return id, ok && id != uuid.Nil
}

func DeviceAuthMiddleware(validator application.DeviceTokenValidator, next http.HandlerFunc) http.HandlerFunc {
	return func(w http.ResponseWriter, r *http.Request) {
		deviceToken := strings.TrimSpace(r.Header.Get(DeviceTokenHeader))
		if deviceToken == "" {
			http.Error(w, "missing X-Device-Token header", http.StatusUnauthorized)
			return
		}

		controllerID, err := extractControllerID(r)
		if errors.Is(err, errControllerIDMismatch) {
			http.Error(w, "forbidden: controller_id mismatch", http.StatusForbidden)
			return
		}
		if err != nil || controllerID == uuid.Nil {
			http.Error(w, "invalid or missing controller_id", http.StatusBadRequest)
			return
		}

		if validator == nil {
			slog.Error("DeviceAuthMiddleware misconfigured: validator is nil")
			http.Error(w, "device authentication service unavailable", http.StatusServiceUnavailable)
			return
		}

		authRes, err := validator.ValidateTokenByControllerID(r.Context(), controllerID, deviceToken)
		if err != nil {
			slog.Warn("Device auth gRPC call failed (fail-closed)", "controller_id", controllerID, "error", err)
			http.Error(w, "device authentication service unavailable", http.StatusServiceUnavailable)
			return
		}

		if authRes == nil || !authRes.IsValid || (authRes.ControllerID != uuid.Nil && authRes.ControllerID != controllerID) {
			slog.Warn("Device auth rejected", "controller_id", controllerID)
			http.Error(w, "forbidden: invalid device token for controller", http.StatusForbidden)
			return
		}

		ctx := context.WithValue(r.Context(), authenticatedControllerIDKey, controllerID)
		next(w, r.WithContext(ctx))
	}
}

func extractControllerID(r *http.Request) (uuid.UUID, error) {
	queryIDStr := strings.TrimSpace(r.URL.Query().Get("controller_id"))

	if r.Method == http.MethodGet || r.Method == http.MethodHead || r.Body == nil {
		if queryIDStr == "" {
			return uuid.Nil, io.EOF
		}
		return uuid.Parse(queryIDStr)
	}

	limited := io.LimitReader(r.Body, 1*1024*1024+1)
	rawBody, err := io.ReadAll(limited)
	if err != nil {
		return uuid.Nil, err
	}
	r.Body = io.NopCloser(bytes.NewReader(rawBody))

	var body struct {
		ControllerID string `json:"controller_id"`
	}
	if err := json.Unmarshal(rawBody, &body); err != nil {
		return uuid.Nil, err
	}

	bodyID, err := uuid.Parse(strings.TrimSpace(body.ControllerID))
	if err != nil {
		return uuid.Nil, err
	}

	if queryIDStr != "" {
		queryID, err := uuid.Parse(queryIDStr)
		if err != nil {
			return uuid.Nil, err
		}
		if queryID != bodyID {
			return uuid.Nil, errControllerIDMismatch
		}
	}

	return bodyID, nil
}
