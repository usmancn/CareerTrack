using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddJobApplicationPostingForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "JobApplications"
                SET "InternshipPostingId" = NULL
                WHERE "InternshipPostingId" IS NOT NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM "JobPostings"
                      WHERE "JobPostings"."Id" = "JobApplications"."InternshipPostingId"
                  );
                """);

            migrationBuilder.CreateIndex(
                name: "IX_JobApplications_InternshipPostingId",
                table: "JobApplications",
                column: "InternshipPostingId");

            migrationBuilder.AddForeignKey(
                name: "FK_JobApplications_JobPostings_InternshipPostingId",
                table: "JobApplications",
                column: "InternshipPostingId",
                principalTable: "JobPostings",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_JobApplications_JobPostings_InternshipPostingId",
                table: "JobApplications");

            migrationBuilder.DropIndex(
                name: "IX_JobApplications_InternshipPostingId",
                table: "JobApplications");
        }
    }
}
