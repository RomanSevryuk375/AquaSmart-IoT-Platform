using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemetry.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ecosystems",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    controller_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ecosystems", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "jsonb", nullable: false),
                    occurred_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    processed_on_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_outbox_messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sensors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    controller_id = table.Column<Guid>(type: "uuid", nullable: false),
                    ecosystem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    unit = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    last_value = table.Column<double>(type: "double precision", precision: 10, scale: 4, nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_data_delayed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sensors", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "telemetry_aggregate_data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sensor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    period_start = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    period = table.Column<int>(type: "integer", nullable: false),
                    min_value = table.Column<double>(type: "double precision", precision: 10, scale: 4, nullable: false),
                    max_value = table.Column<double>(type: "double precision", precision: 10, scale: 4, nullable: false),
                    avg_value = table.Column<double>(type: "double precision", precision: 10, scale: 4, nullable: false),
                    data_points_count = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_aggregated = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_telemetry_aggregate_data", x => x.id);
                    table.ForeignKey(
                        name: "fk_telemetry_aggregate_data_sensors_sensor_id",
                        column: x => x.sensor_id,
                        principalTable: "sensors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "telemetry_raw_data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    sensor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<double>(type: "double precision", precision: 10, scale: 4, nullable: false),
                    external_message_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_aggregated = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_telemetry_raw_data", x => x.id);
                    table.ForeignKey(
                        name: "fk_telemetry_raw_data_sensors_sensor_id",
                        column: x => x.sensor_id,
                        principalTable: "sensors",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_ecosystems_controller_id",
                table: "ecosystems",
                column: "controller_id");

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_occurred_on_utc",
                table: "outbox_messages",
                column: "occurred_on_utc");

            migrationBuilder.CreateIndex(
                name: "ix_sensors_ecosystem_id",
                table: "sensors",
                column: "ecosystem_id");

            migrationBuilder.CreateIndex(
                name: "ix_telemetry_aggregate_data_is_aggregated",
                table: "telemetry_aggregate_data",
                column: "is_aggregated");

            migrationBuilder.CreateIndex(
                name: "ix_telemetry_aggregate_data_sensor_id_period_period_start",
                table: "telemetry_aggregate_data",
                columns: new[] { "sensor_id", "period", "period_start" });

            migrationBuilder.CreateIndex(
                name: "ix_telemetry_raw_data_external_message_id",
                table: "telemetry_raw_data",
                column: "external_message_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_telemetry_raw_data_is_aggregated",
                table: "telemetry_raw_data",
                column: "is_aggregated");

            migrationBuilder.CreateIndex(
                name: "ix_telemetry_raw_data_sensor_id_recorded_at",
                table: "telemetry_raw_data",
                columns: new[] { "sensor_id", "recorded_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ecosystems");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "telemetry_aggregate_data");

            migrationBuilder.DropTable(
                name: "telemetry_raw_data");

            migrationBuilder.DropTable(
                name: "sensors");
        }
    }
}
