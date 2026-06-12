using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peer_Car.Migrations
{
    /// <inheritdoc />
    public partial class AddIsKeyHandedOverToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsKeyHandedOver",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsKeyHandedOver",
                table: "Bookings");
        }
    }
}
