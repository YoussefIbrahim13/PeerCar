using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peer_Car.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityVerificationToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DocumentStatus",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentStatus",
                table: "AspNetUsers");
        }
    }
}
