using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCreatedByToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "Sprints",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "DuAns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CreatedBy",
                table: "CongViecs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_CreatedBy",
                table: "Sprints",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_DuAns_CreatedBy",
                table: "DuAns",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_CongViecs_CreatedBy",
                table: "CongViecs",
                column: "CreatedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_CongViecs_Users_CreatedBy",
                table: "CongViecs",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DuAns_Users_CreatedBy",
                table: "DuAns",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Sprints_Users_CreatedBy",
                table: "Sprints",
                column: "CreatedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CongViecs_Users_CreatedBy",
                table: "CongViecs");

            migrationBuilder.DropForeignKey(
                name: "FK_DuAns_Users_CreatedBy",
                table: "DuAns");

            migrationBuilder.DropForeignKey(
                name: "FK_Sprints_Users_CreatedBy",
                table: "Sprints");

            migrationBuilder.DropIndex(
                name: "IX_Sprints_CreatedBy",
                table: "Sprints");

            migrationBuilder.DropIndex(
                name: "IX_DuAns_CreatedBy",
                table: "DuAns");

            migrationBuilder.DropIndex(
                name: "IX_CongViecs_CreatedBy",
                table: "CongViecs");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Sprints");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DuAns");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CongViecs");
        }
    }
}
