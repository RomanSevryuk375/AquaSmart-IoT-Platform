using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Device.Infrastructure.Migrations;

/// <inheritdoc />
public partial class AddValueObjects : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "connection_protocol",
            table: "sensors");

        migrationBuilder.DropColumn(
            name: "connection_protocol",
            table: "relays");

        migrationBuilder.DropColumn(
            name: "action",
            table: "relay_command_queues");

        migrationBuilder.AlterColumn<string>(
            name: "connection_address",
            table: "sensors",
            type: "character varying(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(32)",
            oldMaxLength: 32);

        migrationBuilder.AlterColumn<string>(
            name: "connection_address",
            table: "relays",
            type: "character varying(64)",
            maxLength: 64,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(32)",
            oldMaxLength: 32);

        migrationBuilder.AddColumn<bool>(
            name: "targe_state",
            table: "relay_command_queues",
            type: "boolean",
            nullable: false,
            defaultValue: false);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "targe_state",
            table: "relay_command_queues");

        migrationBuilder.AlterColumn<string>(
            name: "connection_address",
            table: "sensors",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(64)",
            oldMaxLength: 64);

        migrationBuilder.AddColumn<int>(
            name: "connection_protocol",
            table: "sensors",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AlterColumn<string>(
            name: "connection_address",
            table: "relays",
            type: "character varying(32)",
            maxLength: 32,
            nullable: false,
            oldClrType: typeof(string),
            oldType: "character varying(64)",
            oldMaxLength: 64);

        migrationBuilder.AddColumn<int>(
            name: "connection_protocol",
            table: "relays",
            type: "integer",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<int>(
            name: "action",
            table: "relay_command_queues",
            type: "integer",
            nullable: false,
            defaultValue: 0);
    }
}
