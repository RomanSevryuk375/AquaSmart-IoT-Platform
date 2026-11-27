using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Device.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMqttControllerMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "config_version",
                table: "controllers",
                type: "bigint",
                nullable: false,
                defaultValue: 1L);

            migrationBuilder.AddColumn<string>(
                name: "firmware_version",
                table: "controllers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ip_address",
                table: "controllers",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "offline_alert_sent",
                table: "controllers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "rssi",
                table: "controllers",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "uptime_sec",
                table: "controllers",
                type: "bigint",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "config_version",
                table: "controllers");

            migrationBuilder.DropColumn(
                name: "firmware_version",
                table: "controllers");

            migrationBuilder.DropColumn(
                name: "ip_address",
                table: "controllers");

            migrationBuilder.DropColumn(
                name: "offline_alert_sent",
                table: "controllers");

            migrationBuilder.DropColumn(
                name: "rssi",
                table: "controllers");

            migrationBuilder.DropColumn(
                name: "uptime_sec",
                table: "controllers");
        }
    }
}
