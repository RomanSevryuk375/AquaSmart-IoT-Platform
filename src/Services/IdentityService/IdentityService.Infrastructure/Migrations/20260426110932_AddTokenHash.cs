using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTokenHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "token",
                table: "refresh_tokens",
                newName: "token_hash");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_token",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_token_hash");

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "permissions",
                value: "[\"tank:read\",\"tank:create\",\"tank:update\",\"tank:delete\",\"tank:limit:1\",\"device:control\",\"auto:rule:create\",\"auto:rule:limit:5\",\"auto:schedule:create\",\"data:view\",\"notify:log:read\",\"notify:log:write\",\"account:update\",\"account:view\"]");

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "permissions",
                value: "[\"tank:read\",\"tank:create\",\"tank:update\",\"tank:delete\",\"tank:limit:10\",\"device:control\",\"auto:rule:create\",\"auto:rule:limit:10\",\"auto:schedule:create\",\"data:view\",\"data:history\",\"notify:tg\",\"notify:log:read\",\"notify:log:write\",\"notify:reminder\",\"account:update\",\"account:view\"]");

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "permissions",
                value: "[\"tank:read\",\"tank:create\",\"tank:update\",\"tank:delete\",\"tank:limit:unlim\",\"device:control\",\"device:manual\",\"auto:rule:create\",\"auto:rule:limit:unlim\",\"auto:schedule:create\",\"auto:vacation\",\"data:view\",\"data:history\",\"data:diag\",\"data:rt\",\"notify:log:read\",\"notify:log:write\",\"notify:reminder\",\"notify:email\",\"notify:tg\"]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "token_hash",
                table: "refresh_tokens",
                newName: "token");

            migrationBuilder.RenameIndex(
                name: "ix_refresh_tokens_token_hash",
                table: "refresh_tokens",
                newName: "ix_refresh_tokens_token");

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "permissions",
                value: new List<string> { "tank:read", "tank:create", "tank:update", "tank:delete", "tank:limit:1", "device:control", "auto:rule:create", "auto:rule:limit:5", "auto:schedule:create", "data:view", "notify:log:read", "notify:log:write", "account:update", "account:view" });

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "permissions",
                value: new List<string> { "tank:read", "tank:create", "tank:update", "tank:delete", "tank:limit:10", "device:control", "auto:rule:create", "auto:rule:limit:10", "auto:schedule:create", "data:view", "data:history", "notify:tg", "notify:log:read", "notify:log:write", "notify:reminder", "account:update", "account:view" });

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "permissions",
                value: new List<string> { "tank:read", "tank:create", "tank:update", "tank:delete", "tank:limit:unlim", "device:control", "device:manual", "auto:rule:create", "auto:rule:limit:unlim", "auto:schedule:create", "auto:vacation", "data:view", "data:history", "data:diag", "data:rt", "notify:log:read", "notify:log:write", "notify:reminder", "notify:email", "notify:tg" });
        }
    }
}
