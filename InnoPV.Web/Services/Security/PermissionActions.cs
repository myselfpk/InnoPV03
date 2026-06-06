namespace InnoPV.Web.Services.Security;

public static class PermissionActions
{
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
