using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Enums;
using LeasiNetWeb.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Web.Controllers;

public class VertraegeController : BaseController
{
    private readonly IVertragService _vertraege;
    private readonly IAnhangService _anhaenge;
    private readonly IApplicationDbContext _db;

    public VertraegeController(IVertragService vertraege, IAnhangService anhaenge, IApplicationDbContext db)
    {
        _vertraege = vertraege;
        _anhaenge = anhaenge;
        _db = db;
    }

    // ── Übersicht ──────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(VertragStatus? status = null)
    {
        var liste = await _vertraege.GetAlleVertraege(status);
        ViewBag.AktuellerStatus = status;
        return View(liste);
    }

    // ── Detail ─────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Details(int id)
    {
        var detail = await _vertraege.GetVertragDetail(id);
        if (detail is null) return NotFound();
        return View(detail);
    }

    // ── Neu anlegen aus Antrag ─────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Erstellen(int antragId)
    {
        try
        {
            var id = await _vertraege.ErstellenAsync(antragId, AktuellerBenutzerId);
            TempData["Erfolg"] = "Vertrag erfolgreich angelegt.";
            return RedirectToAction(nameof(Details), new { id });
        }
        catch (Exception ex)
        {
            TempData["Fehler"] = ex.Message;
            return RedirectToAction("Details", "Antraege", new { id = antragId });
        }
    }

    // ── Bearbeiten ─────────────────────────────────────────────────────────────

    public async Task<IActionResult> Bearbeiten(int id)
    {
        var detail = await _vertraege.GetVertragDetail(id);
        if (detail is null) return NotFound();

        var vm = new VertragBearbeitenViewModel
        {
            Id = detail.Id,
            VertragNummer = detail.VertragNummer,
            LeasingantragId = detail.LeasingantragId,
            AntragNummer = detail.AntragNummer,
            VertragtypId = detail.VertragtypId,
            Vertragsbeginn = detail.Vertragsbeginn,
            Vertragsende = detail.Vertragsende,
            LaufzeitMonate = detail.LaufzeitMonate,
            Finanzierungsbetrag = detail.Finanzierungsbetrag,
            Restwert = detail.Restwert,
            MonatlicheRate = detail.MonatlicheRate,
            Zinssatz = detail.Zinssatz,
            Vertragstypen = await GetVertragstypenSelectList()
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Bearbeiten(VertragBearbeitenViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Vertragstypen = await GetVertragstypenSelectList();
            return View(model);
        }

        await _vertraege.AktualisiereVertrag(model.Id, new VertragAktualisierenDto(
            model.VertragtypId,
            model.Vertragsbeginn,
            model.Vertragsende,
            model.LaufzeitMonate,
            model.Finanzierungsbetrag,
            model.Restwert,
            model.MonatlicheRate,
            model.Zinssatz
        ), AktuellerBenutzerId);

        TempData["Erfolg"] = "Vertrag gespeichert.";
        return RedirectToAction(nameof(Details), new { id = model.Id });
    }

    // ── Status-Aktionen ────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aktivieren(int id)
    {
        await StatusAktion(id, VertragStatus.Aktiv);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Beenden(int id)
    {
        await StatusAktion(id, VertragStatus.Beendet);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Kuendigen(int id)
    {
        await StatusAktion(id, VertragStatus.Gekuendigt);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archivieren(int id)
    {
        await StatusAktion(id, VertragStatus.Archiviert);
        return RedirectToAction(nameof(Index));
    }

    // ── Anhang ─────────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnhangHochladen(int vertragId, IFormFile datei)
    {
        if (datei is null || datei.Length == 0)
        {
            TempData["Fehler"] = "Bitte wählen Sie eine Datei aus.";
            return RedirectToAction(nameof(Details), new { id = vertragId });
        }

        try
        {
            await using var stream = datei.OpenReadStream();
            await _anhaenge.HochladenAsync(stream, datei.FileName, datei.ContentType,
                datei.Length, AnhangTyp.Vertragsdokument,
                AktuellerBenutzerId, vertragId: vertragId);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Fehler"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = vertragId });
    }

    [HttpGet]
    public async Task<IActionResult> AnhangHerunterladen(int anhangId)
    {
        try
        {
            var (inhalt, contentType, dateiname) = await _anhaenge.HerunterladenAsync(anhangId);
            return File(inhalt, contentType, dateiname);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    // ── Private helpers ────────────────────────────────────────────────────────

    private async Task StatusAktion(int id, VertragStatus neuerStatus)
    {
        try
        {
            await _vertraege.StatusWechsel(id, neuerStatus, AktuellerBenutzerId);
            TempData["Erfolg"] = $"Vertragsstatus geändert zu: {neuerStatus}.";
        }
        catch (Exception ex)
        {
            TempData["Fehler"] = ex.Message;
        }
    }

    private async Task<IEnumerable<SelectListItem>> GetVertragstypenSelectList()
    {
        var typen = await _db.Vertragstypen
            .Where(t => t.IstAktiv)
            .OrderBy(t => t.Bezeichnung)
            .Select(t => new SelectListItem(t.Bezeichnung, t.Id.ToString()))
            .ToListAsync();
        return typen;
    }
}
