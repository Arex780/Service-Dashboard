using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WebArchive.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<long>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Authors = table.Column<string>(nullable: true),
                    WebAdress = table.Column<string>(nullable: true),
                    Description = table.Column<string>(nullable: false),
                    PostCreator = table.Column<string>(nullable: true),
                    PostTime = table.Column<DateTime>(nullable: false),
                    EditTime = table.Column<DateTime>(nullable: false),
                    Status = table.Column<bool>(nullable: false),
                    Repository = table.Column<string>(nullable: true),
                    WrittenIn = table.Column<string>(nullable: true),
                    Userland = table.Column<string>(nullable: true),
                    Logo = table.Column<byte[]>(nullable: true),
                    ImageUI = table.Column<byte[]>(nullable: true),
                    Version = table.Column<string>(nullable: true),
                    ShortDesc = table.Column<string>(maxLength: 100, nullable: false),
                    Keygen = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Projects");
        }
    }
}
