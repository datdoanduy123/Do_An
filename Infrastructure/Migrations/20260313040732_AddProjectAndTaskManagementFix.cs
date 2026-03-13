using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectAndTaskManagementFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DuAns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenDuAn = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuAns", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "KyNangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenKyNang = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KyNangs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "QuyTacGiaoViecAIs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaQuyTac = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GiaTri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    LoaiDuLieu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuyTacGiaoViecAIs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sprints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DuAnId = table.Column<int>(type: "int", nullable: false),
                    TenSprint = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MucTieuStoryPoints = table.Column<int>(type: "int", nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sprints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sprints_DuAns_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TaiLieuDuAns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DuAnId = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    FileType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UploadedBy = table.Column<int>(type: "int", nullable: false),
                    UploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaiLieuDuAns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TaiLieuDuAns_DuAns_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "KyNangNguoiDungs",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false),
                    KyNangId = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    SoNamKinhNghiem = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KyNangNguoiDungs", x => new { x.UserId, x.KyNangId });
                    table.ForeignKey(
                        name: "FK_KyNangNguoiDungs_KyNangs_KyNangId",
                        column: x => x.KyNangId,
                        principalTable: "KyNangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_KyNangNguoiDungs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CongViecs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DuAnId = table.Column<int>(type: "int", nullable: false),
                    SprintId = table.Column<int>(type: "int", nullable: true),
                    TieuDe = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LoaiCongViec = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DoUuTien = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TrangThai = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    StoryPoints = table.Column<int>(type: "int", nullable: false),
                    AssigneeId = table.Column<int>(type: "int", nullable: true),
                    PhuongThucGiaoViec = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AiReasoning = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AiMatchScore = table.Column<double>(type: "float", nullable: true),
                    ThoiGianUocTinh = table.Column<double>(type: "float", nullable: false),
                    ThoiGianThucTe = table.Column<double>(type: "float", nullable: true),
                    NgayBatDau = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NgayKetThuc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongViecs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CongViecs_DuAns_DuAnId",
                        column: x => x.DuAnId,
                        principalTable: "DuAns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CongViecs_Sprints_SprintId",
                        column: x => x.SprintId,
                        principalTable: "Sprints",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CongViecs_Users_AssigneeId",
                        column: x => x.AssigneeId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "YeuCauCongViecs",
                columns: table => new
                {
                    CongViecId = table.Column<int>(type: "int", nullable: false),
                    KyNangId = table.Column<int>(type: "int", nullable: false),
                    MucDoYeuCau = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_YeuCauCongViecs", x => new { x.CongViecId, x.KyNangId });
                    table.ForeignKey(
                        name: "FK_YeuCauCongViecs_CongViecs_CongViecId",
                        column: x => x.CongViecId,
                        principalTable: "CongViecs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_YeuCauCongViecs_KyNangs_KyNangId",
                        column: x => x.KyNangId,
                        principalTable: "KyNangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CongViecs_AssigneeId",
                table: "CongViecs",
                column: "AssigneeId");

            migrationBuilder.CreateIndex(
                name: "IX_CongViecs_DuAnId",
                table: "CongViecs",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_CongViecs_SprintId",
                table: "CongViecs",
                column: "SprintId");

            migrationBuilder.CreateIndex(
                name: "IX_KyNangNguoiDungs_KyNangId",
                table: "KyNangNguoiDungs",
                column: "KyNangId");

            migrationBuilder.CreateIndex(
                name: "IX_QuyTacGiaoViecAIs_MaQuyTac",
                table: "QuyTacGiaoViecAIs",
                column: "MaQuyTac",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sprints_DuAnId",
                table: "Sprints",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_TaiLieuDuAns_DuAnId",
                table: "TaiLieuDuAns",
                column: "DuAnId");

            migrationBuilder.CreateIndex(
                name: "IX_YeuCauCongViecs_KyNangId",
                table: "YeuCauCongViecs",
                column: "KyNangId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KyNangNguoiDungs");

            migrationBuilder.DropTable(
                name: "QuyTacGiaoViecAIs");

            migrationBuilder.DropTable(
                name: "TaiLieuDuAns");

            migrationBuilder.DropTable(
                name: "YeuCauCongViecs");

            migrationBuilder.DropTable(
                name: "CongViecs");

            migrationBuilder.DropTable(
                name: "KyNangs");

            migrationBuilder.DropTable(
                name: "Sprints");

            migrationBuilder.DropTable(
                name: "DuAns");
        }
    }
}
