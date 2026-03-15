using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCongViecSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "NgayKetThuc",
                table: "CongViecs",
                newName: "NgayKetThucThucTe");

            migrationBuilder.RenameColumn(
                name: "NgayBatDau",
                table: "CongViecs",
                newName: "NgayKetThucDuKien");

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayBatDauDuKien",
                table: "CongViecs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NgayBatDauThucTe",
                table: "CongViecs",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ViTri",
                table: "CongViecs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "PhuThuocCongViecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TaskId = table.Column<int>(type: "int", nullable: false),
                    DependsOnTaskId = table.Column<int>(type: "int", nullable: false),
                    DependencyType = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PhuThuocCongViecs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhuThuocCongViecs_CongViecs_DependsOnTaskId",
                        column: x => x.DependsOnTaskId,
                        principalTable: "CongViecs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhuThuocCongViecs_CongViecs_TaskId",
                        column: x => x.TaskId,
                        principalTable: "CongViecs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhuThuocCongViecs_DependsOnTaskId",
                table: "PhuThuocCongViecs",
                column: "DependsOnTaskId");

            migrationBuilder.CreateIndex(
                name: "IX_PhuThuocCongViecs_TaskId",
                table: "PhuThuocCongViecs",
                column: "TaskId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PhuThuocCongViecs");

            migrationBuilder.DropColumn(
                name: "NgayBatDauDuKien",
                table: "CongViecs");

            migrationBuilder.DropColumn(
                name: "NgayBatDauThucTe",
                table: "CongViecs");

            migrationBuilder.DropColumn(
                name: "ViTri",
                table: "CongViecs");

            migrationBuilder.RenameColumn(
                name: "NgayKetThucThucTe",
                table: "CongViecs",
                newName: "NgayKetThuc");

            migrationBuilder.RenameColumn(
                name: "NgayKetThucDuKien",
                table: "CongViecs",
                newName: "NgayBatDau");
        }
    }
}
