using LeasiNetWeb.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Web.Controllers;

public class HilfeController : BaseController
{
    private readonly IApplicationDbContext _db;

    public HilfeController(IApplicationDbContext db) => _db = db;

    public async Task<IActionResult> Index(string? schluessel = null)
    {
        var hilfeTexte = await _db.HilfeTexte
            .Where(h => h.IstAktiv && (schluessel == null || h.Schluessel == schluessel))
            .OrderBy(h => h.Titel)
            .ToListAsync();

        return View(hilfeTexte);
    }
}
