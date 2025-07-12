using Microsoft.EntityFrameworkCore.Migrations;

namespace CarRentalMVC.Migrations
{
    public partial class AddPendingEmailToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PendingEmail",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingEmail",
                table: "AspNetUsers");
        }
    }
}
