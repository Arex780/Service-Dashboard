using Microsoft.EntityFrameworkCore.Migrations;

namespace WebArchive.Migrations
{
    public partial class ChangeStatusToInt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "Projects",
                nullable: false,
                oldClrType: typeof(byte),
                oldType: "tinyint");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "Status",
                table: "Projects",
                type: "tinyint",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
