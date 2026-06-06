using InnoPV.Domain.Identity;
using InnoPV.Web.Services.CaseClosure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InnoPV.Web.Controllers;

[Authorize(Roles = AppRoles.Admin + "," + AppRoles.PvManager + "," + AppRoles.MedicalReviewer)]
public class CaseClosureController : Controller
{
    private readonly ICaseClosureValidationService _closureValidationService;

    public CaseClosureController(ICaseClosureValidationService closureValidationService)
    {
        _closureValidationService = closureValidationService;
    }

    [HttpGet]
    public async Task<IActionResult> Validate(long caseId)
    {
        var model = await _closureValidationService.ValidateCaseForClosureAsync(caseId);

        return View(model);
    }
}