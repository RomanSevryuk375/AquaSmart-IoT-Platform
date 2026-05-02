using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Device.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "controllers",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    mac_address = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: false),
                    device_token_hash = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    is_online = table.Column<bool>(type: "boolean", nullable: false),
                    last_seen_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_controllers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "relays",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    controller_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hardware_pin = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    purpose = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_manual = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_relays", x => x.id);
                    table.ForeignKey(
                        name: "fk_relays_controllers_controller_id",
                        column: x => x.controller_id,
                        principalTable: "controllers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "sensors",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    controller_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hardware_pin = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    state = table.Column<int>(type: "integer", nullable: false),
                    unit = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_sensors", x => x.id);
                    table.ForeignKey(
                        name: "fk_sensors_controllers_controller_id",
                        column: x => x.controller_id,
                        principalTable: "controllers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_controllers_device_token_hash",
                table: "controllers",
                column: "device_token_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_controllers_mac_address",
                table: "controllers",
                column: "mac_address",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_controllers_user_id",
                table: "controllers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_relays_controller_id_hardware_pin",
                table: "relays",
                columns: new[] { "controller_id", "hardware_pin" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_sensors_controller_id_hardware_pin",
                table: "sensors",
                columns: new[] { "controller_id", "hardware_pin" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "relays");

            migrationBuilder.DropTable(
                name: "sensors");

            migrationBuilder.DropTable(
                name: "controllers");
        }
    }
}
