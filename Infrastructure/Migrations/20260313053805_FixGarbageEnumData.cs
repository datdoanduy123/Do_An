using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixGarbageEnumData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE CongViecs SET DoUuTien = 'Medium' WHERE DoUuTien = 'string' OR DoUuTien IS NULL;");
            migrationBuilder.Sql("UPDATE CongViecs SET TrangThai = 'Todo' WHERE TrangThai = 'string' OR TrangThai IS NULL;");
            migrationBuilder.Sql("UPDATE CongViecs SET LoaiCongViec = 'Backend' WHERE LoaiCongViec = 'string' OR LoaiCongViec IS NULL;");
            migrationBuilder.Sql("UPDATE CongViecs SET PhuongThucGiaoViec = 'Manual' WHERE PhuongThucGiaoViec = 'string' OR PhuongThucGiaoViec IS NULL;");
            migrationBuilder.Sql("UPDATE DuAns SET TrangThai = 'Planning' WHERE TrangThai = 'string' OR TrangThai IS NULL;");
            migrationBuilder.Sql("UPDATE Sprints SET TrangThai = 'New' WHERE TrangThai = 'string' OR TrangThai IS NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
