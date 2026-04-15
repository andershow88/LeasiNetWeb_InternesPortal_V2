using System.Security.Claims;
using System.Text;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Web.ViewModels;
using Markdig;
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

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Dokumentation()
    {
        // Anwenderdokumentation liegt unter docs/Anwenderdokumentation.md im Repo-Root
        // Suche im aktuellen Verzeichnis und bis zu 3 Ebenen nach oben
        var searchDir = Directory.GetCurrentDirectory();
        string? readmePath = null;
        for (int i = 0; i < 4; i++)
        {
            var candidate = Path.Combine(searchDir, "docs", "Anwenderdokumentation.md");
            if (System.IO.File.Exists(candidate)) { readmePath = candidate; break; }
            var parent = Directory.GetParent(searchDir)?.FullName;
            if (parent == null) break;
            searchDir = parent;
        }
        if (readmePath == null)
            return NotFound("Anwenderdokumentation.md nicht gefunden.");

        var markdownText = System.IO.File.ReadAllText(readmePath);
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var htmlBody = Markdown.ToHtml(markdownText, pipeline);

        var fullHtml = $$"""
            <!DOCTYPE html>
            <html lang="de">
            <head>
                <meta charset="utf-8" />
                <title>Anwenderdokumentation – LeasiNetWeb Internes Portal</title>
                <style>
                    body {
                        font-family: 'Segoe UI', Arial, sans-serif;
                        max-width: 860px;
                        margin: 40px auto;
                        padding: 0 24px 60px;
                        color: #1a1a2e;
                        line-height: 1.7;
                    }
                    h1, h2, h3, h4 { color: #1a1a2e; margin-top: 1.5em; }
                    h1 { border-bottom: 2px solid #c8a96e; padding-bottom: 6px; }
                    h2 { border-bottom: 1px solid #e0e0e0; padding-bottom: 4px; }
                    code { background: #f4f4f4; padding: 2px 6px; border-radius: 3px; font-size: 0.88em; }
                    pre { background: #f4f4f4; padding: 14px; border-radius: 6px; overflow-x: auto; }
                    pre code { background: none; padding: 0; }
                    table { border-collapse: collapse; width: 100%; margin: 1em 0; }
                    th, td { border: 1px solid #d0d0d0; padding: 8px 12px; text-align: left; }
                    th { background: #f0f0f0; font-weight: 600; }
                    blockquote { border-left: 4px solid #c8a96e; margin: 0; padding: 8px 16px; background: #fdf9f2; }
                    a { color: #c8a96e; }
                    img { max-width: 100%; }
                    @media print {
                        body { margin: 0; padding: 20px; }
                        .no-print { display: none; }
                    }
                </style>
            </head>
            <body>
                <div class="no-print" style="background:#1a1a2e;color:#fff;padding:10px 20px;margin:-40px -24px 30px;display:flex;align-items:center;gap:12px;">
                    <span style="font-weight:600;">LeasiNetWeb – Anwenderdokumentation</span>
                    <button onclick="window.print()" style="margin-left:auto;background:#c8a96e;color:#fff;border:none;padding:6px 16px;border-radius:4px;cursor:pointer;font-size:0.9rem;">Als PDF speichern</button>
                </div>
                {{htmlBody}}
                <script>window.print();</script>
            </body>
            </html>
            """;

        return Content(fullHtml, "text/html", Encoding.UTF8);
    }
}
