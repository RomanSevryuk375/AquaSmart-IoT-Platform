using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Device.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRelayCommandsQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_relays_controller_id_hardware_pin",
                table: "relays");

            migrationBuilder.RenameColumn(
                name: "hardware_pin",
                table: "sensors",
                newName: "connection_address");

            migrationBuilder.RenameIndex(
                name: "ix_sensors_controller_id_hardware_pin",
                table: "sensors",
                newName: "ix_sensors_controller_id_connection_address");

            migrationBuilder.RenameColumn(
                name: "hardware_pin",
                table: "relays",
                newName: "connection_address");

            migrationBuilder.AlterColumn<string>(
                name: "unit",
                table: "sensors",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AddColumn<int>(
                name: "connection_protocol",
                table: "sensors",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "sensors",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "connection_protocol",
                table: "relays",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "is_normaly_open",
                table: "relays",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "relays",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "power_sensor_id",
                table: "relays",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "relay_command_queues",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    controller_id = table.Column<Guid>(type: "uuid", nullable: false),
                    relay_id = table.Column<Guid>(type: "uuid", nullable: false),
                    action = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    expire_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    attempt_count = table.Column<int>(type: "integer", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_relay_command_queues", x => x.id);
                    table.ForeignKey(
                        name: "fk_relay_command_queues_controllers_controller_id",
                        column: x => x.controller_id,
                        principalTable: "controllers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_relay_command_queues_relays_relay_id",
                        column: x => x.relay_id,
                        principalTable: "relays",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_relays_controller_id_power_sensor_id_connection_address",
                table: "relays",
                columns: new[] { "controller_id", "power_sensor_id", "connection_address" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_relays_power_sensor_id",
                table: "relays",
                column: "power_sensor_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_relay_command_queues_controller_id_status",
                table: "relay_command_queues",
                columns: new[] { "controller_id", "status" });

            migrationBuilder.CreateIndex(
                name: "ix_relay_command_queues_relay_id",
                table: "relay_command_queues",
                column: "relay_id");

            migrationBuilder.AddForeignKey(
                name: "fk_relays_sensors_power_sensor_id",
                table: "relays",
                column: "power_sensor_id",
                principalTable: "sensors",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_relays_sensors_power_sensor_id",
                table: "relays");

            migrationBuilder.DropTable(
                name: "relay_command_queues");

            migrationBuilder.DropIndex(
                name: "ix_relays_controller_id_power_sensor_id_connection_address",
                table: "relays");

            migrationBuilder.DropIndex(
                name: "ix_relays_power_sensor_id",
                table: "relays");

            migrationBuilder.DropColumn(
                name: "connection_protocol",
                table: "sensors");

            migrationBuilder.DropColumn(
                name: "name",
                table: "sensors");

            migrationBuilder.DropColumn(
                name: "connection_protocol",
                table: "relays");

            migrationBuilder.DropColumn(
                name: "is_normaly_open",
                table: "relays");

            migrationBuilder.DropColumn(
                name: "name",
                table: "relays");

            migrationBuilder.DropColumn(
                name: "power_sensor_id",
                table: "relays");

            migrationBuilder.RenameColumn(
                name: "connection_address",
                table: "sensors",
                newName: "hardware_pin");

            migrationBuilder.RenameIndex(
                name: "ix_sensors_controller_id_connection_address",
                table: "sensors",
                newName: "ix_sensors_controller_id_hardware_pin");

            migrationBuilder.RenameColumn(
                name: "connection_address",
                table: "relays",
                newName: "hardware_pin");

            migrationBuilder.AlterColumn<string>(
                name: "unit",
                table: "sensors",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(32)",
                oldMaxLength: 32);

            migrationBuilder.CreateIndex(
                name: "ix_relays_controller_id_hardware_pin",
                table: "relays",
                columns: new[] { "controller_id", "hardware_pin" },
                unique: true);
        }
    }
}
