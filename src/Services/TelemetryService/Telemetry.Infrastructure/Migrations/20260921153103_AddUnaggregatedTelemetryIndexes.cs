using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemetry.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUnaggregatedTelemetryIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "ix_telemetry_raw_data_is_aggregated_recorded_at",
                table: "telemetry_raw_data",
                columns: new[] { "is_aggregated", "recorded_at" });

            migrationBuilder.CreateIndex(
                name: "ix_telemetry_aggregate_data_period_is_aggregated_period_start",
                table: "telemetry_aggregate_data",
                columns: new[] { "period", "is_aggregated", "period_start" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_telemetry_raw_data_is_aggregated_recorded_at",
                table: "telemetry_raw_data");

            migrationBuilder.DropIndex(
                name: "ix_telemetry_aggregate_data_period_is_aggregated_period_start",
                table: "telemetry_aggregate_data");
        }
    }
}
