namespace InnoPV.Web.Services.Security;

public static class PermissionActions
{
    public const string AdminOnly = "access.admin.only";
    public const string AdminOrPvManager = "access.admin.or.pvmanager";
    public const string AdminOrPvAssociate = "access.admin.or.pvassociate";
    public const string AdminOrPvManagerOrMedicalReviewer = "access.admin.or.pvmanager.or.medicalreviewer";
    public const string AdminOrPvAssociateOrPvManager = "access.admin.or.pvassociate.or.pvmanager";
    public const string AuthenticatedPvUser = "access.all.pv.roles";
    public const string AdminOrMedicalReviewer = "access.admin.or.medicalreviewer";

    public const string ViewCase = "case.view";
    public const string EditCase = "case.edit";
    public const string ProcessWorkflow = "case.workflow.process";
    public const string UploadAttachment = "case.attachment.upload";

    public const string DuplicateCheckView = "duplicate.view";
    public const string DuplicateDecision = "duplicate.decision";

    public const string RegulatorySubmissionView = "regsubmission.view";
    public const string RegulatorySubmissionCreate = "regsubmission.create";
    public const string RegulatorySubmissionSubmit = "regsubmission.submit";
    public const string RegulatorySubmissionAcknowledge = "regsubmission.ack";
    public const string RegulatorySubmissionDownload = "regsubmission.download";
    public const string RegulatorySubmissionValidate = "regsubmission.validate";
    public const string RegulatorySubmissionExportValidation = "regsubmission.validation.export";
}
