using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LeasiNetWeb.Web.Controllers;

[Authorize(Policy = "InternerPruefer")]
public class InternePruefungController : BaseController
{
    private readonly IInternePruefungService _pruefung;
    private readonly IAnhangService _anhaenge;

    public InternePruefungController(IInternePruefungService pruefung, IAnhangService anhaenge)
    {
        _pruefung = pruefung;
        _anhaenge = anhaenge;
    }

    // ── Übersicht ──────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index()
    {
        var liste = IstAdministrator
            ? await _pruefung.GetAllePruefungenAsync()
            : await _pruefung.GetMeinePruefungenAsync(AktuellerBenutzerId);
        return View(liste);
    }

    // ── Wizard-Daten für Antrag-Details ───────────────────────────────────────

    [HttpGet]
    [AllowAnonymous]  // ViewComponent im Antraege-Context — Authz via Antraege-Controller
    public async Task<IActionResult> WizardDaten(int antragId)
    {
        var daten = await _pruefung.GetWizardDatenAsync(antragId);
        if (daten is null) return NotFound();
        return Json(daten);
    }

    // ── Detail / Checkliste ────────────────────────────────────────────────────

    public async Task<IActionResult> Details(int antragId)
    {
        var dto = await _pruefung.GetByAntragIdAsync(antragId);
        if (dto is null)
        {
            TempData["Fehler"] = "Keine interne Prüfung für diesen Antrag gefunden.";
            return RedirectToAction("Details", "Antraege", new { id = antragId });
        }
        ViewData["AntragId"] = antragId;
        return View(dto);
    }

    // ── Protokoll-Druckansicht ────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> Protokoll(int antragId)
    {
        var dto = await _pruefung.GetByAntragIdAsync(antragId);
        if (dto is null) return NotFound();
        return View(dto);
    }

    // ── Starten (Wizard-Ergebnis) ─────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Starten(
        int antragId,
        [FromForm] List<int> prueferIds,
        [FromForm] List<string> prueferBezeichnungen)
    {
        try
        {
            var schritte = prueferIds
                .Select((id, i) => new PruefungsSchrittInput(
                    id,
                    i < prueferBezeichnungen.Count ? prueferBezeichnungen[i] : $"{i + 1}. Prüfer"))
                .ToList();

            var hauptPruefer = schritte.Count > 0 ? schritte[0].PrueferMBId : AktuellerBenutzerId;
            await _pruefung.StartenAsync(antragId, hauptPruefer, schritte);
            TempData["Erfolg"] = "Interne Prüfung gestartet.";
        }
        catch (Exception ex)
        {
            TempData["Fehler"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { antragId });
    }

    // ── Pflicht erfüllen ───────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PflichtErfuellen(int pflichtId, int antragId, string? bemerkungen)
    {
        await _pruefung.PflichtErfuellenAsync(pflichtId, bemerkungen, AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { antragId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PflichtRueckgaengig(int pflichtId, int antragId)
    {
        await _pruefung.PflichtRueckgaengigAsync(pflichtId);
        return RedirectToAction(nameof(Details), new { antragId });
    }

    // ── Schritt abschließen ────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SchrittAbschliessen(int schrittId, int antragId, string? ergebnis)
    {
        try
        {
            await _pruefung.SchrittAbschliessenAsync(schrittId, AktuellerBenutzerId, ergebnis);
            TempData["Erfolg"] = "Prüfschritt abgeschlossen.";
        }
        catch (Exception ex)
        {
            TempData["Fehler"] = ex.Message;
        }
        return RedirectToAction(nameof(Details), new { antragId });
    }

    // ── Abschließen ────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Abschliessen(int id, int antragId, string? ergebnis)
    {
        try
        {
            await _pruefung.AbschliessenAsync(id, AktuellerBenutzerId, ergebnis);
            TempData["Erfolg"] = "Interne Prüfung abgeschlossen. Antrag ist wieder bei Mitarbeiter.";
            return RedirectToAction("Details", "Antraege", new { id = antragId });
        }
        catch (Exception ex)
        {
            TempData["Fehler"] = ex.Message;
            return RedirectToAction(nameof(Details), new { antragId });
        }
    }

    // ── Anhang ─────────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnhangHochladen(int pruefungId, int antragId, IFormFile datei)
    {
        if (datei is null || datei.Length == 0)
        {
            TempData["Fehler"] = "Bitte wählen Sie eine Datei aus.";
            return RedirectToAction(nameof(Details), new { antragId });
        }
        try
        {
            await using var stream = datei.OpenReadStream();
            await _anhaenge.HochladenAsync(stream, datei.FileName, datei.ContentType,
                datei.Length, AnhangTyp.Pruefungsdokument, AktuellerBenutzerId, pruefungId: pruefungId);
        }
        catch (InvalidOperationException ex) { TempData["Fehler"] = ex.Message; }
        return RedirectToAction(nameof(Details), new { antragId });
    }

    [HttpGet]
    public async Task<IActionResult> AnhangHerunterladen(int anhangId)
    {
        try
        {
            var (inhalt, contentType, dateiname) = await _anhaenge.HerunterladenAsync(anhangId);
            return File(inhalt, contentType, dateiname);
        }
        catch (KeyNotFoundException) { return NotFound(); }
    }
}
