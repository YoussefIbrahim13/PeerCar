using Microsoft.EntityFrameworkCore.Migrations;

namespace CarRentalMVC.Migrations
{
    public partial class AddBookingIdToReview : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "BookingId",
                table: "Reviews",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BookingId",
                table: "Reviews");
        }
    }
}
