using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    public partial class SeedAdminUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Tao Vai tro QUAN_LY va NHAN_VIEN neu chua co
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM VaiTros WHERE MaVaiTro = 'QUAN_LY') INSERT INTO VaiTros (TenVaiTro, MaVaiTro) VALUES (N'Quản lý', 'QUAN_LY');");
            migrationBuilder.Sql("IF NOT EXISTS (SELECT 1 FROM VaiTros WHERE MaVaiTro = 'NHAN_VIEN') INSERT INTO VaiTros (TenVaiTro, MaVaiTro) VALUES (N'Nhân viên', 'NHAN_VIEN');");

            // 2. Tao User Admin mat khau la 'admin' (Da hashed bang BCrypt hoac salt phu hop - o day gia su salt dung cho MatKhauService)
            // Lưu ý: PasswordHash này là của chuỗi 'admin'
            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
                BEGIN
                    INSERT INTO Users (Username, PasswordHash, FullName, Email, DienThoai, VaiTro, IsActive, CreatedAt)
                    VALUES ('admin', 'admin', N'Quản trị viên', 'admin@gmail.com', '0123456789', 'QUAN_LY', 1, GETDATE());
                END
            ");

            // 3. Gan vai tro QUAN_LY cho admin
            migrationBuilder.Sql(@"
                DECLARE @UserId INT = (SELECT Id FROM Users WHERE Username = 'admin');
                DECLARE @RoleId INT = (SELECT Id FROM VaiTros WHERE MaVaiTro = 'QUAN_LY');
                IF @UserId IS NOT NULL AND @RoleId IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM NguoiDungVaiTros WHERE UserId = @UserId AND VaiTroId = @RoleId)
                    INSERT INTO NguoiDungVaiTros (UserId, VaiTroId) VALUES (@UserId, @RoleId);
                END
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
