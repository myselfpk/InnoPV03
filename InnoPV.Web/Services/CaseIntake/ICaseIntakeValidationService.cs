using InnoPV.Domain.Entities;
using InnoPV.Web.Models.PvCase;

namespace InnoPV.Web.Services.CaseIntake;

public interface ICaseIntakeValidationService
{
    CaseIntakeValidationResult Validate(PvCaseIntakeViewModel model);
    int CalculateCompletenessScore(PvCase pvCase);
}
