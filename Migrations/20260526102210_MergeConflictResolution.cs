using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerTrack.Migrations
{
    /// <inheritdoc />
    public partial class MergeConflictResolution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "InternshipPostingId",
                table: "JobApplications",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerId",
                table: "Companies",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Companies_EmployerId",
                table: "Companies",
                column: "EmployerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_AspNetUsers_EmployerId",
                table: "Companies",
                column: "EmployerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_AspNetUsers_EmployerId",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_EmployerId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "InternshipPostingId",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "EmployerId",
                table: "Companies");
        }
    }
}
