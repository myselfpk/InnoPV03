using InnoPV.Domain.Identity;
using InnoPV.Web.Services.SlaAlerts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace InnoPV.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AuthenticatedPvUser)]
public class SlaAlertController : Controller
{
    private readonly ISlaAlertService _slaAlertService;

    public SlaAlertController(ISlaAlertService slaAlertService)
    {
        _slaAlertService = slaAlertService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var model = User.IsInRole(AppRoles.Admin)
            ? await _slaAlertService.GetPreviewAsync()
            : await _slaAlertService.GetPreviewForUserAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));

        return View(model);
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOrPvManager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendDueSoonAlerts()
    {
        var result = await _slaAlertService.SendDueSoonAlertsAsync();

        TempData["SuccessMessage"] =
            $"Due soon alerts processed. T-7: {result.T7CaseCount}, T-3: {result.T3CaseCount}, T-1: {result.T1CaseCount}, Recipients: {result.EmailRecipientCount}.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOrPvManager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendOverdueAlerts()
    {
        var result = await _slaAlertService.SendOverdueAlertsAsync();

        TempData["SuccessMessage"] =
            $"Overdue alerts processed. Cases: {result.OverdueCaseCount}, Recipients: {result.EmailRecipientCount}.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Policy = AuthorizationPolicies.AdminOrPvManager)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SendAllAlerts()
    {
        var result = await _slaAlertService.SendAllAlertsAsync();

        TempData["SuccessMessage"] =
            $"All SLA alerts processed. T-7: {result.T7CaseCount}, T-3: {result.T3CaseCount}, T-1: {result.T1CaseCount}, Overdue: {result.OverdueCaseCount}, Recipients: {result.EmailRecipientCount}.";

        return RedirectToAction(nameof(Index));
    }
}
