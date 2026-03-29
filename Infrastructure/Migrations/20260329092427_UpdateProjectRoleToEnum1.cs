using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectRoleToEnum1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TraoLoiCongViecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CongViecId = table.Column<int>(type: "int", nullable: false),
                    NguoiTaoId = table.Column<int>(type: "int", nullable: false),
                    NoiDung = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Loai = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TraoLoiCongViecs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TraoLoiCongViecs_CongViecs_CongViecId",
                        column: x => x.CongViecId,
                        principalTable: "CongViecs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TraoLoiCongViecs_Users_NguoiTaoId",
                        column: x => x.NguoiTaoId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TraoLoiCongViecs_CongViecId",
                table: "TraoLoiCongViecs",
                column: "CongViecId");

            migrationBuilder.CreateIndex(
                name: "IX_TraoLoiCongViecs_NguoiTaoId",
                table: "TraoLoiCongViecs",
                column: "NguoiTaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TraoLoiCongViecs");
        }
    }
}
