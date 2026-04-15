using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Enums;
using LeasiNetWeb.Infrastructure.Data;
using LeasiNetWeb.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Web.Controllers;

[Authorize(Policy = "Administrator")]
public class AdminController : BaseController
{
    private readonly IAdminService _admin;
    private readonly ApplicationDbContext _db;

    public AdminController(IAdminService admin, ApplicationDbContext db)
    {
        _admin = admin;
        _db = db;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────────

    public async Task<IActionResult> Index()
    {
        var benutzer = await _admin.GetBenutzerAsync();
        var leasinggesellschaften = await _admin.GetLeasinggesellschaftenAsync();
        ViewBag.AnzahlBenutzer = benutzer.Count();
        ViewBag.AnzahlAktiveBenutzer = benutzer.Count(b => b.IstAktiv);
        ViewBag.AnzahlLG = leasinggesellschaften.Count();
        ViewBag.AnzahlAktiveLG = leasinggesellschaften.Count(l => l.IstAktiv);
        return View();
    }

    // ── Benutzer ──────────────────────────────────────────────────────────────

    public async Task<IActionResult> Benutzer()
    {
        var liste = await _admin.GetBenutzerAsync();
        return View(liste);
    }

    public async Task<IActionResult> BenutzerNeu()
    {
        var vm = await BenutzerFormVmAsync(new BenutzerFormViewModel());
        return View("BenutzerForm", vm);
    }

    public async Task<IActionResult> BenutzerBearbeiten(int id)
    {
        var dto = await _admin.GetBenutzerByIdAsync(id);
        if (dto is null) return NotFound();

        var vm = await BenutzerFormVmAsync(new BenutzerFormViewModel
        {
            Id = dto.Id,
            Benutzername = dto.Benutzername,
            Vorname = dto.Vorname,
            Nachname = dto.Nachname,
            EMail = dto.EMail,
            Rolle = dto.Rolle,
            IstAktiv = dto.IstAktiv,
            LeasinggesellschaftId = dto.LeasinggesellschaftId
        });
        return View("BenutzerForm", vm);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BenutzerSpeichern(BenutzerFormViewModel vm)
    {
        // Passwort ist nur beim Erstellen Pflicht
        if (vm.Id == 0 && string.IsNullOrWhiteSpace(vm.Passwort))
            ModelState.AddModelError(nameof(vm.Passwort), "Passwort ist beim Erstellen erforderlich.");

        if (!ModelState.IsValid)
            return View("BenutzerForm", await BenutzerFormVmAsync(vm));

        try
        {
            if (vm.Id == 0)
            {
                await _admin.BenutzerErstellenAsync(new BenutzerErstellenDto(
                    vm.Benutzername, vm.Vorname, vm.Nachname, vm.EMail,
                    vm.Rolle, vm.IstAktiv, vm.LeasinggesellschaftId, vm.Passwort!));
                TempData["Erfolg"] = $"Benutzer '{vm.Benutzername}' wurde erstellt.";
            }
            else
            {
                await _admin.BenutzerBearbeitenAsync(new BenutzerBearbeitenDto(
                    vm.Id, vm.Benutzername, vm.Vorname, vm.Nachname, vm.EMail,
                    vm.Rolle, vm.IstAktiv, vm.LeasinggesellschaftId,
                    string.IsNullOrWhiteSpace(vm.Passwort) ? null : vm.Passwort));
                TempData["Erfolg"] = $"Benutzer '{vm.Benutzername}' wurde gespeichert.";
            }
            return RedirectToAction(nameof(Benutzer));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View("BenutzerForm", await BenutzerFormVmAsync(vm));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BenutzerDeaktivieren(int id)
    {
        await _admin.BenutzerDeaktivierenAsync(id);
        TempData["Erfolg"] = "Benutzer wurde deaktiviert.";
        return RedirectToAction(nameof(Benutzer));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> BenutzerAktivieren(int id)
    {
        await _admin.BenutzerAktivierenAsync(id);
        TempData["Erfolg"] = "Benutzer wurde aktiviert.";
        return RedirectToAction(nameof(Benutzer));
    }

    private async Task<BenutzerFormViewModel> BenutzerFormVmAsync(BenutzerFormViewModel vm)
    {
        var lgs = await _admin.GetLeasinggesellschaftenAsync();
        vm.LeasinggesellschaftenListe = lgs
            .Where(l => l.IstAktiv)
            .Select(l => new SelectListItem(l.Name, l.Id.ToString()));
        return vm;
    }

    // ── Leasinggesellschaften ─────────────────────────────────────────────────

    public async Task<IActionResult> Leasinggesellschaften()
    {
        var liste = await _admin.GetLeasinggesellschaftenAsync();
        return View(liste);
    }

    public IActionResult LeasinggesellschaftNeu() =>
        View("LeasinggesellschaftForm", new LeasinggesellschaftFormViewModel());

    public async Task<IActionResult> LeasinggesellschaftBearbeiten(int id)
    {
        var dto = await _admin.GetLeasinggesellschaftByIdAsync(id);
        if (dto is null) return NotFound();

        return View("LeasinggesellschaftForm", new LeasinggesellschaftFormViewModel
        {
            Id = dto.Id, Name = dto.Name, Kurzbezeichnung = dto.Kurzbezeichnung,
            Strasse = dto.Strasse, PLZ = dto.PLZ, Ort = dto.Ort, Land = dto.Land,
            Telefon = dto.Telefon, EMail = dto.EMail,
            Ansprechpartner = dto.Ansprechpartner,
            ObligoLimit = dto.ObligoLimit, IstAktiv = dto.IstAktiv
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> LeasinggesellschaftSpeichern(LeasinggesellschaftFormViewModel vm)
    {
        if (!ModelState.IsValid) return View("LeasinggesellschaftForm", vm);

        var dto = new LeasinggesellschaftDetailDto(
            vm.Id, vm.Name, vm.Kurzbezeichnung, vm.Strasse, vm.PLZ, vm.Ort,
            vm.Land, vm.Telefon, vm.EMail, vm.Ansprechpartner, vm.ObligoLimit, vm.IstAktiv);

        if (vm.Id == 0)
        {
            await _admin.LeasinggesellschaftErstellenAsync(dto);
            TempData["Erfolg"] = $"Leasinggesellschaft '{vm.Name}' wurde erstellt.";
        }
        else
        {
            await _admin.LeasinggesellschaftBearbeitenAsync(dto);
            TempData["Erfolg"] = $"Leasinggesellschaft '{vm.Name}' wurde gespeichert.";
        }
        return RedirectToAction(nameof(Leasinggesellschaften));
    }

    // ── Stammdaten (Ablehnungsgründe + Gerätetypen) ───────────────────────────

    public async Task<IActionResult> Stammdaten()
    {
        ViewBag.Ablehnungsgruende = await _admin.GetAblehnungsgruendeAsync();
        ViewBag.Geraetetypen = await _admin.GetGeraetetypenAsync();
        return View();
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AblehnungsgrundSpeichern(AblehnungsgrundFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Fehler"] = "Ungültige Eingabe.";
            return RedirectToAction(nameof(Stammdaten));
        }
        try
        {
            await _admin.AblehnungsgrundSpeichernAsync(new AblehnungsgrundDto(
                vm.Id, vm.Code, vm.Bezeichnung, vm.IstAktiv));
            TempData["Erfolg"] = "Ablehnungsgrund gespeichert.";
        }
        catch (Exception ex)
        {
            TempData["Fehler"] = ex.Message;
        }
        return RedirectToAction(nameof(Stammdaten));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AblehnungsgrundLoeschen(int id)
    {
        try
        {
            await _admin.AblehnungsgrundLoeschenAsync(id);
            TempData["Erfolg"] = "Ablehnungsgrund gelöscht.";
        }
        catch (Exception ex)
        {
            TempData["Fehler"] = ex.Message;
        }
        return RedirectToAction(nameof(Stammdaten));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GeraetetypSpeichern(GeraetetypFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Fehler"] = "Ungültige Eingabe.";
            return RedirectToAction(nameof(Stammdaten));
        }
        try
        {
            await _admin.GeraetetypSpeichernAsync(new GeraetetypDto(
                vm.Id, vm.Bezeichnung, vm.Beschreibung, vm.IstAktiv, vm.ElterntypId, null));
            TempData["Erfolg"] = "Gerätetyp gespeichert.";
        }
        catch (Exception ex)
        {
            TempData["Fehler"] = ex.Message;
        }
        return RedirectToAction(nameof(Stammdaten));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> GeraetetypLoeschen(int id)
    {
        try
        {
            await _admin.GeraetetypLoeschenAsync(id);
            TempData["Erfolg"] = "Gerätetyp gelöscht.";
        }
        catch (Exception ex)
        {
            TempData["Fehler"] = ex.Message;
        }
        return RedirectToAction(nameof(Stammdaten));
    }

    // ── Demo-Daten laden ──────────────────────────────────────────────────────

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DemoDatenLaden()
    {
        try
        {
            // 1. Remove dependent tables that have Restrict or ClientSetNull delete behavior
            //    (must go before Leasingantraege to avoid FK violations)
            var vertraege = await _db.Vertraege.ToListAsync();
            _db.Vertraege.RemoveRange(vertraege);

            var dokAustausche = await _db.DokumentAustausche.ToListAsync();
            _db.DokumentAustausche.RemoveRange(dokAustausche);

            await _db.SaveChangesAsync();

            // 2. Remove all Leasingantraege — cascade deletes Leasingobjekte,
            //    InternePruefungen, PruefungsPflichten, Kommentare, Anhaenge,
            //    Ereignisse and Obligos automatically.
            var antraege = await _db.Leasingantraege.ToListAsync();
            _db.Leasingantraege.RemoveRange(antraege);
            await _db.SaveChangesAsync();

            // 3. Seed 100 demo applications + Verträge
            await DataSeeder.SeedDemoAntraegeAsync(_db);

            var anzahl = await _db.Leasingantraege.CountAsync();
            TempData["Erfolg"] = $"Demo-Daten erfolgreich geladen: {anzahl} Anträge erstellt.";
        }
        catch (Exception ex)
        {
            TempData["Fehler"] = $"Fehler beim Laden der Demo-Daten: {ex.Message}";
        }

        return RedirectToAction(nameof(Index));
    }
}
