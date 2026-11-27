package infrastructure

import (
	"context"
	"fmt"
	"strings"
	"time"

	"github.com/google/uuid"
	"github.com/romansevryuk375/aquasmart-firmware/internal/application"
	"github.com/romansevryuk375/aquasmart-firmware/internal/infrastructure/devicegrpc"
	"google.golang.org/grpc"
	"google.golang.org/grpc/credentials/insecure"
)

type GrpcDeviceTokenValidator struct {
	conn   *grpc.ClientConn
	client devicegrpc.DeviceIntegrationGrpcClient
}

func NewGrpcDeviceTokenValidator(rawURL string) (*GrpcDeviceTokenValidator, error) {
	target := normalizeGrpcTarget(rawURL)
	if target == "" {
		return nil, fmt.Errorf("device gRPC URL is empty")
	}

	conn, err := grpc.NewClient(target, grpc.WithTransportCredentials(insecure.NewCredentials()))
	if err != nil {
		return nil, fmt.Errorf("failed to create gRPC client for %s: %w", target, err)
	}

	return &GrpcDeviceTokenValidator{
		conn:   conn,
		client: devicegrpc.NewDeviceIntegrationGrpcClient(conn),
	}, nil
}

func (v *GrpcDeviceTokenValidator) ValidateTokenByControllerID(
	ctx context.Context,
	controllerID uuid.UUID,
	deviceToken string,
) (*application.DeviceAuthResult, error) {
	callCtx := ctx
	var cancel context.CancelFunc
	if _, hasDeadline := ctx.Deadline(); !hasDeadline {
		callCtx, cancel = context.WithTimeout(ctx, 5*time.Second)
		defer cancel()
	}

	resp, err := v.client.ValidateTokenByControllerId(callCtx, &devicegrpc.ValidateTokenByControllerIdRequest{
		ControllerId: controllerID.String(),
		DeviceToken:  deviceToken,
	})
	if err != nil {
		return nil, fmt.Errorf("device auth gRPC call failed: %w", err)
	}

	parsedControllerID, _ := uuid.Parse(resp.GetControllerId())
	parsedUserID, _ := uuid.Parse(resp.GetUserId())

	return &application.DeviceAuthResult{
		IsValid:      resp.GetIsValid(),
		ControllerID: parsedControllerID,
		UserID:       parsedUserID,
	}, nil
}

func (v *GrpcDeviceTokenValidator) Close() error {
	if v.conn != nil {
		return v.conn.Close()
	}
	return nil
}

func normalizeGrpcTarget(raw string) string {
	trimmed := strings.TrimSpace(raw)
	trimmed = strings.TrimPrefix(trimmed, "http://")
	trimmed = strings.TrimPrefix(trimmed, "https://")
	trimmed = strings.TrimSuffix(trimmed, "/")
	return trimmed
}
