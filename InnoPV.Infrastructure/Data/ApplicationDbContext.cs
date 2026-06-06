using InnoPV.Domain.Entities;
using InnoPV.Domain.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InnoPV.Infrastructure.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<PvCase> PvCases => Set<PvCase>();
    public DbSet<CaseComment> CaseComments => Set<CaseComment>();
    public DbSet<ChecklistMaster> ChecklistMasters => Set<ChecklistMaster>();
    public DbSet<ChecklistItem> ChecklistItems => Set<ChecklistItem>();
    public DbSet<CaseChecklistResponse> CaseChecklistResponses => Set<CaseChecklistResponse>();
    public DbSet<AuditTrail> AuditTrails => Set<AuditTrail>();
    public DbSet<ProductMaster> ProductMasters => Set<ProductMaster>();
    public DbSet<SponsorMaster> SponsorMasters => Set<SponsorMaster>();
    public DbSet<StudyMaster> StudyMasters => Set<StudyMaster>();
    public DbSet<CommonMasterOption> CommonMasterOptions => Set<CommonMasterOption>();
    public DbSet<CasePatientDetail> CasePatientDetails => Set<CasePatientDetail>();
    public DbSet<CaseReporterDetail> CaseReporterDetails => Set<CaseReporterDetail>();
    public DbSet<CaseAdverseEventDetail> CaseAdverseEventDetails => Set<CaseAdverseEventDetail>();
    public DbSet<CaseSuspectProductDetail> CaseSuspectProductDetails => Set<CaseSuspectProductDetail>();
    public DbSet<CaseConcomitantMedication> CaseConcomitantMedications => Set<CaseConcomitantMedication>();
    public DbSet<CaseLabDetail> CaseLabDetails => Set<CaseLabDetail>();
    public DbSet<CaseAttachment> CaseAttachments => Set<CaseAttachment>();
    public DbSet<CaseDuplicateAssessment> CaseDuplicateAssessments => Set<CaseDuplicateAssessment>();
    public DbSet<CaseFollowUp> CaseFollowUps => Set<CaseFollowUp>();
    public DbSet<CaseRegulatorySubmission> CaseRegulatorySubmissions => Set<CaseRegulatorySubmission>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<PvCase>(entity =>
        {
            entity.ToTable("PvCases");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CaseNo)
                .HasMaxLength(50)
                .IsRequired();

            entity.HasIndex(x => x.CaseNo)
                .IsUnique();

            entity.HasIndex(x => new { x.IsDeleted, x.Status });
            entity.HasIndex(x => new { x.IsDeleted, x.CurrentAssignedRole });
            entity.HasIndex(x => new { x.IsDeleted, x.DueDate });
            entity.HasIndex(x => new { x.IsDeleted, x.CreatedOnUtc });

            entity.Property(x => x.CaseSource)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.InitialReporterName)
                .HasMaxLength(200);

            entity.Property(x => x.InitialPatientIdentifier)
                .HasMaxLength(100);

            entity.Property(x => x.InitialProductName)
                .HasMaxLength(200);

            entity.Property(x => x.InitialEventTerm)
                .HasMaxLength(250);

            entity.Property(x => x.CurrentAssignedRole)
                .HasMaxLength(100);

            entity.Property(x => x.Narrative)
                .HasColumnType("nvarchar(max)");

            entity.Property(x => x.CaseNarrativeHtml)
                .HasColumnType("nvarchar(max)");
        });

        builder.Entity<CaseComment>(entity =>
        {
            entity.ToTable("CaseComments");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CommentType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.CommentText)
                .HasMaxLength(2000)
                .IsRequired();

            entity.Property(x => x.CommentedByRole)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasOne(x => x.PvCase)
                .WithMany()
                .HasForeignKey(x => x.PvCaseId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<ChecklistMaster>(entity =>
        {
            entity.ToTable("ChecklistMasters");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ChecklistName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.ApplicableRole)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ApplicableStage)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.VersionNo)
                .HasMaxLength(20)
                .IsRequired();
        });

        builder.Entity<ChecklistItem>(entity =>
        {
            entity.ToTable("ChecklistItems");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ItemText)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasOne(x => x.ChecklistMaster)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.ChecklistMasterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CaseChecklistResponse>(entity =>
        {
            entity.ToTable("CaseChecklistResponses");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.RoleName)
                .HasMaxLength(100)
                .IsRequired();

            entity.HasOne(x => x.PvCase)
                .WithMany()
                .HasForeignKey(x => x.PvCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ChecklistItem)
                .WithMany()
                .HasForeignKey(x => x.ChecklistItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<AuditTrail>(entity =>
        {
            entity.ToTable("AuditTrails");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CaseNo)
                .HasMaxLength(100);

            entity.Property(x => x.EntityName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.ActionType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.FieldName)
                .HasMaxLength(150);

            entity.Property(x => x.OldValue)
                .HasMaxLength(4000);

            entity.Property(x => x.NewValue)
                .HasMaxLength(4000);

            entity.Property(x => x.Remarks)
                .HasMaxLength(4000);

            entity.Property(x => x.PerformedByUserId)
                .HasMaxLength(450);

            entity.Property(x => x.PerformedByUserName)
                .HasMaxLength(250);

            entity.HasIndex(x => x.PvCaseId);
            entity.HasIndex(x => x.CaseNo);
            entity.HasIndex(x => x.PerformedOnUtc);
        });

        builder.Entity<ProductMaster>(entity =>
        {
            entity.ToTable("ProductMasters");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ProductName)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.GenericName)
                .HasMaxLength(250);

            entity.Property(x => x.ProductCode)
                .HasMaxLength(50);

            entity.Property(x => x.ProductType)
                .HasMaxLength(100);

            entity.Property(x => x.Strength)
                .HasMaxLength(100);

            entity.Property(x => x.DosageForm)
                .HasMaxLength(100);

            entity.Property(x => x.ManufacturerName)
                .HasMaxLength(250);

            entity.Property(x => x.MarketingAuthorizationHolder)
                .HasMaxLength(250);

            entity.Property(x => x.Remarks)
                .HasMaxLength(1000);

            entity.HasIndex(x => x.ProductName);
        });

        builder.Entity<SponsorMaster>(entity =>
        {
            entity.ToTable("SponsorMasters");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.SponsorName)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.SponsorCode)
                .HasMaxLength(50);

            entity.Property(x => x.ContactPerson)
                .HasMaxLength(150);

            entity.Property(x => x.ContactEmail)
                .HasMaxLength(150);

            entity.Property(x => x.ContactPhone)
                .HasMaxLength(30);

            entity.Property(x => x.Address)
                .HasMaxLength(1000);

            entity.Property(x => x.Remarks)
                .HasMaxLength(1000);

            entity.HasIndex(x => x.SponsorName);
        });

        builder.Entity<StudyMaster>(entity =>
        {
            entity.ToTable("StudyMasters");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.StudyTitle)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.ProtocolNo)
                .HasMaxLength(150);

            entity.Property(x => x.StudyCode)
                .HasMaxLength(100);

            entity.Property(x => x.Indication)
                .HasMaxLength(250);

            entity.Property(x => x.StudyType)
                .HasMaxLength(150);

            entity.Property(x => x.Remarks)
                .HasMaxLength(1000);

            entity.HasOne(x => x.SponsorMaster)
                .WithMany()
                .HasForeignKey(x => x.SponsorMasterId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<CommonMasterOption>(entity =>
        {
            entity.ToTable("CommonMasterOptions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(x => x.Code)
                .HasMaxLength(100);

            entity.Property(x => x.Description)
                .HasMaxLength(1000);

            entity.HasIndex(x => new { x.MasterType, x.Name });
        });

        builder.Entity<CasePatientDetail>(entity =>
        {
            entity.ToTable("CasePatientDetails");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.PatientInitials).HasMaxLength(20);
            entity.Property(x => x.PatientIdentifier).HasMaxLength(100);
            entity.Property(x => x.AgeUnit).HasMaxLength(20);
            entity.Property(x => x.Gender).HasMaxLength(30);
            entity.Property(x => x.WeightKg).HasPrecision(19, 3);
            entity.Property(x => x.HeightCm).HasPrecision(18, 2);

            entity.Property(x => x.RelevantMedicalHistory).HasMaxLength(2000);
            entity.Property(x => x.AllergyHistory).HasMaxLength(1000);
            entity.Property(x => x.PregnancyRemarks).HasMaxLength(1000);
            entity.Property(x => x.OtherPatientInformation).HasMaxLength(2000);

            entity.HasOne(x => x.PvCase)
                .WithMany()
                .HasForeignKey(x => x.PvCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PvCaseId);
        });

        builder.Entity<CaseReporterDetail>(entity =>
        {
            entity.ToTable("CaseReporterDetails");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ReporterName).HasMaxLength(200).IsRequired();
            entity.Property(x => x.ReporterType).HasMaxLength(100);
            entity.Property(x => x.Qualification).HasMaxLength(100);
            entity.Property(x => x.OrganizationName).HasMaxLength(250);
            entity.Property(x => x.Department).HasMaxLength(150);
            entity.Property(x => x.Email).HasMaxLength(150);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Address).HasMaxLength(1000);
            entity.Property(x => x.City).HasMaxLength(100);
            entity.Property(x => x.State).HasMaxLength(100);
            entity.Property(x => x.Country).HasMaxLength(100);
            entity.Property(x => x.ReporterRemarks).HasMaxLength(1000);

            entity.HasOne(x => x.PvCase)
                .WithMany()
                .HasForeignKey(x => x.PvCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PvCaseId);
        });

        builder.Entity<CaseAdverseEventDetail>(entity =>
        {
            entity.ToTable("CaseAdverseEventDetails");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.EventTerm).HasMaxLength(250).IsRequired();
            entity.Property(x => x.EventDescription).HasMaxLength(3000);
            entity.Property(x => x.SeriousnessCriteria).HasMaxLength(250);
            entity.Property(x => x.Severity).HasMaxLength(100);
            entity.Property(x => x.Outcome).HasMaxLength(100);
            entity.Property(x => x.CauseOfDeath).HasMaxLength(1000);
            entity.Property(x => x.TreatmentGivenForEvent).HasMaxLength(2000);
            entity.Property(x => x.EventRemarks).HasMaxLength(2000);

            entity.HasOne(x => x.PvCase)
                .WithMany()
                .HasForeignKey(x => x.PvCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PvCaseId);
        });

        builder.Entity<CaseSuspectProductDetail>(entity =>
        {
            entity.ToTable("CaseSuspectProductDetails");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.ProductName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.GenericName).HasMaxLength(250);
            entity.Property(x => x.BatchNo).HasMaxLength(100);
            entity.Property(x => x.Dose).HasMaxLength(100);
            entity.Property(x => x.DoseUnit).HasMaxLength(50);
            entity.Property(x => x.Route).HasMaxLength(100);
            entity.Property(x => x.Frequency).HasMaxLength(100);
            entity.Property(x => x.Indication).HasMaxLength(250);
            entity.Property(x => x.ActionTaken).HasMaxLength(150);
            entity.Property(x => x.Dechallenge).HasMaxLength(150);
            entity.Property(x => x.Rechallenge).HasMaxLength(150);
            entity.Property(x => x.Causality).HasMaxLength(150);
            entity.Property(x => x.ProductRemarks).HasMaxLength(2000);

            entity.HasOne(x => x.PvCase)
                .WithMany()
                .HasForeignKey(x => x.PvCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PvCaseId);
        });

        builder.Entity<CaseConcomitantMedication>(entity =>
        {
            entity.ToTable("CaseConcomitantMedications");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.MedicationName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.Dose).HasMaxLength(100);
            entity.Property(x => x.DoseUnit).HasMaxLength(50);
            entity.Property(x => x.Route).HasMaxLength(100);
            entity.Property(x => x.Frequency).HasMaxLength(100);
            entity.Property(x => x.Indication).HasMaxLength(250);
            entity.Property(x => x.Remarks).HasMaxLength(1000);

            entity.HasOne(x => x.PvCase)
                .WithMany()
                .HasForeignKey(x => x.PvCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PvCaseId);
        });

        builder.Entity<CaseLabDetail>(entity =>
        {
            entity.ToTable("CaseLabDetails");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.TestName).HasMaxLength(250).IsRequired();
            entity.Property(x => x.TestResult).HasMaxLength(250);
            entity.Property(x => x.Unit).HasMaxLength(50);
            entity.Property(x => x.NormalRange).HasMaxLength(100);
            entity.Property(x => x.ClinicalSignificance).HasMaxLength(1000);
            entity.Property(x => x.Remarks).HasMaxLength(1000);

            entity.HasOne(x => x.PvCase)
                .WithMany()
                .HasForeignKey(x => x.PvCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PvCaseId);
        });

        builder.Entity<CaseAttachment>(entity =>
        {
            entity.ToTable("CaseAttachments");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.AttachmentType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FilePath).HasMaxLength(500).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000);
            entity.Property(x => x.UploadedByUserId).HasMaxLength(450).IsRequired();

            entity.HasOne(x => x.PvCase)
                .WithMany()
                .HasForeignKey(x => x.PvCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PvCaseId);
        });

        builder.Entity<CaseDuplicateAssessment>(entity =>
        {
            entity.ToTable("CaseDuplicateAssessments");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.MatchReasons)
                .HasMaxLength(2000);

            entity.Property(x => x.DecisionRemarks)
                .HasMaxLength(2000);

            entity.Property(x => x.AssessedByUserId)
                .HasMaxLength(450);

            entity.HasOne(x => x.PvCase)
                .WithMany()
                .HasForeignKey(x => x.PvCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.PotentialDuplicatePvCase)
                .WithMany()
                .HasForeignKey(x => x.PotentialDuplicatePvCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PvCaseId);
            entity.HasIndex(x => x.PotentialDuplicatePvCaseId);
        });

        builder.Entity<CaseFollowUp>(entity =>
        {
            entity.ToTable("CaseFollowUps");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.FollowUpNo)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Source)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ReceivedFrom)
                .HasMaxLength(250);

            entity.Property(x => x.Description)
                .HasMaxLength(4000)
                .IsRequired();

            entity.Property(x => x.AdditionalInformation)
                .HasMaxLength(4000);

            entity.Property(x => x.ProcessedRemarks)
                .HasMaxLength(4000);

            entity.Property(x => x.ProcessedByUserId)
                .HasMaxLength(450);

            entity.HasOne(x => x.PvCase)
                .WithMany()
                .HasForeignKey(x => x.PvCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PvCaseId);
            entity.HasIndex(x => x.FollowUpNo);
            entity.HasIndex(x => x.ReceiptDate);
            entity.HasIndex(x => x.IsProcessed);
        });

        builder.Entity<CaseRegulatorySubmission>(entity =>
        {
            entity.ToTable("CaseRegulatorySubmissions");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.SubmissionNo)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.SubmissionType)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.RecipientAuthority)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.SubmissionFormat)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.ReferenceNo)
                .HasMaxLength(200);

            entity.Property(x => x.SubmittedByUserId)
                .HasMaxLength(450);

            entity.Property(x => x.AcknowledgementRemarks)
                .HasMaxLength(4000);

            entity.Property(x => x.Remarks)
                .HasMaxLength(4000);

            entity.Property(x => x.OriginalFileName)
                .HasMaxLength(255);

            entity.Property(x => x.StoredFileName)
                .HasMaxLength(255);

            entity.Property(x => x.FilePath)
                .HasMaxLength(1000);

            entity.Property(x => x.ContentType)
                .HasMaxLength(200);

            entity.HasOne(x => x.PvCase)
                .WithMany()
                .HasForeignKey(x => x.PvCaseId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.PvCaseId);
            entity.HasIndex(x => x.SubmissionNo);
            entity.HasIndex(x => x.SubmissionStatus);
            entity.HasIndex(x => x.DueDate);
            entity.HasIndex(x => x.SubmittedDate);
        });
    }
}
