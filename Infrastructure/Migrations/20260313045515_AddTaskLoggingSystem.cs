using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTaskLoggingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NhatKyCongViecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CongViecId = table.Column<int>(type: "int", nullable: false),
                    NguoiCapNhatId = table.Column<int>(type: "int", nullable: false),
                    SoGioLamViec = table.Column<double>(type: "float", nullable: false),
                    GhiChu = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    NgayCapNhat = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhatKyCongViecs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NhatKyCongViecs_CongViecs_CongViecId",
                        column: x => x.CongViecId,
                        principalTable: "CongViecs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NhatKyCongViecs_Users_NguoiCapNhatId",
                        column: x => x.NguoiCapNhatId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyCongViecs_CongViecId",
                table: "NhatKyCongViecs",
                column: "CongViecId");

            migrationBuilder.CreateIndex(
                name: "IX_NhatKyCongViecs_NguoiCapNhatId",
                table: "NhatKyCongViecs",
                column: "NguoiCapNhatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NhatKyCongViecs");
        }
    }
}
