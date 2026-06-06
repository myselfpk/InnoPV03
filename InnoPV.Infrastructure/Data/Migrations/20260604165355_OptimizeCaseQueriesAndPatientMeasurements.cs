using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnoPV.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeCaseQueriesAndPatientMeasurements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "WeightKg",
                table: "CasePatientDetails",
                type: "decimal(19,3)",
                precision: 19,
                scale: 3,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PvCases_IsDeleted_CreatedOnUtc",
                table: "PvCases",
                columns: new[] { "IsDeleted", "CreatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_PvCases_IsDeleted_CurrentAssignedRole",
                table: "PvCases",
                columns: new[] { "IsDeleted", "CurrentAssignedRole" });

            migrationBuilder.CreateIndex(
                name: "IX_PvCases_IsDeleted_DueDate",
                table: "PvCases",
                columns: new[] { "IsDeleted", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_PvCases_IsDeleted_Status",
                table: "PvCases",
                columns: new[] { "IsDeleted", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PvCases_IsDeleted_CreatedOnUtc",
                table: "PvCases");

            migrationBuilder.DropIndex(
                name: "IX_PvCases_IsDeleted_CurrentAssignedRole",
                table: "PvCases");

            migrationBuilder.DropIndex(
                name: "IX_PvCases_IsDeleted_DueDate",
                table: "PvCases");

            migrationBuilder.DropIndex(
                name: "IX_PvCases_IsDeleted_Status",
                table: "PvCases");

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightKg",
                table: "CasePatientDetails",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(19,3)",
                oldPrecision: 19,
                oldScale: 3,
                oldNullable: true);
        }
    }
}
