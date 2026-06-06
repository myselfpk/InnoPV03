using InnoPV.Web.Models.ClosureValidation;

namespace InnoPV.Web.Services.CaseClosure;

public interface ICaseClosureValidationService
{
    Task<CaseClosureValidationViewModel> ValidateCaseForClosureAsync(long caseId);
}