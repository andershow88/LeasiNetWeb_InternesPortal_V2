using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Web.Controllers;

public class NachrichtenController : BaseController
{
    private readonly INachrichtService _nachrichten;
    private readonly IApplicationDbContext _db;

    public NachrichtenController(INachrichtService nachrichten, IApplicationDbContext db)
    {
        _nachrichten = nachrichten;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var posteingang = await _nachrichten.GetPosteingang(AktuellerBenutzerId);
        return View(posteingang);
    }

    public async Task<IActionResult> Details(int id)
    {
        var nachricht = await _nachrichten.GetNachricht(id, AktuellerBenutzerId);
        if (nachricht is null) return NotFound();

        await _nachrichten.AlsGelesenMarkieren(id, AktuellerBenutzerId);
        return View(nachricht);
    }

    public async Task<IActionResult> Neu(int? antragId = null)
    {
        await FuelleEmpfaengerDropdown();
        return View(new NachrichtSendenViewModel { LeasingantragId = antragId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Neu(NachrichtSendenViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await FuelleEmpfaengerDropdown();
            return View(model);
        }

        await _nachrichten.Senden(
            new NachrichtSendenDto(model.EmpfaengerId, model.LeasingantragId, model.Betreff, model.Text),
            AktuellerBenutzerId);

        return RedirectToAction(nameof(Index));
    }

    // AJAX endpoint — replaces ajax_updateEntryRead.groovy from Intrexx
    [HttpPost]
    public async Task<IActionResult> AlsGelesenMarkieren([FromBody] int nachrichtId)
    {
        await _nachrichten.AlsGelesenMarkieren(nachrichtId, AktuellerBenutzerId);
        return Ok();
    }

    private async Task FuelleEmpfaengerDropdown()
    {
        var benutzer = await _db.Benutzer
            .Where(b => b.IstAktiv && b.Id != AktuellerBenutzerId)
            .OrderBy(b => b.Nachname)
            .Select(b => new { b.Id, Name = b.Vorname + " " + b.Nachname })
            .ToListAsync();

        ViewBag.Empfaenger = new SelectList(benutzer, "Id", "Name");
    }
}
