using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateProm1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraoLoiCongViecs_Users_NguoiTaoId",
                table: "TraoLoiCongViecs");

            migrationBuilder.AlterColumn<string>(
                name: "NoiDung",
                table: "TraoLoiCongViecs",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "CongViecId1",
                table: "TraoLoiCongViecs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TraoLoiCongViecs_CongViecId1",
                table: "TraoLoiCongViecs",
                column: "CongViecId1");

            migrationBuilder.AddForeignKey(
                name: "FK_TraoLoiCongViecs_CongViecs_CongViecId1",
                table: "TraoLoiCongViecs",
                column: "CongViecId1",
                principalTable: "CongViecs",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TraoLoiCongViecs_Users_NguoiTaoId",
                table: "TraoLoiCongViecs",
                column: "NguoiTaoId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TraoLoiCongViecs_CongViecs_CongViecId1",
                table: "TraoLoiCongViecs");

            migrationBuilder.DropForeignKey(
                name: "FK_TraoLoiCongViecs_Users_NguoiTaoId",
                table: "TraoLoiCongViecs");

            migrationBuilder.DropIndex(
                name: "IX_TraoLoiCongViecs_CongViecId1",
                table: "TraoLoiCongViecs");

            migrationBuilder.DropColumn(
                name: "CongViecId1",
                table: "TraoLoiCongViecs");

            migrationBuilder.AlterColumn<string>(
                name: "NoiDung",
                table: "TraoLoiCongViecs",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddForeignKey(
                name: "FK_TraoLoiCongViecs_Users_NguoiTaoId",
                table: "TraoLoiCongViecs",
                column: "NguoiTaoId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
