using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveStoryPoints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MucTieuStoryPoints",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "StoryPoints",
                table: "CongViecs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MucTieuStoryPoints",
                table: "Sprints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StoryPoints",
                table: "CongViecs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
