using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Device.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "sensors",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "relays",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_sensors_user_id",
                table: "sensors",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_relays_user_id",
                table: "relays",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_sensors_user_id",
                table: "sensors");

            migrationBuilder.DropIndex(
                name: "ix_relays_user_id",
                table: "relays");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "sensors");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "relays");
        }
    }
}
