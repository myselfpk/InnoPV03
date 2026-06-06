using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InnoPV.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDetailedCaseEntryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CaseAdverseEventDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PvCaseId = table.Column<long>(type: "bigint", nullable: false),
                    EventTerm = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    EventDescription = table.Column<string>(type: "nvarchar(3000)", maxLength: 3000, nullable: true),
                    EventStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EventStopDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSerious = table.Column<bool>(type: "bit", nullable: false),
                    SeriousnessCriteria = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Severity = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Outcome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DeathDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CauseOfDeath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    WasHospitalized = table.Column<bool>(type: "bit", nullable: false),
                    HospitalizationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DischargeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TreatmentGivenForEvent = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    EventRemarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseAdverseEventDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseAdverseEventDetails_PvCases_PvCaseId",
                        column: x => x.PvCaseId,
                        principalTable: "PvCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseAttachments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PvCaseId = table.Column<long>(type: "bigint", nullable: false),
                    AttachmentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    UploadedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseAttachments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseAttachments_PvCases_PvCaseId",
                        column: x => x.PvCaseId,
                        principalTable: "PvCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseConcomitantMedications",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PvCaseId = table.Column<long>(type: "bigint", nullable: false),
                    MedicationName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Dose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DoseUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Route = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TherapyStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TherapyStopDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Indication = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsMedicationForEventTreatment = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseConcomitantMedications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseConcomitantMedications_PvCases_PvCaseId",
                        column: x => x.PvCaseId,
                        principalTable: "PvCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseLabDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PvCaseId = table.Column<long>(type: "bigint", nullable: false),
                    TestName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    TestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TestResult = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NormalRange = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ClinicalSignificance = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseLabDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseLabDetails_PvCases_PvCaseId",
                        column: x => x.PvCaseId,
                        principalTable: "PvCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CasePatientDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PvCaseId = table.Column<long>(type: "bigint", nullable: false),
                    PatientInitials = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PatientIdentifier = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    AgeUnit = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    WeightKg = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    HeightCm = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsPregnant = table.Column<bool>(type: "bit", nullable: false),
                    PregnancyRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RelevantMedicalHistory = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AllergyHistory = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OtherPatientInformation = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CasePatientDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CasePatientDetails_PvCases_PvCaseId",
                        column: x => x.PvCaseId,
                        principalTable: "PvCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseReporterDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PvCaseId = table.Column<long>(type: "bigint", nullable: false),
                    ReporterName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ReporterType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Qualification = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OrganizationName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Department = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    City = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    State = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DateOfReport = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ConsentForFollowUp = table.Column<bool>(type: "bit", nullable: false),
                    ReporterRemarks = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseReporterDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseReporterDetails_PvCases_PvCaseId",
                        column: x => x.PvCaseId,
                        principalTable: "PvCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseSuspectProductDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PvCaseId = table.Column<long>(type: "bigint", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    GenericName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    BatchNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Dose = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DoseUnit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Route = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Frequency = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TherapyStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TherapyStopDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Indication = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    ActionTaken = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Dechallenge = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Rechallenge = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Causality = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    ProductRemarks = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseSuspectProductDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseSuspectProductDetails_PvCases_PvCaseId",
                        column: x => x.PvCaseId,
                        principalTable: "PvCases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseAdverseEventDetails_PvCaseId",
                table: "CaseAdverseEventDetails",
                column: "PvCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseAttachments_PvCaseId",
                table: "CaseAttachments",
                column: "PvCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseConcomitantMedications_PvCaseId",
                table: "CaseConcomitantMedications",
                column: "PvCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseLabDetails_PvCaseId",
                table: "CaseLabDetails",
                column: "PvCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CasePatientDetails_PvCaseId",
                table: "CasePatientDetails",
                column: "PvCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseReporterDetails_PvCaseId",
                table: "CaseReporterDetails",
                column: "PvCaseId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseSuspectProductDetails_PvCaseId",
                table: "CaseSuspectProductDetails",
                column: "PvCaseId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseAdverseEventDetails");

            migrationBuilder.DropTable(
                name: "CaseAttachments");

            migrationBuilder.DropTable(
                name: "CaseConcomitantMedications");

            migrationBuilder.DropTable(
                name: "CaseLabDetails");

            migrationBuilder.DropTable(
                name: "CasePatientDetails");

            migrationBuilder.DropTable(
                name: "CaseReporterDetails");

            migrationBuilder.DropTable(
                name: "CaseSuspectProductDetails");
        }
    }
}
