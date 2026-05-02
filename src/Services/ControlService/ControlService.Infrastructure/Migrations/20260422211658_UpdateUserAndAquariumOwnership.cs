using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Control.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUserAndAquariumOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "user_id",
                table: "aquariums",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "ix_aquariums_user_id",
                table: "aquariums",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_aquariums_user_id",
                table: "aquariums");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "aquariums");
        }
    }
}
