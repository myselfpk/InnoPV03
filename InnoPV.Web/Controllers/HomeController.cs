using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InnoPV.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        if (User.IsInRole("Admin") || User.IsInRole("PV Manager"))
        {
            return RedirectToAction("Index", "PvDashboard");
        }

        return RedirectToAction("Index", "CaseInbox");
    }

    [AllowAnonymous]
    public IActionResult Privacy()
    {
        return View();
    }
}