using System.Security.Claims;
using KarateSchool.Web.Models.Entities;
using KarateSchool.Web.Models.ViewModels;
using KarateSchool.Web.Services.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace KarateSchool.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;

    public AccountController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var result = await _authService.LoginAsync(model.Email, model.Password);

        switch (result.Outcome)
        {
            case LoginOutcome.Success:
                await SignInAsync(result.User!, model.RememberMe);
                return RedirectToLocal(model.ReturnUrl);

            case LoginOutcome.LockedOut:
                ModelState.AddModelError(
                    string.Empty,
                    $"This account is locked due to too many failed attempts. Try again after {result.LockoutEnd:t}.");
                break;

            default:
                ModelState.AddModelError(string.Empty, "Invalid email or password.");
                break;
        }

        return View(model);
    }

    [HttpGet]
    public IActionResult Register()
    {
        return View(new RegisterStudentViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterStudentViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _authService.RegisterStudentAsync(model);
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(model.Email), ex.Message);
            return View(model);
        }
        catch (ArgumentException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }

        return RedirectToAction(nameof(RegisterConfirmation));
    }

    [HttpGet]
    public IActionResult RegisterConfirmation() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    private async Task SignInAsync(User user, bool rememberMe)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = rememberMe,
            });
    }

    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);
        return RedirectToAction("Index", "Home");
    }
}
