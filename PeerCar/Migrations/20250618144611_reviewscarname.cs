using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeerCar.Migrations
{
    /// <inheritdoc />
    public partial class reviewscarname : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_RenterId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Cars_AspNetUsers_OwnerId",
                table: "Cars");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Cars_CarId",
                table: "Reviews");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_RenterId",
                table: "Bookings",
                column: "RenterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Cars_AspNetUsers_OwnerId",
                table: "Cars",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Cars_CarId",
                table: "Reviews",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Bookings_AspNetUsers_RenterId",
                table: "Bookings");

            migrationBuilder.DropForeignKey(
                name: "FK_Cars_AspNetUsers_OwnerId",
                table: "Cars");

            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Cars_CarId",
                table: "Reviews");

            migrationBuilder.AddForeignKey(
                name: "FK_Bookings_AspNetUsers_RenterId",
                table: "Bookings",
                column: "RenterId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Cars_AspNetUsers_OwnerId",
                table: "Cars",
                column: "OwnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Cars_CarId",
                table: "Reviews",
                column: "CarId",
                principalTable: "Cars",
                principalColumn: "Id");
        }
    }
}
