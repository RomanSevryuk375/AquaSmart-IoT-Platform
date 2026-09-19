using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IdentityService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOutboxMessageSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_occurred_on_utc",
                table: "outbox_messages");

            migrationBuilder.AddColumn<DateTime>(
                name: "next_retry_on_utc",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "retry_count",
                table: "outbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

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

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_occurred_on_utc",
                table: "outbox_messages",
                column: "occurred_on_utc",
                filter: "processed_on_utc IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_outbox_messages_occurred_on_utc",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "next_retry_on_utc",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "retry_count",
                table: "outbox_messages");

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000001"),
                column: "version",
                value: new Guid("b9875e1e-44bf-4d8a-aa4a-7ac5ada42a28"));

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000002"),
                column: "version",
                value: new Guid("78de41de-286a-4762-b88c-ed659975076a"));

            migrationBuilder.UpdateData(
                table: "subscriptions",
                keyColumn: "id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000003"),
                column: "version",
                value: new Guid("1f8c20bc-c96f-4731-8157-f270dcc20c92"));

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_occurred_on_utc",
                table: "outbox_messages",
                column: "occurred_on_utc");
        }
    }
}
