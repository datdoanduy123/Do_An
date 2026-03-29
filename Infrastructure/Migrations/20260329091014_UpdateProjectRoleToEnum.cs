using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProjectRoleToEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Convert existing string data to numeric values before altering column type
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = '0' WHERE ProjectRole = 'Member'");
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = '1' WHERE ProjectRole = 'Developer'");
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = '2' WHERE ProjectRole = 'Tester'");
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = '3' WHERE ProjectRole = 'QA'");
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = '4' WHERE ProjectRole = 'PM'");
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = '5' WHERE ProjectRole = 'BA'");
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = '0' WHERE ProjectRole NOT IN ('0','1','2','3','4','5')");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectRole",
                table: "DuAnNguoiDungs",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProjectRole",
                table: "DuAnNguoiDungs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
                
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = 'Member' WHERE ProjectRole = '0'");
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = 'Developer' WHERE ProjectRole = '1'");
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = 'Tester' WHERE ProjectRole = '2'");
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = 'QA' WHERE ProjectRole = '3'");
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = 'PM' WHERE ProjectRole = '4'");
            migrationBuilder.Sql("UPDATE DuAnNguoiDungs SET ProjectRole = 'BA' WHERE ProjectRole = '5'");
        }
    }
}
