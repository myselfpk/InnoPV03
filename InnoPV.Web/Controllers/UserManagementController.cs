using InnoPV.Domain.Identity;
using InnoPV.Web.Models.UserManagement;
using InnoPV.Web.Services.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InnoPV.Web.Controllers;

[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class UserManagementController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IAppEmailSender _emailSender;

    public UserManagementController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IAppEmailSender emailSender)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailSender = emailSender;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var users = _userManager.Users
            .OrderBy(x => x.FullName)
            .ThenBy(x => x.Email)
            .ToList();

        var model = new List<UserListItemViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);

            model.Add(new UserListItemViewModel
            {
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                MobileNo = user.PhoneNumber,
                Designation = user.Designation,
                Department = user.Department,
                Role = roles.FirstOrDefault() ?? string.Empty,
                IsActive = user.IsActive,
                MustChangePassword = user.MustChangePassword,
                LockoutEnd = user.LockoutEnd
            });
        }

        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new CreateUserViewModel
        {
            SendCredentialsByEmail = true
        };

        await LoadRoleOptionsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserViewModel model)
    {
        await LoadRoleOptionsAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email.Trim());

        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Email), "A user with this email already exists.");
            return View(model);
        }

        var temporaryPassword = GenerateTemporaryPassword();

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true,
            PhoneNumber = model.MobileNo.Trim(),
            PhoneNumberConfirmed = true,
            FullName = model.FullName.Trim(),
            Designation = model.Designation?.Trim(),
            Department = model.Department?.Trim(),
            IsActive = true,
            MustChangePassword = true,
            LockoutEnabled = true,
            CreatedOnUtc = DateTime.UtcNow
        };

        var createResult = await _userManager.CreateAsync(user, temporaryPassword);

        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, model.Role);

        if (!roleResult.Succeeded)
        {
            foreach (var error in roleResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        if (model.SendCredentialsByEmail)
        {
            await SendCredentialEmailAsync(user, temporaryPassword);
        }

        TempData["SuccessMessage"] = model.SendCredentialsByEmail
            ? "User created successfully and credentials have been emailed."
            : "User created successfully.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["ErrorMessage"] = "User id is required.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var roles = await _userManager.GetRolesAsync(user);

        var model = new EditUserViewModel
        {
            UserId = user.Id,
            FullName = user.FullName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            MobileNo = user.PhoneNumber ?? string.Empty,
            Designation = user.Designation,
            Department = user.Department,
            Role = roles.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive,
            MustChangePassword = user.MustChangePassword
        };

        await LoadRoleOptionsAsync(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditUserViewModel model)
    {
        await LoadRoleOptionsAsync(model);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId);

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        var email = model.Email.Trim();

        var userWithSameEmail = await _userManager.FindByEmailAsync(email);

        if (userWithSameEmail != null && userWithSameEmail.Id != user.Id)
        {
            ModelState.AddModelError(nameof(model.Email), "Another user with this email already exists.");
            return View(model);
        }

        user.FullName = model.FullName.Trim();
        user.Email = email;
        user.UserName = email;
        user.NormalizedEmail = email.ToUpperInvariant();
        user.NormalizedUserName = email.ToUpperInvariant();
        user.PhoneNumber = model.MobileNo.Trim();
        user.Designation = model.Designation?.Trim();
        user.Department = model.Department?.Trim();
        user.IsActive = model.IsActive;
        user.MustChangePassword = model.MustChangePassword;

        if (!model.IsActive)
        {
            user.LockoutEnd = DateTimeOffset.UtcNow.AddYears(100);
        }
        else
        {
            user.LockoutEnd = null;
        }

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            foreach (var error in updateResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        var existingRoles = await _userManager.GetRolesAsync(user);

        if (existingRoles.Any())
        {
            await _userManager.RemoveFromRolesAsync(user, existingRoles);
        }

        await _userManager.AddToRoleAsync(user, model.Role);

        TempData["SuccessMessage"] = "User details updated successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ShareCredentials(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            TempData["ErrorMessage"] = "User id is required.";
            return RedirectToAction(nameof(Index));
        }

        var user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            TempData["ErrorMessage"] = "User not found.";
            return RedirectToAction(nameof(Index));
        }

        if (!user.IsActive)
        {
            TempData["ErrorMessage"] = "Credentials cannot be shared because the user is inactive.";
            return RedirectToAction(nameof(Index));
        }

        var temporaryPassword = GenerateTemporaryPassword();

        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        var resetResult = await _userManager.ResetPasswordAsync(
            user,
            resetToken,
            temporaryPassword);

        if (!resetResult.Succeeded)
        {
            foreach (var error in resetResult.Errors)
            {
                TempData["ErrorMessage"] = error.Description;
                break;
            }

            return RedirectToAction(nameof(Index));
        }

        user.MustChangePassword = true;
        await _userManager.UpdateAsync(user);

        await SendCredentialEmailAsync(user, temporaryPassword);

        TempData["SuccessMessage"] = $"New temporary credentials have been shared with {user.Email}.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SendCredentialEmailAsync(ApplicationUser user, string temporaryPassword)
    {
        var loginUrl = Url.Action(
            "Login",
            "Account",
            null,
            Request.Scheme) ?? string.Empty;

        var subject = "InnoPV Login Credentials";

        var body = UserCredentialEmailTemplate.BuildCredentialEmailBody(
            user.FullName ?? user.Email ?? "User",
            user.Email ?? string.Empty,
            temporaryPassword,
            loginUrl);

        await _emailSender.SendEmailAsync(
            user.Email ?? string.Empty,
            subject,
            body);
    }

    private static string GenerateTemporaryPassword()
    {
        var random = Guid.NewGuid().ToString("N")[..8];

        return $"InnoPV@{random}1";
    }

    private async Task LoadRoleOptionsAsync(CreateUserViewModel model)
    {
        model.RoleOptions = await GetRoleOptionsAsync();
    }

    private async Task LoadRoleOptionsAsync(EditUserViewModel model)
    {
        model.RoleOptions = await GetRoleOptionsAsync();
    }

    private async Task<List<SelectListItem>> GetRoleOptionsAsync()
    {
        var roles = _roleManager.Roles
            .OrderBy(x => x.Name)
            .Select(x => x.Name!)
            .ToList();

        return await Task.FromResult(
            roles.Select(x => new SelectListItem
            {
                Value = x,
                Text = x
            }).ToList());
    }
}