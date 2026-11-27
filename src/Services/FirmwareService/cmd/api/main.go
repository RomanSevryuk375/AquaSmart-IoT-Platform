package main

import (
	"context"
	"errors"
	"log/slog"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/aws/aws-sdk-go-v2/config"
	"github.com/aws/aws-sdk-go-v2/service/s3"
	_ "github.com/jackc/pgx/v5/stdlib"
	"github.com/jmoiron/sqlx"

	"github.com/romansevryuk375/aquasmart-firmware/internal/application"
	"github.com/romansevryuk375/aquasmart-firmware/internal/infrastructure"
	"github.com/romansevryuk375/aquasmart-firmware/internal/presentation"

	_ "github.com/romansevryuk375/aquasmart-firmware/docs"
	httpSwagger "github.com/swaggo/http-swagger/v2"
)

func main() {
	logger := slog.New(slog.NewJSONHandler(os.Stdout, &slog.HandlerOptions{Level: slog.LevelInfo}))
	slog.SetDefault(logger)

	port := os.Getenv("PORT")
	if port == "" {
		port = "8080"
	}

	logger.Info("Connecting to database...")
	dbUrl := os.Getenv("DATABASE_URL")
	if dbUrl == "" {
		dbUrl = "postgres://postgres:password@localhost:5432/firmware_db?sslmode=disable"
	}
	db, err := sqlx.Connect("pgx", dbUrl)
	if err != nil {
		logger.Error("Failed to connect to database", "error", err)
		os.Exit(1)
	}
	defer db.Close()

	db.SetMaxOpenConns(25)
	db.SetMaxIdleConns(25)
	db.SetConnMaxLifetime(5 * time.Minute)

	logger.Info("Configuring AWS S3 client...")
	s3Bucket := os.Getenv("S3_BUCKET_NAME")
	if s3Bucket == "" {
		s3Bucket = "aquasmart-firmware-bucket"
	}
	awsCfg, err := config.LoadDefaultConfig(context.Background())
	if err != nil {
		logger.Error("Failed to load AWS config", "error", err)
		os.Exit(1)
	}
	s3Client := s3.NewFromConfig(awsCfg, func(o *s3.Options) {
		o.UsePathStyle = true
	})

	deviceGrpcURL := os.Getenv("DEVICE_GRPC_URL")
	if deviceGrpcURL == "" {
		deviceGrpcURL = "device-api:50051"
	}
	logger.Info("Configuring DeviceService gRPC auth client...", "target", deviceGrpcURL)
	deviceTokenValidator, err := infrastructure.NewGrpcDeviceTokenValidator(deviceGrpcURL)
	if err != nil {
		logger.Error("Failed to initialize DeviceService gRPC client", "error", err)
		os.Exit(1)
	}
	defer deviceTokenValidator.Close()

	blobStorage := infrastructure.NewS3BlobStorage(s3Client, s3Bucket)
	hwRepo := infrastructure.NewPostgresHardwareProfileRepository(db)
	fwRepo := infrastructure.NewPostgresFirmwareRepository(db)
	campRepo := infrastructure.NewPostgresCampaignRepository(db)

	hwService := application.NewHardwareProfileService(hwRepo)
	fwService := application.NewFirmwareService(fwRepo, blobStorage)
	campService := application.NewCampaignService(fwRepo, campRepo)
	otaService := application.NewOtaDistributionService(campRepo, fwRepo, blobStorage)

	hwHandler := presentation.NewHardwareProfileHandler(*hwService)
	fwHandler := presentation.NewFirmwareReleasesHandler(fwService)
	campHandler := presentation.NewCampaignHandler(campService)
	otaHandler := presentation.NewOTAHandler(otaService, campService)

	mux := http.NewServeMux()

	mux.HandleFunc("GET /swagger/", httpSwagger.Handler(
		httpSwagger.URL("/swagger/doc.json"),
	))

	// --- Device API (ESP32, authenticated via X-Device-Token + DeviceService gRPC) ---
	mux.HandleFunc("GET /api/firmware/v1/ota/check", presentation.DeviceAuthMiddleware(deviceTokenValidator, otaHandler.CheckUpdate))
	mux.HandleFunc("GET /api/firmware/v1/ota/download/{campaignId}", presentation.DeviceAuthMiddleware(deviceTokenValidator, otaHandler.HandleDownloadFirmware))
	mux.HandleFunc("POST /api/firmware/v1/ota/status", presentation.DeviceAuthMiddleware(deviceTokenValidator, otaHandler.HandleUpdateTargetStatus))

	// --- Admin API ---
	// Hardware Profiles
	mux.HandleFunc("POST /api/firmware/v1/hardware-profiles", hwHandler.HandleCreateProfile)
	mux.HandleFunc("PUT /api/firmware/v1/hardware-profiles/{id}/name", hwHandler.HandleRenameProfile)
	mux.HandleFunc("POST /api/firmware/v1/hardware-profiles/{id}/deprecate", hwHandler.HandleProfileDeprecate)

	// Firmware Releases
	mux.HandleFunc("POST /api/firmware/v1/firmwares", fwHandler.HandleUploadFirmware)
	mux.HandleFunc("POST /api/firmware/v1/firmwares/{id}/publish", fwHandler.HandlePublishFirmware)
	mux.HandleFunc("POST /api/firmware/v1/firmwares/{id}/revoke", fwHandler.HandleRevokeFirmware)

	// Campaigns
	mux.HandleFunc("POST /api/firmware/v1/campaigns", campHandler.HandleCreateCampaign)
	mux.HandleFunc("POST /api/firmware/v1/campaigns/{id}/targets", campHandler.HandleAddTargets)
	mux.HandleFunc("POST /api/firmware/v1/campaigns/{id}/start", campHandler.HandleStartCampaign)
	mux.HandleFunc("POST /api/firmware/v1/campaigns/{id}/pause", campHandler.HandlePauseCampaign)
	mux.HandleFunc("POST /api/firmware/v1/campaigns/{id}/cancel", campHandler.HandleCancelCampaign)
	mux.HandleFunc("DELETE /api/firmware/v1/campaigns/{id}", campHandler.HandleDeleteCampaign)

	mux.HandleFunc("GET /health", func(w http.ResponseWriter, r *http.Request) {
		w.WriteHeader(http.StatusOK)
		w.Write([]byte("OK"))
	})

	srv := &http.Server{
		Addr:         ":" + port,
		Handler:      mux,
		ReadTimeout:  15 * time.Second,
		WriteTimeout: 120 * time.Second,
		IdleTimeout:  120 * time.Second,
	}

	stopChan := make(chan os.Signal, 1)
	signal.Notify(stopChan, os.Interrupt, syscall.SIGTERM)

	go func() {
		logger.Info("Starting Firmware Service", "port", port)
		if err := srv.ListenAndServe(); err != nil && !errors.Is(err, http.ErrServerClosed) {
			logger.Error("Server error", "error", err)
			os.Exit(1)
		}
	}()

	<-stopChan
	logger.Info("Shutting down server gracefully...")

	ctx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	if err := srv.Shutdown(ctx); err != nil {
		logger.Error("Server forced to shutdown", "error", err)
	}

	logger.Info("Server stopped")
}
