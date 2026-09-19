using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notification.Infrastructure.Migrations
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

            migrationBuilder.CreateIndex(
                name: "ix_outbox_messages_occurred_on_utc",
                table: "outbox_messages",
                column: "occurred_on_utc");
        }
    }
}
