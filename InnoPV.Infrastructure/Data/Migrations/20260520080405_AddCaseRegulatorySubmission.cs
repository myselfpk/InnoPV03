using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnoPV.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseRegulatorySubmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseFollowUps_PvCases_PvCaseId",
                table: "CaseFollowUps");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "CaseFollowUps",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "ReceivedFrom",
                table: "CaseFollowUps",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessedRemarks",
                table: "CaseFollowUps",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessedByUserId",
                table: "CaseFollowUps",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FollowUpNo",
                table: "CaseFollowUps",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CaseFollowUps",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalInformation",
                table: "CaseFollowUps",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "CaseRegulatorySubmissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PvCaseId = table.Column<long>(type: "bigint", nullable: false),
                    SubmissionNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubmissionType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecipientAuthority = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SubmissionFormat = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubmissionStatus = table.Column<int>(type: "int", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgementReceivedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceNo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SubmittedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AcknowledgementRemarks = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseRegulatorySubmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseRegulatorySubmissions_PvCases_PvCaseId",
                        column: x => x.PvCaseId,
                        principalTable: "PvCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseFollowUps_FollowUpNo",
                table: "CaseFollowUps",
                column: "FollowUpNo");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFollowUps_IsProcessed",
                table: "CaseFollowUps",
                column: "IsProcessed");

            migrationBuilder.CreateIndex(
                name: "IX_CaseFollowUps_ReceiptDate",
                table: "CaseFollowUps",
                column: "ReceiptDate");

            migrationBuilder.CreateIndex(
                name: "IX_CaseRegulatorySubmissions_DueDate",
                table: "CaseRegulatorySubmissions",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_CaseRegulatorySubmissions_PvCaseId",
                table: "CaseRegulatorySubmissions",
                column: "PvCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseRegulatorySubmissions_SubmissionNo",
                table: "CaseRegulatorySubmissions",
                column: "SubmissionNo");

            migrationBuilder.CreateIndex(
                name: "IX_CaseRegulatorySubmissions_SubmissionStatus",
                table: "CaseRegulatorySubmissions",
                column: "SubmissionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_CaseRegulatorySubmissions_SubmittedDate",
                table: "CaseRegulatorySubmissions",
                column: "SubmittedDate");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseFollowUps_PvCases_PvCaseId",
                table: "CaseFollowUps",
                column: "PvCaseId",
                principalTable: "PvCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseFollowUps_PvCases_PvCaseId",
                table: "CaseFollowUps");

            migrationBuilder.DropTable(
                name: "CaseRegulatorySubmissions");

            migrationBuilder.DropIndex(
                name: "IX_CaseFollowUps_FollowUpNo",
                table: "CaseFollowUps");

            migrationBuilder.DropIndex(
                name: "IX_CaseFollowUps_IsProcessed",
                table: "CaseFollowUps");

            migrationBuilder.DropIndex(
                name: "IX_CaseFollowUps_ReceiptDate",
                table: "CaseFollowUps");

            migrationBuilder.AlterColumn<string>(
                name: "Source",
                table: "CaseFollowUps",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "ReceivedFrom",
                table: "CaseFollowUps",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(250)",
                oldMaxLength: 250,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessedRemarks",
                table: "CaseFollowUps",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ProcessedByUserId",
                table: "CaseFollowUps",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FollowUpNo",
                table: "CaseFollowUps",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CaseFollowUps",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000);

            migrationBuilder.AlterColumn<string>(
                name: "AdditionalInformation",
                table: "CaseFollowUps",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseFollowUps_PvCases_PvCaseId",
                table: "CaseFollowUps",
                column: "PvCaseId",
                principalTable: "PvCases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
