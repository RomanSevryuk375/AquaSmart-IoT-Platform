package presentation

import (
	"bytes"
	"context"
	"errors"
	"io"
	"net/http"
	"net/http/httptest"
	"testing"

	"github.com/google/uuid"
	"github.com/romansevryuk375/aquasmart-firmware/internal/application"
)

type mockDeviceTokenValidator struct {
	validateFn func(ctx context.Context, controllerID uuid.UUID, deviceToken string) (*application.DeviceAuthResult, error)
}

func (m *mockDeviceTokenValidator) ValidateTokenByControllerID(
	ctx context.Context,
	controllerID uuid.UUID,
	deviceToken string,
) (*application.DeviceAuthResult, error) {
	return m.validateFn(ctx, controllerID, deviceToken)
}

func TestDeviceAuthMiddleware_MissingToken_Returns401(t *testing.T) {
	controllerID := uuid.New()
	called := false

	validator := &mockDeviceTokenValidator{
		validateFn: func(ctx context.Context, id uuid.UUID, token string) (*application.DeviceAuthResult, error) {
			t.Fatal("validator must not be called when token is missing")
			return nil, nil
		},
	}

	handler := DeviceAuthMiddleware(validator, func(w http.ResponseWriter, r *http.Request) {
		called = true
		w.WriteHeader(http.StatusOK)
	})

	req := httptest.NewRequest(http.MethodGet, "/api/firmware/v1/ota/check?controller_id="+controllerID.String(), nil)
	rec := httptest.NewRecorder()

	handler.ServeHTTP(rec, req)

	if rec.Code != http.StatusUnauthorized {
		t.Fatalf("expected 401 Unauthorized, got %d", rec.Code)
	}
	if called {
		t.Fatal("next handler must not be called")
	}
}

func TestDeviceAuthMiddleware_InvalidControllerID_Returns400(t *testing.T) {
	validator := &mockDeviceTokenValidator{
		validateFn: func(ctx context.Context, id uuid.UUID, token string) (*application.DeviceAuthResult, error) {
			t.Fatal("validator must not be called for invalid controller_id")
			return nil, nil
		},
	}

	handler := DeviceAuthMiddleware(validator, func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusOK)
	})

	cases := []struct {
		name   string
		method string
		url    string
		body   string
	}{
		{name: "query not uuid", method: http.MethodGet, url: "/api/firmware/v1/ota/check?controller_id=not-a-uuid"},
		{name: "query missing", method: http.MethodGet, url: "/api/firmware/v1/ota/check"},
		{name: "body not uuid", method: http.MethodPost, url: "/api/firmware/v1/ota/status", body: `{"controller_id":"bad"}`},
		{name: "body invalid json", method: http.MethodPost, url: "/api/firmware/v1/ota/status", body: `{bad-json`},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			req := httptest.NewRequest(tc.method, tc.url, bytes.NewBufferString(tc.body))
			req.Header.Set(DeviceTokenHeader, "ak_valid_token")
			rec := httptest.NewRecorder()

			handler.ServeHTTP(rec, req)

			if rec.Code != http.StatusBadRequest {
				t.Fatalf("expected 400 Bad Request, got %d", rec.Code)
			}
		})
	}
}

func TestDeviceAuthMiddleware_InvalidTokenOrIDOR_Returns403(t *testing.T) {
	controllerA := uuid.New()
	controllerB := uuid.New()

	cases := []struct {
		name string
		res  *application.DeviceAuthResult
	}{
		{name: "is_valid false", res: &application.DeviceAuthResult{IsValid: false}},
		{name: "mismatched controller id (IDOR)", res: &application.DeviceAuthResult{IsValid: true, ControllerID: controllerA}},
	}

	for _, tc := range cases {
		t.Run(tc.name, func(t *testing.T) {
			called := false
			validator := &mockDeviceTokenValidator{
				validateFn: func(ctx context.Context, id uuid.UUID, token string) (*application.DeviceAuthResult, error) {
					return tc.res, nil
				},
			}

			handler := DeviceAuthMiddleware(validator, func(w http.ResponseWriter, r *http.Request) {
				called = true
				w.WriteHeader(http.StatusOK)
			})

			req := httptest.NewRequest(http.MethodGet, "/api/firmware/v1/ota/check?controller_id="+controllerB.String(), nil)
			req.Header.Set(DeviceTokenHeader, "ak_controller_a_token")
			rec := httptest.NewRecorder()

			handler.ServeHTTP(rec, req)

			if rec.Code != http.StatusForbidden {
				t.Fatalf("expected 403 Forbidden, got %d", rec.Code)
			}
			if called {
				t.Fatal("next handler must not be called on IDOR / invalid token")
			}
		})
	}
}

func TestDeviceAuthMiddleware_GrpcUnavailable_Returns503FailClosed(t *testing.T) {
	controllerID := uuid.New()
	called := false

	validator := &mockDeviceTokenValidator{
		validateFn: func(ctx context.Context, id uuid.UUID, token string) (*application.DeviceAuthResult, error) {
			return nil, errors.New("rpc error: code = Unavailable desc = connection refused")
		},
	}

	handler := DeviceAuthMiddleware(validator, func(w http.ResponseWriter, r *http.Request) {
		called = true
		w.WriteHeader(http.StatusOK)
	})

	req := httptest.NewRequest(http.MethodGet, "/api/firmware/v1/ota/check?controller_id="+controllerID.String(), nil)
	req.Header.Set(DeviceTokenHeader, "ak_valid_token")
	rec := httptest.NewRecorder()

	handler.ServeHTTP(rec, req)

	if rec.Code != http.StatusServiceUnavailable {
		t.Fatalf("expected 503 Service Unavailable, got %d", rec.Code)
	}
	if called {
		t.Fatal("next handler must not be called when gRPC is unavailable")
	}
}

func TestDeviceAuthMiddleware_ValidToken_AllowsGetAndPreservesPostBody(t *testing.T) {
	controllerID := uuid.New()
	userID := uuid.New()
	expectedToken := "ak_controller_secret"

	validator := &mockDeviceTokenValidator{
		validateFn: func(ctx context.Context, id uuid.UUID, token string) (*application.DeviceAuthResult, error) {
			if id != controllerID || token != expectedToken {
				return &application.DeviceAuthResult{IsValid: false}, nil
			}
			return &application.DeviceAuthResult{
				IsValid:      true,
				ControllerID: controllerID,
				UserID:       userID,
			}, nil
		},
	}

	// GET with query param
	getCalled := false
	getHandler := DeviceAuthMiddleware(validator, func(w http.ResponseWriter, r *http.Request) {
		getCalled = true
		authID, ok := AuthenticatedControllerIDFromContext(r.Context())
		if !ok || authID != controllerID {
			t.Fatalf("expected authenticated controller %s in context, got %v (ok=%v)", controllerID, authID, ok)
		}
		w.WriteHeader(http.StatusOK)
	})

	getReq := httptest.NewRequest(http.MethodGet, "/api/firmware/v1/ota/check?controller_id="+controllerID.String(), nil)
	getReq.Header.Set(DeviceTokenHeader, expectedToken)
	getRec := httptest.NewRecorder()
	getHandler.ServeHTTP(getRec, getReq)

	if getRec.Code != http.StatusOK || !getCalled {
		t.Fatalf("expected GET 200 OK and next called, got %d (called=%v)", getRec.Code, getCalled)
	}

	// POST with JSON body — verify body is restored for downstream handler
	rawPayload := `{"campaign_id":"` + uuid.New().String() + `","controller_id":"` + controllerID.String() + `","status":1}`
	var downstreamBody string
	postHandler := DeviceAuthMiddleware(validator, func(w http.ResponseWriter, r *http.Request) {
		b, err := io.ReadAll(r.Body)
		if err != nil {
			t.Fatalf("failed reading restored body: %v", err)
		}
		downstreamBody = string(b)
		w.WriteHeader(http.StatusNoContent)
	})

	postReq := httptest.NewRequest(http.MethodPost, "/api/firmware/v1/ota/status", bytes.NewBufferString(rawPayload))
	postReq.Header.Set(DeviceTokenHeader, expectedToken)
	postRec := httptest.NewRecorder()
	postHandler.ServeHTTP(postRec, postReq)

	if postRec.Code != http.StatusNoContent {
		t.Fatalf("expected POST 204 No Content, got %d", postRec.Code)
	}
	if downstreamBody != rawPayload {
		t.Fatalf("expected downstream body %q, got %q", rawPayload, downstreamBody)
	}
}

func TestDeviceAuthMiddleware_QueryAndBodyMismatch_Returns403WithoutCallingNext(t *testing.T) {
	controllerA := uuid.New()
	controllerB := uuid.New()
	called := false

	validator := &mockDeviceTokenValidator{
		validateFn: func(ctx context.Context, id uuid.UUID, token string) (*application.DeviceAuthResult, error) {
			if id == controllerA {
				return &application.DeviceAuthResult{IsValid: true, ControllerID: controllerA}, nil
			}
			return &application.DeviceAuthResult{IsValid: false}, nil
		},
	}

	handler := DeviceAuthMiddleware(validator, func(w http.ResponseWriter, r *http.Request) {
		called = true
		w.WriteHeader(http.StatusNoContent)
	})

	body := `{"campaign_id":"` + uuid.New().String() + `","controller_id":"` + controllerB.String() + `","status":4}`
	req := httptest.NewRequest(http.MethodPost, "/api/firmware/v1/ota/status?controller_id="+controllerA.String(), bytes.NewBufferString(body))
	req.Header.Set(DeviceTokenHeader, "ak_controller_a_token")
	rec := httptest.NewRecorder()

	handler.ServeHTTP(rec, req)

	if rec.Code != http.StatusForbidden {
		t.Fatalf("expected 403 Forbidden on query/body controller_id mismatch, got %d", rec.Code)
	}
	if called {
		t.Fatal("next handler must not be called when query and body controller_id differ")
	}
}
