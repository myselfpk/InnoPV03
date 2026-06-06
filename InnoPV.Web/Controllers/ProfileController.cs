using InnoPV.Domain.Identity;
using InnoPV.Web.Models.Profile;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace InnoPV.Web.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var model = new UserProfileViewModel
        {
            FullName = user.FullName ?? user.Email ?? string.Empty,
            Email = user.Email ?? string.Empty,
            MobileNo = user.PhoneNumber,
            Designation = user.Designation,
            Department = user.Department,
            Role = roles.FirstOrDefault() ?? string.Empty,
            MustChangePassword = user.MustChangePassword
        };

        return View(model);
    }

    [HttpGet]
    public IActionResult ChangePassword(bool force = false)
    {
        var model = new ChangePasswordViewModel
        {
            IsForcedChange = force
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.CurrentPassword == model.NewPassword)
        {
            ModelState.AddModelError(nameof(model.NewPassword), "New password should not be same as current password.");
            return View(model);
        }

        var user = await _userManager.GetUserAsync(User);

        if (user == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var result = await _userManager.ChangePasswordAsync(
            user,
            model.CurrentPassword,
            model.NewPassword);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        user.MustChangePassword = false;

        await _userManager.UpdateAsync(user);

        await _signInManager.RefreshSignInAsync(user);

        TempData["SuccessMessage"] = "Password changed successfully.";

        if (model.IsForcedChange)
        {
            return RedirectToAction("Index", "CaseInbox");
        }

        return RedirectToAction(nameof(Index));
    }
}