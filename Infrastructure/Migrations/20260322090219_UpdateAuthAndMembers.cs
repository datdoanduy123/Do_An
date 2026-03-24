using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuthAndMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VaiTro",
                table: "Users");

            migrationBuilder.CreateTable(
                name: "DuAnNguoiDungs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DuAnId = table.Column<int>(type: "int", nullable: false),
                    NguoiDungId = table.Column<int>(type: "int", nullable: false),
                    ProjectRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JointAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuAnNguoiDungs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuAnNguoiDungs_DuAns_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DuAnNguoiDungs_Users_NguoiDungId",
                        column: x => x.NguoiDungId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DuAnNguoiDungs_DuAnId",
                table: "DuAnNguoiDungs",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_DuAnNguoiDungs_NguoiDungId",
                table: "DuAnNguoiDungs",
                column: "NguoiDungId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DuAnNguoiDungs");

            migrationBuilder.AddColumn<string>(
                name: "VaiTro",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);
        }
    }
}
