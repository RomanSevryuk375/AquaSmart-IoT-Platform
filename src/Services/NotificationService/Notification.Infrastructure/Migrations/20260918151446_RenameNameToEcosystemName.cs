using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameNameToEcosystemName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "name",
                table: "ecosystems",
                newName: "ecosystem_name");

            migrationBuilder.AlterColumn<string>(
                name: "time_zone",
                table: "users",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ecosystem_name",
                table: "ecosystems",
                newName: "name");

            migrationBuilder.AlterColumn<string>(
                name: "time_zone",
                table: "users",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);
        }
    }
}
