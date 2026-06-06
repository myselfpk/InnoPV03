using InnoPV.Domain.Identity;
using InnoPV.Web.Models.Account;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using InnoPV.Web.Services.Email;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace InnoPV.Web.Controllers;

public class AccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AccountController> _logger;
    private readonly IAppEmailSender _emailSender;

    public AccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AccountController> logger,
        IAppEmailSender emailSender)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
        _emailSender = emailSender;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Dashboard");
        }

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return View(model);
        }

        if (!user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "Your account is inactive. Please contact administrator.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
        {
            user.LastLoginOnUtc = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            _logger.LogInformation("User {Email} logged in successfully.", model.Email);

            if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
            {
                return Redirect(model.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Your account has been locked due to multiple failed login attempts. Please try again later or contact administrator.");
            return View(model);
        }

        if (result.RequiresTwoFactor)
        {
            ModelState.AddModelError(string.Empty, "Two-factor authentication is required. This feature will be configured in the next security phase.");
            return View(model);
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var email = model.Email.Trim();

        var user = await _userManager.FindByEmailAsync(email);

        /*
          Security note:
          We do not reveal whether the email exists or not.
          This prevents user enumeration.
        */
        if (user == null || !user.IsActive)
        {
            TempData["SuccessMessage"] = "If the email is registered, password reset instructions will be sent.";
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        var encodedToken = WebEncoders.Base64UrlEncode(
            Encoding.UTF8.GetBytes(token));

        var resetLink = Url.Action(
            nameof(ResetPassword),
            "Account",
            new
            {
                userId = user.Id,
                token = encodedToken
            },
            Request.Scheme);

        var body = ForgotPasswordEmailTemplate.BuildForgotPasswordEmailBody(
            user.FullName ?? user.Email ?? "User",
            resetLink ?? string.Empty);

        await _emailSender.SendEmailAsync(
            user.Email ?? string.Empty,
            "InnoPV Password Reset Request",
            body);

        TempData["SuccessMessage"] = "If the email is registered, password reset instructions will be sent.";
        return RedirectToAction(nameof(ForgotPasswordConfirmation));
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }

    [HttpGet]
    public IActionResult ResetPassword(string userId, string token)
    {
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
        {
            TempData["ErrorMessage"] = "Invalid password reset link.";
            return RedirectToAction(nameof(Login));
        }

        var model = new ResetPasswordViewModel
        {
            UserId = userId,
            Token = token
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByIdAsync(model.UserId);

        if (user == null || !user.IsActive)
        {
            TempData["ErrorMessage"] = "Invalid password reset request.";
            return RedirectToAction(nameof(Login));
        }

        var decodedToken = Encoding.UTF8.GetString(
            WebEncoders.Base64UrlDecode(model.Token));

        var result = await _userManager.ResetPasswordAsync(
            user,
            decodedToken,
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
        user.LockoutEnd = null;

        await _userManager.UpdateAsync(user);

        TempData["SuccessMessage"] = "Your password has been reset successfully. Please login with your new password.";
        return RedirectToAction(nameof(Login));
    }
}