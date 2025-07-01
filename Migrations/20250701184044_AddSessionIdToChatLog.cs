using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DummyDataDog.Migrations
{
    public partial class AddSessionIdToChatLog : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "sessionid",
                table: "chatlogs",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sessionid",
                table: "chatlogs");
        }
    }
}
