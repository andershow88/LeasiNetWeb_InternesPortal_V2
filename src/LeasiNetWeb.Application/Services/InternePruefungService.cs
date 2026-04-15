using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Entities;
using LeasiNetWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Application.Services;

public class InternePruefungService : IInternePruefungService
{
    private readonly IApplicationDbContext _db;
    private readonly IEreignisService _ereignis;

    public InternePruefungService(IApplicationDbContext db, IEreignisService ereignis)
    {
        _db = db;
        _ereignis = ereignis;
    }

    public async Task<int> StartenAsync(int antragId, int hauptPrueferMBId, List<PruefungsSchrittInput> schritte)
    {
        var antrag = await _db.Leasingantraege
            .Include(a => a.Leasinggesellschaft)
            .FirstOrDefaultAsync(a => a.Id == antragId)
            ?? throw new KeyNotFoundException($"Antrag {antragId} nicht gefunden.");

        var existing = await _db.InternePruefungen
            .FirstOrDefaultAsync(p => p.LeasingantragId == antragId && !p.Abgeschlossen);
        if (existing is not null)
            return existing.Id;

        var pruefungNummer = await GenerierePruefungNummerAsync(antrag.Leasinggesellschaft?.Kurzbezeichnung);

        var pruefung = new InternePruefung
        {
            LeasingantragId = antragId,
            PrueferMBId     = hauptPrueferMBId,
            PruefungNummer  = pruefungNummer,
            Abgeschlossen   = false,
            ErstelltAm      = DateTime.UtcNow,
            GeaendertAm     = DateTime.UtcNow,
            ErstelltVonId   = hauptPrueferMBId,
            GeaendertVonId  = hauptPrueferMBId
        };
        _db.InternePruefungen.Add(pruefung);
        await _db.SaveChangesAsync();

        foreach (var p in StandardPflichten(pruefung.Id))
            _db.PruefungsPflichten.Add(p);

        var eingabeSchritte = schritte?.Count > 0
            ? schritte
            : new List<PruefungsSchrittInput> { new(hauptPrueferMBId, "1. Prüfer") };

        for (int i = 0; i < eingabeSchritte.Count; i++)
        {
            _db.PruefungsSchritte.Add(new PruefungsSchritt
            {
                InternePruefungId = pruefung.Id,
                Sortierung        = i + 1,
                Bezeichnung       = eingabeSchritte[i].Bezeichnung,
                PrueferMBId       = eingabeSchritte[i].PrueferMBId,
                Abgeschlossen     = false,
                ErstelltAm        = DateTime.UtcNow,
                GeaendertAm       = DateTime.UtcNow,
                ErstelltVonId     = hauptPrueferMBId,
                GeaendertVonId    = hauptPrueferMBId
            });
        }

        antrag.Status         = AntragStatus.InterneKontrolleErforderlich;
        antrag.GeaendertAm    = DateTime.UtcNow;
        antrag.GeaendertVonId = hauptPrueferMBId;

        await _db.SaveChangesAsync();
        await _ereignis.EreignisAufzeichnen(antragId, EreignisTyp.InternePruefungGestartet,
            hauptPrueferMBId, $"Interne Kontrolle gestartet (Prüfnummer: {pruefungNummer})");

        return pruefung.Id;
    }

    public async Task<PruefungWizardDatenDto?> GetWizardDatenAsync(int antragId)
    {
        var antrag = await _db.Leasingantraege
            .Include(a => a.Leasinggesellschaft)
            .FirstOrDefaultAsync(a => a.Id == antragId);
        if (antrag is null) return null;

        var pruefer = await _db.Benutzer
            .Where(b => b.IstAktiv && (b.Rolle == BenutzerRolle.InternerPruefer || b.Rolle == BenutzerRolle.Administrator))
            .OrderBy(b => b.Nachname)
            .Select(b => new PrueferOptionDto(b.Id, b.Vorname + " " + b.Nachname, b.Rolle.ToString()))
            .ToListAsync();

        return new PruefungWizardDatenDto(antrag.Id, antrag.AntragNummer,
            antrag.Leasinggesellschaft?.Name, antrag.Obligo, pruefer);
    }

    public async Task<InternePruefungDto?> GetByAntragIdAsync(int antragId)
    {
        var p = await _db.InternePruefungen
            .Include(x => x.PrueferMB)
            .Include(x => x.Leasingantrag).ThenInclude(a => a.Leasinggesellschaft)
            .Include(x => x.Pflichten)
            .Include(x => x.Schritte).ThenInclude(s => s.PrueferMB)
            .Include(x => x.Anhaenge).ThenInclude(a => a.HochgeladenVon)
            .Where(x => x.LeasingantragId == antragId)
            .OrderByDescending(x => x.ErstelltAm)
            .FirstOrDefaultAsync();
        return p is null ? null : ToDto(p);
    }

    public async Task<InternePruefungDto?> GetByIdAsync(int id)
    {
        var p = await _db.InternePruefungen
            .Include(x => x.PrueferMB)
            .Include(x => x.Leasingantrag).ThenInclude(a => a.Leasinggesellschaft)
            .Include(x => x.Pflichten)
            .Include(x => x.Schritte).ThenInclude(s => s.PrueferMB)
            .Include(x => x.Anhaenge).ThenInclude(a => a.HochgeladenVon)
            .FirstOrDefaultAsync(x => x.Id == id);
        return p is null ? null : ToDto(p);
    }

    public async Task<IEnumerable<InternePruefungListeDto>> GetMeinePruefungenAsync(int prueferMBId)
    {
        return await _db.InternePruefungen
            .Include(p => p.PrueferMB)
            .Include(p => p.Leasingantrag)
            .Include(p => p.Pflichten)
            .Include(p => p.Schritte)
            .Where(p => (p.PrueferMBId == prueferMBId ||
                         p.Schritte.Any(s => s.PrueferMBId == prueferMBId)) && !p.Abgeschlossen)
            .OrderByDescending(p => p.ErstelltAm)
            .Select(p => new InternePruefungListeDto(
                p.Id, p.LeasingantragId, p.Leasingantrag.AntragNummer,
                p.PrueferMB.Vorname + " " + p.PrueferMB.Nachname, p.PruefungNummer,
                p.Abgeschlossen, p.AbgeschlossenAm,
                p.Pflichten.Count, p.Pflichten.Count(f => f.Erfuellt),
                p.Schritte.Count, p.Schritte.Count(s => s.Abgeschlossen)))
            .ToListAsync();
    }

    public async Task<IEnumerable<InternePruefungListeDto>> GetAllePruefungenAsync()
    {
        return await _db.InternePruefungen
            .Include(p => p.PrueferMB)
            .Include(p => p.Leasingantrag)
            .Include(p => p.Pflichten)
            .Include(p => p.Schritte)
            .OrderByDescending(p => p.ErstelltAm)
            .Select(p => new InternePruefungListeDto(
                p.Id, p.LeasingantragId, p.Leasingantrag.AntragNummer,
                p.PrueferMB.Vorname + " " + p.PrueferMB.Nachname, p.PruefungNummer,
                p.Abgeschlossen, p.AbgeschlossenAm,
                p.Pflichten.Count, p.Pflichten.Count(f => f.Erfuellt),
                p.Schritte.Count, p.Schritte.Count(s => s.Abgeschlossen)))
            .ToListAsync();
    }

    public async Task PflichtErfuellenAsync(int pflichtId, string? bemerkungen, int benutzerId)
    {
        var pflicht = await _db.PruefungsPflichten.FindAsync(pflichtId)
            ?? throw new KeyNotFoundException();
        pflicht.Erfuellt       = true;
        pflicht.ErfuelltAm     = DateTime.UtcNow;
        pflicht.Bemerkungen    = bemerkungen;
        pflicht.GeaendertAm    = DateTime.UtcNow;
        pflicht.GeaendertVonId = benutzerId;
        await _db.SaveChangesAsync();
    }

    public async Task PflichtRueckgaengigAsync(int pflichtId)
    {
        var pflicht = await _db.PruefungsPflichten.FindAsync(pflichtId)
            ?? throw new KeyNotFoundException();
        pflicht.Erfuellt    = false;
        pflicht.ErfuelltAm  = null;
        pflicht.Bemerkungen = null;
        pflicht.GeaendertAm = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task SchrittAbschliessenAsync(int schrittId, int benutzerId, string? ergebnis)
    {
        var schritt = await _db.PruefungsSchritte
            .Include(s => s.InternePruefung)
            .FirstOrDefaultAsync(s => s.Id == schrittId)
            ?? throw new KeyNotFoundException();

        schritt.Abgeschlossen   = true;
        schritt.AbgeschlossenAm = DateTime.UtcNow;
        schritt.Ergebnis        = ergebnis;
        schritt.GeaendertAm     = DateTime.UtcNow;
        schritt.GeaendertVonId  = benutzerId;

        await _db.SaveChangesAsync();
        await _ereignis.EreignisAufzeichnen(
            schritt.InternePruefung.LeasingantragId,
            EreignisTyp.AntragStatusGeaendert, benutzerId,
            $"Prüfschritt {schritt.Sortierung} ({schritt.Bezeichnung}) abgeschlossen.");
    }

    public async Task AbschliessenAsync(int id, int benutzerId, string? ergebnis)
    {
        var pruefung = await _db.InternePruefungen
            .Include(p => p.Leasingantrag)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException();

        pruefung.Abgeschlossen   = true;
        pruefung.AbgeschlossenAm = DateTime.UtcNow;
        pruefung.Ergebnis        = ergebnis;
        pruefung.GeaendertAm     = DateTime.UtcNow;
        pruefung.GeaendertVonId  = benutzerId;

        pruefung.Leasingantrag.Status         = AntragStatus.BeiMitarbeiter;
        pruefung.Leasingantrag.GeaendertAm    = DateTime.UtcNow;
        pruefung.Leasingantrag.GeaendertVonId = benutzerId;

        await _db.SaveChangesAsync();
        await _ereignis.EreignisAufzeichnen(pruefung.LeasingantragId,
            EreignisTyp.InternePruefungAbgeschlossen, benutzerId,
            $"Interne Kontrolle abgeschlossen (Nr: {pruefung.PruefungNummer ?? "–"}). Ergebnis: {ergebnis ?? "–"}");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private async Task<string> GenerierePruefungNummerAsync(string? lgKuerzel)
    {
        var kuerzel = !string.IsNullOrWhiteSpace(lgKuerzel) ? lgKuerzel : "LNW";
        var jahr    = DateTime.UtcNow.Year;
        var prefix  = $"{kuerzel}/{jahr}/";
        var anzahl  = await _db.InternePruefungen
            .CountAsync(p => p.PruefungNummer != null && p.PruefungNummer.StartsWith(prefix));
        return $"{prefix}{(anzahl + 1):D3}";
    }

    private static InternePruefungDto ToDto(InternePruefung p) => new(
        p.Id, p.LeasingantragId, p.Leasingantrag.AntragNummer,
        p.Leasingantrag.Leasinggesellschaft?.Name, p.Leasingantrag.Obligo,
        p.PrueferMB.Anzeigename, p.PrueferMBId, p.PruefungNummer,
        p.Abgeschlossen, p.AbgeschlossenAm, p.Ergebnis,
        p.Pflichten.OrderBy(f => f.Sortierung).Select(f => new PruefungsPflichtDto(
            f.Id, f.InternePruefungId, f.Bezeichnung, f.Beschreibung,
            f.Erfuellt, f.ErfuelltAm, f.Bemerkungen, f.Sortierung)),
        p.Schritte.OrderBy(s => s.Sortierung).Select(s => new PruefungsSchrittDto(
            s.Id, s.InternePruefungId, s.Sortierung, s.Bezeichnung,
            s.PrueferMB.Anzeigename, s.PrueferMBId,
            s.Abgeschlossen, s.AbgeschlossenAm, s.Ergebnis)),
        p.Anhaenge.Select(a => new AnhangDto(a.Id, a.Dateiname, a.Typ,
            a.DateigroesseBytes, a.HochgeladenVon.Anzeigename, a.ErstelltAm))
    );

    private static List<PruefungsPflicht> StandardPflichten(int pruefungId) =>
    [
        new() { InternePruefungId=pruefungId, Sortierung=10, Bezeichnung="KYC-Prüfung",
            Beschreibung="Know-Your-Customer-Prüfung durchgeführt und dokumentiert.",
            ErstelltAm=DateTime.UtcNow, GeaendertAm=DateTime.UtcNow },
        new() { InternePruefungId=pruefungId, Sortierung=20, Bezeichnung="Bonitätsprüfung",
            Beschreibung="Bonitätsprüfung des Antragstellers abgeschlossen.",
            ErstelltAm=DateTime.UtcNow, GeaendertAm=DateTime.UtcNow },
        new() { InternePruefungId=pruefungId, Sortierung=30, Bezeichnung="Geldwäsche-Check",
            Beschreibung="Geldwäsche-Prüfung gemäß GwG durchgeführt.",
            ErstelltAm=DateTime.UtcNow, GeaendertAm=DateTime.UtcNow },
        new() { InternePruefungId=pruefungId, Sortierung=40, Bezeichnung="Sanktionslisten-Abgleich",
            Beschreibung="Abgleich mit EU- und internationalen Sanktionslisten erfolgt.",
            ErstelltAm=DateTime.UtcNow, GeaendertAm=DateTime.UtcNow },
        new() { InternePruefungId=pruefungId, Sortierung=50, Bezeichnung="Unterlagen vollständig",
            Beschreibung="Alle erforderlichen Unterlagen liegen vor und wurden geprüft.",
            ErstelltAm=DateTime.UtcNow, GeaendertAm=DateTime.UtcNow },
        new() { InternePruefungId=pruefungId, Sortierung=60, Bezeichnung="Obligo-Limit eingehalten",
            Beschreibung="Das Obligo-Limit der Leasinggesellschaft wird durch den Antrag nicht überschritten.",
            ErstelltAm=DateTime.UtcNow, GeaendertAm=DateTime.UtcNow },
    ];
}
