using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Peer_Car.Migrations
{
    /// <inheritdoc />
    public partial class AddUserDocumentsFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDocumentsVerified",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LicenseBackUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LicenseFrontUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdBackUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NationalIdFrontUrl",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDocumentsVerified",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LicenseBackUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "LicenseFrontUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NationalIdBackUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NationalIdFrontUrl",
                table: "AspNetUsers");
        }
    }
}
