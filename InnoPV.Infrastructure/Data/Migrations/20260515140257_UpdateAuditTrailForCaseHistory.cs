using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnoPV.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditTrailForCaseHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PerformedByRole",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "Reason",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "RecordId",
                table: "AuditTrails");

            migrationBuilder.RenameColumn(
                name: "TableName",
                table: "AuditTrails",
                newName: "EntityName");

            migrationBuilder.RenameColumn(
                name: "IpAddress",
                table: "AuditTrails",
                newName: "CaseNo");

            migrationBuilder.AlterColumn<string>(
                name: "PerformedByUserId",
                table: "AuditTrails",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "OldValue",
                table: "AuditTrails",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NewValue",
                table: "AuditTrails",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<long>(
                name: "EntityId",
                table: "AuditTrails",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformedByUserName",
                table: "AuditTrails",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "PvCaseId",
                table: "AuditTrails",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Remarks",
                table: "AuditTrails",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_CaseNo",
                table: "AuditTrails",
                column: "CaseNo");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_PerformedOnUtc",
                table: "AuditTrails",
                column: "PerformedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AuditTrails_PvCaseId",
                table: "AuditTrails",
                column: "PvCaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AuditTrails_CaseNo",
                table: "AuditTrails");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrails_PerformedOnUtc",
                table: "AuditTrails");

            migrationBuilder.DropIndex(
                name: "IX_AuditTrails_PvCaseId",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "PerformedByUserName",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "PvCaseId",
                table: "AuditTrails");

            migrationBuilder.DropColumn(
                name: "Remarks",
                table: "AuditTrails");

            migrationBuilder.RenameColumn(
                name: "EntityName",
                table: "AuditTrails",
                newName: "TableName");

            migrationBuilder.RenameColumn(
                name: "CaseNo",
                table: "AuditTrails",
                newName: "IpAddress");

            migrationBuilder.AlterColumn<string>(
                name: "PerformedByUserId",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldMaxLength: 450,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "OldValue",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "NewValue",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(4000)",
                oldMaxLength: 4000,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PerformedByRole",
                table: "AuditTrails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "AuditTrails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RecordId",
                table: "AuditTrails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
