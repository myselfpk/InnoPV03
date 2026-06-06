using InnoPV.Web.Models.SubmissionValidation;

namespace InnoPV.Web.Services.SubmissionValidation;

public interface ISubmissionReadinessValidationService
{
    Task<SubmissionReadinessValidationViewModel> ValidateCaseForSubmissionAsync(long caseId);
}
