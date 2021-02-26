using Microsoft.EntityFrameworkCore.Migrations;

namespace WebArchive.Migrations
{
    public partial class ChangeStatusToByte : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<byte>(
                name: "Status",
                table: "Projects",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "Status",
                table: "Projects",
                type: "bit",
                nullable: false,
                oldClrType: typeof(byte));
        }
    }
}
