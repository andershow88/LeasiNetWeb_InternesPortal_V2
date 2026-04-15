using System.Security.Claims;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Web.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeasiNetWeb.Web.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _auth;

    public AccountController(IAuthService auth) => _auth = auth;

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid) return View(model);

        var benutzer = await _auth.ValidateAsync(model.Benutzername, model.Passwort);
        if (benutzer is null)
        {
            ModelState.AddModelError(string.Empty, "Benutzername oder Passwort ungültig.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new("BenutzerId", benutzer.Id.ToString()),
            new("Benutzername", benutzer.Benutzername),
            new(ClaimTypes.Name, benutzer.Anzeigename),
            new("Rolle", benutzer.Rolle.ToString()),
            new(ClaimTypes.Email, benutzer.EMail)
        };

        var identity = new ClaimsIdentity(claims, "LeasiNetWeb.Auth");
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync("LeasiNetWeb.Auth", principal,
            new AuthenticationProperties { IsPersistent = model.MerkenAuf });

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync("LeasiNetWeb.Auth");
        return RedirectToAction("Login");
    }

    [AllowAnonymous]
    public IActionResult ZugriffVerweigert() => View();
}
