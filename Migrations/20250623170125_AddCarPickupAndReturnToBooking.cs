using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeerCar.Migrations
{
    /// <inheritdoc />
    public partial class AddCarPickupAndReturnToBooking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CarReceivedDate",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CarReturnedDate",
                table: "Bookings",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCarReceivedByUser",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsCarReturnedByUser",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundAmount",
                table: "Bookings",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CarReceivedDate",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "CarReturnedDate",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsCarReceivedByUser",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "IsCarReturnedByUser",
                table: "Bookings");

            migrationBuilder.DropColumn(
                name: "RefundAmount",
                table: "Bookings");
        }
    }
}
