using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSkillHierarchy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CongNgheId",
                table: "KyNangs",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "NhomKyNangs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenNhom = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NhomKyNangs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CongNghes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenCongNghe = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    MoTa = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NhomKyNangId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CongNghes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CongNghes_NhomKyNangs_NhomKyNangId",
                        column: x => x.NhomKyNangId,
                        principalTable: "NhomKyNangs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_KyNangs_CongNgheId",
                table: "KyNangs",
                column: "CongNgheId");

            migrationBuilder.CreateIndex(
                name: "IX_CongNghes_NhomKyNangId",
                table: "CongNghes",
                column: "NhomKyNangId");

            migrationBuilder.AddForeignKey(
                name: "FK_KyNangs_CongNghes_CongNgheId",
                table: "KyNangs",
                column: "CongNgheId",
                principalTable: "CongNghes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KyNangs_CongNghes_CongNgheId",
                table: "KyNangs");

            migrationBuilder.DropTable(
                name: "CongNghes");

            migrationBuilder.DropTable(
                name: "NhomKyNangs");

            migrationBuilder.DropIndex(
                name: "IX_KyNangs_CongNgheId",
                table: "KyNangs");

            migrationBuilder.DropColumn(
                name: "CongNgheId",
                table: "KyNangs");
        }
    }
}
