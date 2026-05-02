using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Control.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreFlisibleSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_automation_rules_aquariums_aquarium_id",
                table: "automation_rules");

            migrationBuilder.DropForeignKey(
                name: "fk_relays_aquariums_aquarium_id",
                table: "relays");

            migrationBuilder.DropForeignKey(
                name: "fk_schedules_aquariums_aquarium_id",
                table: "schedules");

            migrationBuilder.DropForeignKey(
                name: "fk_sensors_aquariums_aquarium_id",
                table: "sensors");

            migrationBuilder.DropForeignKey(
                name: "fk_vacations_aquariums_aquarium_id",
                table: "vacations");

            migrationBuilder.DropTable(
                name: "aquariums");

            migrationBuilder.DropIndex(
                name: "ix_automation_rules_aquarium_id",
                table: "automation_rules");

            migrationBuilder.DropIndex(
                name: "ix_automation_rules_relay_id",
                table: "automation_rules");

            migrationBuilder.DropColumn(
                name: "aquarium_id",
                table: "automation_rules");

            migrationBuilder.DropColumn(
                name: "hysteresis",
                table: "automation_rules");

            migrationBuilder.DropColumn(
                name: "threshold",
                table: "automation_rules");

            migrationBuilder.RenameColumn(
                name: "aquarium_id",
                table: "vacations",
                newName: "ecosystem_id");

            migrationBuilder.RenameIndex(
                name: "ix_vacations_aquarium_id",
                table: "vacations",
                newName: "ix_vacations_ecosystem_id");

            migrationBuilder.RenameColumn(
                name: "aquarium_id",
                table: "sensors",
                newName: "ecosystem_id");

            migrationBuilder.RenameIndex(
                name: "ix_sensors_aquarium_id",
                table: "sensors",
                newName: "ix_sensors_ecosystem_id");

            migrationBuilder.RenameColumn(
                name: "aquarium_id",
                table: "schedules",
                newName: "ecosystem_id");

            migrationBuilder.RenameIndex(
                name: "ix_schedules_aquarium_id",
                table: "schedules",
                newName: "ix_schedules_ecosystem_id");

            migrationBuilder.RenameColumn(
                name: "aquarium_id",
                table: "relays",
                newName: "ecosystem_id");

            migrationBuilder.RenameIndex(
                name: "ix_relays_aquarium_id",
                table: "relays",
                newName: "ix_relays_ecosystem_id");

            migrationBuilder.RenameColumn(
                name: "sensor_id",
                table: "automation_rules",
                newName: "ecosystem_id");

            migrationBuilder.RenameColumn(
                name: "condition",
                table: "automation_rules",
                newName: "operator");

            migrationBuilder.RenameIndex(
                name: "ix_automation_rules_sensor_id",
                table: "automation_rules",
                newName: "ix_automation_rules_ecosystem_id");

            migrationBuilder.AddColumn<Guid>(
                name: "controller_id",
                table: "sensors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<double>(
                name: "last_value",
                table: "sensors",
                type: "double precision",
                precision: 10,
                scale: 4,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "sensors",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "controller_id",
                table: "relays",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

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

            migrationBuilder.AddColumn<bool>(
                name: "is_active",
                table: "automation_rules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "name",
                table: "automation_rules",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ecosystems",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    volume = table.Column<double>(type: "double precision", nullable: true),
                    controller_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_ecosystems", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "rule_conditions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    automation_rule_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sensor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    condition = table.Column<int>(type: "integer", nullable: false),
                    threshold = table.Column<double>(type: "double precision", precision: 10, scale: 4, nullable: false),
                    hysteresis = table.Column<double>(type: "double precision", precision: 10, scale: 4, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_rule_conditions", x => x.id);
                    table.ForeignKey(
                        name: "fk_rule_conditions_automation_rules_automation_rule_id",
                        column: x => x.automation_rule_id,
                        principalTable: "automation_rules",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_relays_power_sensor_id",
                table: "relays",
                column: "power_sensor_id",
                filter: "power_sensor_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_automation_rules_is_active",
                table: "automation_rules",
                column: "is_active");

            migrationBuilder.CreateIndex(
                name: "ix_ecosystems_controller_id",
                table: "ecosystems",
                column: "controller_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_ecosystems_user_id",
                table: "ecosystems",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_rule_conditions_automation_rule_id",
                table: "rule_conditions",
                column: "automation_rule_id");

            migrationBuilder.CreateIndex(
                name: "ix_rule_conditions_sensor_id",
                table: "rule_conditions",
                column: "sensor_id");

            migrationBuilder.AddForeignKey(
                name: "fk_automation_rules_ecosystems_ecosystem_id",
                table: "automation_rules",
                column: "ecosystem_id",
                principalTable: "ecosystems",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_relays_ecosystems_ecosystem_id",
                table: "relays",
                column: "ecosystem_id",
                principalTable: "ecosystems",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_schedules_ecosystems_ecosystem_id",
                table: "schedules",
                column: "ecosystem_id",
                principalTable: "ecosystems",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sensors_ecosystems_ecosystem_id",
                table: "sensors",
                column: "ecosystem_id",
                principalTable: "ecosystems",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vacations_ecosystems_ecosystem_id",
                table: "vacations",
                column: "ecosystem_id",
                principalTable: "ecosystems",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_automation_rules_ecosystems_ecosystem_id",
                table: "automation_rules");

            migrationBuilder.DropForeignKey(
                name: "fk_relays_ecosystems_ecosystem_id",
                table: "relays");

            migrationBuilder.DropForeignKey(
                name: "fk_schedules_ecosystems_ecosystem_id",
                table: "schedules");

            migrationBuilder.DropForeignKey(
                name: "fk_sensors_ecosystems_ecosystem_id",
                table: "sensors");

            migrationBuilder.DropForeignKey(
                name: "fk_vacations_ecosystems_ecosystem_id",
                table: "vacations");

            migrationBuilder.DropTable(
                name: "ecosystems");

            migrationBuilder.DropTable(
                name: "rule_conditions");

            migrationBuilder.DropIndex(
                name: "ix_relays_power_sensor_id",
                table: "relays");

            migrationBuilder.DropIndex(
                name: "ix_automation_rules_is_active",
                table: "automation_rules");

            migrationBuilder.DropColumn(
                name: "controller_id",
                table: "sensors");

            migrationBuilder.DropColumn(
                name: "last_value",
                table: "sensors");

            migrationBuilder.DropColumn(
                name: "name",
                table: "sensors");

            migrationBuilder.DropColumn(
                name: "controller_id",
                table: "relays");

            migrationBuilder.DropColumn(
                name: "name",
                table: "relays");

            migrationBuilder.DropColumn(
                name: "power_sensor_id",
                table: "relays");

            migrationBuilder.DropColumn(
                name: "is_active",
                table: "automation_rules");

            migrationBuilder.DropColumn(
                name: "name",
                table: "automation_rules");

            migrationBuilder.RenameColumn(
                name: "ecosystem_id",
                table: "vacations",
                newName: "aquarium_id");

            migrationBuilder.RenameIndex(
                name: "ix_vacations_ecosystem_id",
                table: "vacations",
                newName: "ix_vacations_aquarium_id");

            migrationBuilder.RenameColumn(
                name: "ecosystem_id",
                table: "sensors",
                newName: "aquarium_id");

            migrationBuilder.RenameIndex(
                name: "ix_sensors_ecosystem_id",
                table: "sensors",
                newName: "ix_sensors_aquarium_id");

            migrationBuilder.RenameColumn(
                name: "ecosystem_id",
                table: "schedules",
                newName: "aquarium_id");

            migrationBuilder.RenameIndex(
                name: "ix_schedules_ecosystem_id",
                table: "schedules",
                newName: "ix_schedules_aquarium_id");

            migrationBuilder.RenameColumn(
                name: "ecosystem_id",
                table: "relays",
                newName: "aquarium_id");

            migrationBuilder.RenameIndex(
                name: "ix_relays_ecosystem_id",
                table: "relays",
                newName: "ix_relays_aquarium_id");

            migrationBuilder.RenameColumn(
                name: "operator",
                table: "automation_rules",
                newName: "condition");

            migrationBuilder.RenameColumn(
                name: "ecosystem_id",
                table: "automation_rules",
                newName: "sensor_id");

            migrationBuilder.RenameIndex(
                name: "ix_automation_rules_ecosystem_id",
                table: "automation_rules",
                newName: "ix_automation_rules_sensor_id");

            migrationBuilder.AddColumn<Guid>(
                name: "aquarium_id",
                table: "automation_rules",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<double>(
                name: "hysteresis",
                table: "automation_rules",
                type: "double precision",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "threshold",
                table: "automation_rules",
                type: "double precision",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "aquariums",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    controller_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_aquariums", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_automation_rules_aquarium_id",
                table: "automation_rules",
                column: "aquarium_id");

            migrationBuilder.CreateIndex(
                name: "ix_automation_rules_relay_id",
                table: "automation_rules",
                column: "relay_id");

            migrationBuilder.CreateIndex(
                name: "ix_aquariums_controller_id",
                table: "aquariums",
                column: "controller_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_aquariums_user_id",
                table: "aquariums",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_automation_rules_aquariums_aquarium_id",
                table: "automation_rules",
                column: "aquarium_id",
                principalTable: "aquariums",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_relays_aquariums_aquarium_id",
                table: "relays",
                column: "aquarium_id",
                principalTable: "aquariums",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_schedules_aquariums_aquarium_id",
                table: "schedules",
                column: "aquarium_id",
                principalTable: "aquariums",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_sensors_aquariums_aquarium_id",
                table: "sensors",
                column: "aquarium_id",
                principalTable: "aquariums",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_vacations_aquariums_aquarium_id",
                table: "vacations",
                column: "aquarium_id",
                principalTable: "aquariums",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
