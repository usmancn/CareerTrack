using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CareerTrack.Migrations
{
    /// <inheritdoc />
    public partial class UpdateWorkflowSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Interviews");

            migrationBuilder.DropTable(
                name: "Offers");

            migrationBuilder.RenameColumn(
                name: "IsApprovedByAdmin",
                table: "DailyLogs",
                newName: "Status");

            migrationBuilder.RenameColumn(
                name: "AdminNote",
                table: "DailyLogs",
                newName: "SchoolNote");

            migrationBuilder.AddColumn<string>(
                name: "EmployerNote",
                table: "JobApplications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InternshipEndDate",
                table: "JobApplications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InternshipStartDate",
                table: "JobApplications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolNote",
                table: "JobApplications",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalInternshipDays",
                table: "JobApplications",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployerNote",
                table: "DailyLogs",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmployerApproved",
                table: "DailyLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSchoolApproved",
                table: "DailyLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "JobApplicationId",
                table: "DailyLogs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "StudentTasks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    AssignedByEmployerId = table.Column<string>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DueDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    IsCompleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentTasks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StudentTasks_AspNetUsers_AssignedByEmployerId",
                        column: x => x.AssignedByEmployerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_StudentTasks_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DailyLogs_JobApplicationId",
                table: "DailyLogs",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTasks_AssignedByEmployerId",
                table: "StudentTasks",
                column: "AssignedByEmployerId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentTasks_JobApplicationId",
                table: "StudentTasks",
                column: "JobApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_DailyLogs_JobApplications_JobApplicationId",
                table: "DailyLogs",
                column: "JobApplicationId",
                principalTable: "JobApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DailyLogs_JobApplications_JobApplicationId",
                table: "DailyLogs");

            migrationBuilder.DropTable(
                name: "StudentTasks");

            migrationBuilder.DropIndex(
                name: "IX_DailyLogs_JobApplicationId",
                table: "DailyLogs");

            migrationBuilder.DropColumn(
                name: "EmployerNote",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "InternshipEndDate",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "InternshipStartDate",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "SchoolNote",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "TotalInternshipDays",
                table: "JobApplications");

            migrationBuilder.DropColumn(
                name: "EmployerNote",
                table: "DailyLogs");

            migrationBuilder.DropColumn(
                name: "IsEmployerApproved",
                table: "DailyLogs");

            migrationBuilder.DropColumn(
                name: "IsSchoolApproved",
                table: "DailyLogs");

            migrationBuilder.DropColumn(
                name: "JobApplicationId",
                table: "DailyLogs");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "DailyLogs",
                newName: "IsApprovedByAdmin");

            migrationBuilder.RenameColumn(
                name: "SchoolNote",
                table: "DailyLogs",
                newName: "AdminNote");

            migrationBuilder.CreateTable(
                name: "Interviews",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    InterviewDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ResultStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    Stage = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interviews_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Offers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    JobApplicationId = table.Column<int>(type: "INTEGER", nullable: false),
                    Benefits = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DeadlineDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsAccepted = table.Column<bool>(type: "INTEGER", nullable: false),
                    Salary = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Offers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Offers_JobApplications_JobApplicationId",
                        column: x => x.JobApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Interviews_JobApplicationId",
                table: "Interviews",
                column: "JobApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_Offers_JobApplicationId",
                table: "Offers",
                column: "JobApplicationId",
                unique: true);
        }
    }
}
