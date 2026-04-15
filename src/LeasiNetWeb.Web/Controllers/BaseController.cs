using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeasiNetWeb.Web.Controllers;

[Authorize]
public abstract class BaseController : Controller
{
    protected int AktuellerBenutzerId =>
        int.Parse(User.FindFirst("BenutzerId")?.Value ?? "0");

    protected string AktuellerBenutzername =>
        User.FindFirst("Benutzername")?.Value ?? string.Empty;

    protected string AktuelleRolle =>
        User.FindFirst("Rolle")?.Value ?? string.Empty;

    protected bool IstAdministrator => AktuelleRolle == "Administrator";
    protected bool IstGenehmiger => AktuelleRolle is "Genehmiger" or "Administrator";
}
