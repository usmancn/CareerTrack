using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerTrack.Migrations
{
    /// <inheritdoc />
    public partial class AddJobApplicationToToDo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "JobApplicationId",
                table: "ToDos",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ToDos_JobApplicationId",
                table: "ToDos",
                column: "JobApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_ToDos_JobApplications_JobApplicationId",
                table: "ToDos",
                column: "JobApplicationId",
                principalTable: "JobApplications",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ToDos_JobApplications_JobApplicationId",
                table: "ToDos");

            migrationBuilder.DropIndex(
                name: "IX_ToDos_JobApplicationId",
                table: "ToDos");

            migrationBuilder.DropColumn(
                name: "JobApplicationId",
                table: "ToDos");
        }
    }
}
