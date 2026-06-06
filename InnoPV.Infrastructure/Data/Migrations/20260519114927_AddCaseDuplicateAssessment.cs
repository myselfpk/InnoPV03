using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnoPV.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseDuplicateAssessment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaseDuplicateAssessments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PvCaseId = table.Column<long>(type: "bigint", nullable: false),
                    PotentialDuplicatePvCaseId = table.Column<long>(type: "bigint", nullable: true),
                    MatchingScore = table.Column<int>(type: "int", nullable: false),
                    MatchReasons = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsConfirmedDuplicate = table.Column<bool>(type: "bit", nullable: false),
                    DecisionRemarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AssessedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AssessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseDuplicateAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseDuplicateAssessments_PvCases_PotentialDuplicatePvCaseId",
                        column: x => x.PotentialDuplicatePvCaseId,
                        principalTable: "PvCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CaseDuplicateAssessments_PvCases_PvCaseId",
                        column: x => x.PvCaseId,
                        principalTable: "PvCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseDuplicateAssessments_PotentialDuplicatePvCaseId",
                table: "CaseDuplicateAssessments",
                column: "PotentialDuplicatePvCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDuplicateAssessments_PvCaseId",
                table: "CaseDuplicateAssessments",
                column: "PvCaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseDuplicateAssessments");
        }
    }
}
