using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Enums;
using LeasiNetWeb.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Web.Controllers;

public class AntraegeController : BaseController
{
    private readonly ILeasingantragService _antraege;
    private readonly IKommentarService _kommentare;
    private readonly IAnhangService _anhaenge;
    private readonly IApplicationDbContext _db;

    public AntraegeController(ILeasingantragService antraege, IKommentarService kommentare,
        IAnhangService anhaenge, IApplicationDbContext db)
    {
        _antraege = antraege;
        _kommentare = kommentare;
        _anhaenge = anhaenge;
        _db = db;
    }

    // ── Übersicht ──────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index(AntragStatus? status = null)
    {
        var liste = await _antraege.GetAlleAntraege(status: status);
        ViewBag.AktuellerStatus = status;
        return View(liste);
    }

    public async Task<IActionResult> MeinePruefungen()
    {
        ViewBag.SeitenTitel      = "Meine Prüfungen";
        ViewBag.SeitenUntertitel = "Anträge, die auf Ihre Prüfung warten";
        ViewBag.SeitenIcon       = "bi-clipboard-check";
        return View("Index", await _antraege.GetMeineZuPruefendenAntraege(AktuellerBenutzerId));
    }

    public async Task<IActionResult> MeineGenehmigungen()
    {
        ViewBag.SeitenTitel      = "Genehmigungen";
        ViewBag.SeitenUntertitel = "Anträge, die Ihre Genehmigung erfordern";
        ViewBag.SeitenIcon       = "bi-check2-square";
        return View("Index", await _antraege.GetMeineZuGenehmigendenAntraege(AktuellerBenutzerId));
    }

    public async Task<IActionResult> MeineZweiteVoten()
    {
        ViewBag.SeitenTitel      = "2. Vote";
        ViewBag.SeitenUntertitel = "Anträge, bei denen ein zweites Votum erforderlich ist";
        ViewBag.SeitenIcon       = "bi-person-check";
        return View("Index", await _antraege.GetMeineZweiteVotenAntraege(AktuellerBenutzerId));
    }

    public async Task<IActionResult> Archiv()
    {
        ViewBag.SeitenTitel      = "Archiv";
        ViewBag.SeitenUntertitel = "Archivierte Leasinganträge";
        ViewBag.SeitenIcon       = "bi-archive";
        var alle = await _db.Leasingantraege
            .Include(a => a.EingereichtVon)
            .Include(a => a.Leasinggesellschaft)
            .Include(a => a.SachbearbeiterMB)
            .Where(a => a.Archiviert)
            .OrderByDescending(a => a.ArchiviertAm)
            .Select(a => new AntragListeDto(a.Id, a.AntragNummer, a.AntragTyp, a.Status,
                a.EingereichtVon.Anzeigename, a.Leasinggesellschaft != null ? a.Leasinggesellschaft.Name : null,
                a.SachbearbeiterMB != null ? a.SachbearbeiterMB.Anzeigename : null,
                a.Obligo, a.ErstelltAm, a.GeaendertAm, a.ZweiteVoteErforderlich))
            .ToListAsync();
        return View("Index", alle);
    }

    // ── Detail ─────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Details(int id)
    {
        var detail = await _antraege.GetAntragDetail(id);
        if (detail is null) return NotFound();

        var vertragId = await _db.Vertraege
            .Where(v => v.LeasingantragId == id)
            .Select(v => (int?)v.Id)
            .FirstOrDefaultAsync();
        ViewBag.VertragId = vertragId;

        var ablehnungsgruende = await _db.Ablehnungsgruende
            .Where(ag => ag.IstAktiv)
            .OrderBy(ag => ag.Bezeichnung)
            .Select(ag => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem(ag.Bezeichnung, ag.Id.ToString()))
            .ToListAsync();
        ViewBag.Ablehnungsgruende = ablehnungsgruende;

        return View(detail);
    }

    // ── Neu anlegen ────────────────────────────────────────────────────────────

    public async Task<IActionResult> Neu()
    {
        await FuelleDropdowns();
        return View(new AntragErstellenViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Neu(AntragErstellenViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await FuelleDropdowns();
            return View(model);
        }

        var id = await _antraege.ErstelleAntrag(
            new AntragErstellenDto(model.AntragTyp, model.LeasinggesellschaftId, model.Obligo, model.Abrechnungsart),
            AktuellerBenutzerId);

        return RedirectToAction(nameof(Details), new { id });
    }

    // ── Status-Aktionen ────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Einreichen(int id)
    {
        await _antraege.StatusWechsel(id, AntragStatus.Eingereicht, AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> InPruefung(int id)
    {
        await _antraege.StatusWechsel(id, AntragStatus.InPruefung, AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PruefungAbschliessen(int id)
    {
        if (AktuelleRolle is not ("SachbearbeiterMB" or "Administrator"))
            return Forbid();
        await _antraege.StatusWechsel(id, AntragStatus.BeiMitarbeiter, AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Genehmigen(int id)
    {
        if (!IstGenehmiger) return Forbid();
        await _antraege.Genehmigen(id, AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ablehnen(int id, int ablehnungsgrundId, string? kommentar)
    {
        if (!IstGenehmiger) return Forbid();
        await _antraege.Ablehnen(id, AktuellerBenutzerId, ablehnungsgrundId, kommentar);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ZweiteVoteAnfordern(int id)
    {
        if (!IstGenehmiger) return Forbid();
        await _antraege.ZweiteVoteAnfordern(id, AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Archivieren(int id)
    {
        await _antraege.Archivieren(id);
        return RedirectToAction(nameof(Index));
    }

    // ── Kommentar ──────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> KommentarHinzufuegen(int antragId, string text, bool istIntern)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            TempData["Fehler"] = "Kommentar darf nicht leer sein.";
            return RedirectToAction(nameof(Details), new { id = antragId });
        }

        await _kommentare.HinzufuegenAsync(new KommentarErstellenDto(antragId, text, istIntern),
            AktuellerBenutzerId);
        return RedirectToAction(nameof(Details), new { id = antragId });
    }

    // ── Anhang ─────────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AnhangHochladen(int antragId, IFormFile datei)
    {
        if (datei is null || datei.Length == 0)
        {
            TempData["Fehler"] = "Bitte wählen Sie eine Datei aus.";
            return RedirectToAction(nameof(Details), new { id = antragId });
        }

        try
        {
            await using var stream = datei.OpenReadStream();
            await _anhaenge.HochladenAsync(stream, datei.FileName, datei.ContentType,
                datei.Length, AnhangTyp.Antragsdokument,
                AktuellerBenutzerId, antragId: antragId);
        }
        catch (InvalidOperationException ex)
        {
            TempData["Fehler"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id = antragId });
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

    private async Task FuelleDropdowns()
    {
        var gesellschaften = await _db.Leasinggesellschaften
            .Where(l => l.IstAktiv)
            .OrderBy(l => l.Name)
            .Select(l => new { l.Id, l.Name })
            .ToListAsync();

        ViewBag.Leasinggesellschaften = new SelectList(gesellschaften, "Id", "Name");
        ViewBag.AntragTypen = Enum.GetValues<AntragTyp>()
            .Select(t => new SelectListItem(t.ToString(), ((int)t).ToString()));
    }
}
