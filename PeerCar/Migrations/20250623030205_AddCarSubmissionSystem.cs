using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PeerCar.Migrations
{
    /// <inheritdoc />
    public partial class AddCarSubmissionSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovalDate",
                table: "Cars",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentPath",
                table: "Cars",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "Cars",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmissionDate",
                table: "Cars",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "SubmissionStatus",
                table: "Cars",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "CarSubmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarId = table.Column<int>(type: "int", nullable: false),
                    SubmittedById = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AdminNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovedById = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarSubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarSubmissions_AspNetUsers_ApprovedById",
                        column: x => x.ApprovedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarSubmissions_AspNetUsers_SubmittedById",
                        column: x => x.SubmittedById,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CarSubmissions_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CarSubmissions_ApprovedById",
                table: "CarSubmissions",
                column: "ApprovedById");

            migrationBuilder.CreateIndex(
                name: "IX_CarSubmissions_CarId",
                table: "CarSubmissions",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_CarSubmissions_SubmittedById",
                table: "CarSubmissions",
                column: "SubmittedById");

            // Set existing cars to approved status (they were created by admins)
            migrationBuilder.Sql("UPDATE Cars SET SubmissionStatus = 1, ApprovalDate = GETDATE() WHERE SubmissionStatus = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarSubmissions");

            migrationBuilder.DropColumn(
                name: "ApprovalDate",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "DocumentPath",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "SubmissionDate",
                table: "Cars");

            migrationBuilder.DropColumn(
                name: "SubmissionStatus",
                table: "Cars");
        }
    }
}
