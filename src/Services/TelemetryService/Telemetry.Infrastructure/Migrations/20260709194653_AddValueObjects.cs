using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Telemetry.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddValueObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "min_value",
                table: "telemetry_aggregate_data",
                newName: "summary_min_value");

            migrationBuilder.RenameColumn(
                name: "max_value",
                table: "telemetry_aggregate_data",
                newName: "summary_max_value");

            migrationBuilder.RenameColumn(
                name: "avg_value",
                table: "telemetry_aggregate_data",
                newName: "summary_avg_value");

            migrationBuilder.RenameColumn(
                name: "data_points_count",
                table: "telemetry_aggregate_data",
                newName: "summary_count");

            migrationBuilder.AddColumn<Guid>(
                name: "version",
                table: "telemetry_raw_data",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "version",
                table: "telemetry_aggregate_data",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "version",
                table: "sensors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "version",
                table: "ecosystems",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "version",
                table: "telemetry_raw_data");

            migrationBuilder.DropColumn(
                name: "version",
                table: "telemetry_aggregate_data");

            migrationBuilder.DropColumn(
                name: "version",
                table: "sensors");

            migrationBuilder.DropColumn(
                name: "version",
                table: "ecosystems");

            migrationBuilder.RenameColumn(
                name: "summary_min_value",
                table: "telemetry_aggregate_data",
                newName: "min_value");

            migrationBuilder.RenameColumn(
                name: "summary_max_value",
                table: "telemetry_aggregate_data",
                newName: "max_value");

            migrationBuilder.RenameColumn(
                name: "summary_avg_value",
                table: "telemetry_aggregate_data",
                newName: "avg_value");

            migrationBuilder.RenameColumn(
                name: "summary_count",
                table: "telemetry_aggregate_data",
                newName: "data_points_count");
        }
    }
}
