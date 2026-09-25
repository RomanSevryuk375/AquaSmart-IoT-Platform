using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTelegramChatIdToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "telegram_chat_id",
                table: "users",
                type: "bigint",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "version",
                value: new Guid("5c994977-84b2-4335-bad2-8dcd0a7f9a59"));

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "version",
                value: new Guid("6f02a937-debb-4ad8-bc75-0a306bfd4ffe"));

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "version",
                value: new Guid("41195810-e36b-43a9-ad8a-c2cfe6b0fde7"));

            migrationBuilder.CreateIndex(
                name: "ix_users_telegram_chat_id",
                table: "users",
                column: "telegram_chat_id",
                unique: true,
                filter: "telegram_chat_id IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_users_telegram_chat_id",
                table: "users");

            migrationBuilder.DropColumn(
                name: "telegram_chat_id",
                table: "users");

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "version",
                value: new Guid("fb13f157-0552-4cb5-b00c-d07d866d387c"));

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "version",
                value: new Guid("c7cf844d-991a-4f28-9c0a-da9aaf3a42dc"));

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "version",
                value: new Guid("4fe111a9-72c4-42dc-8df9-6725cc503be9"));
        }
    }
}
