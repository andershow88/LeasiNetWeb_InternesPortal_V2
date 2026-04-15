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

    public async Task<int> StartenAsync(int antragId, int prueferMBId)
    {
        var antrag = await _db.Leasingantraege.FindAsync(antragId)
            ?? throw new KeyNotFoundException($"Antrag {antragId} nicht gefunden.");

        // Prevent duplicate
        var existing = await _db.InternePruefungen
            .FirstOrDefaultAsync(p => p.LeasingantragId == antragId && !p.Abgeschlossen);
        if (existing is not null)
            return existing.Id;

        var pruefung = new InternePruefung
        {
            LeasingantragId = antragId,
            PrueferMBId = prueferMBId,
            Abgeschlossen = false,
            ErstelltAm = DateTime.UtcNow,
            GeaendertAm = DateTime.UtcNow,
            ErstelltVonId = prueferMBId,
            GeaendertVonId = prueferMBId
        };
        _db.InternePruefungen.Add(pruefung);
        await _db.SaveChangesAsync();

        // Add standard compliance checklist
        var pflichten = StandardPflichten(pruefung.Id);
        foreach (var p in pflichten)
            _db.PruefungsPflichten.Add(p);

        // Transition antrag status
        antrag.Status = AntragStatus.InterneKontrolleErforderlich;
        antrag.GeaendertAm = DateTime.UtcNow;
        antrag.GeaendertVonId = prueferMBId;

        await _db.SaveChangesAsync();
        await _ereignis.EreignisAufzeichnen(antragId, EreignisTyp.AntragStatusGeaendert, prueferMBId,
            "Interne Kontrolle gestartet");

        return pruefung.Id;
    }

    public async Task<InternePruefungDto?> GetByAntragIdAsync(int antragId)
    {
        var pruefung = await _db.InternePruefungen
            .Include(p => p.PrueferMB)
            .Include(p => p.Leasingantrag)
            .Include(p => p.Pflichten)
            .Include(p => p.Anhaenge).ThenInclude(a => a.HochgeladenVon)
            .Where(p => p.LeasingantragId == antragId)
            .OrderByDescending(p => p.ErstelltAm)
            .FirstOrDefaultAsync();

        return pruefung is null ? null : ToDto(pruefung);
    }

    public async Task<InternePruefungDto?> GetByIdAsync(int id)
    {
        var pruefung = await _db.InternePruefungen
            .Include(p => p.PrueferMB)
            .Include(p => p.Leasingantrag)
            .Include(p => p.Pflichten)
            .Include(p => p.Anhaenge).ThenInclude(a => a.HochgeladenVon)
            .FirstOrDefaultAsync(p => p.Id == id);

        return pruefung is null ? null : ToDto(pruefung);
    }

    public async Task<IEnumerable<InternePruefungListeDto>> GetMeinePruefungenAsync(int prueferMBId)
    {
        return await _db.InternePruefungen
            .Include(p => p.PrueferMB)
            .Include(p => p.Leasingantrag)
            .Include(p => p.Pflichten)
            .Where(p => p.PrueferMBId == prueferMBId && !p.Abgeschlossen)
            .OrderByDescending(p => p.ErstelltAm)
            .Select(p => new InternePruefungListeDto(
                p.Id,
                p.LeasingantragId,
                p.Leasingantrag.AntragNummer,
                p.PrueferMB.Anzeigename,
                p.Abgeschlossen,
                p.AbgeschlossenAm,
                p.Pflichten.Count,
                p.Pflichten.Count(f => f.Erfuellt)))
            .ToListAsync();
    }

    public async Task<IEnumerable<InternePruefungListeDto>> GetAllePruefungenAsync()
    {
        return await _db.InternePruefungen
            .Include(p => p.PrueferMB)
            .Include(p => p.Leasingantrag)
            .Include(p => p.Pflichten)
            .OrderByDescending(p => p.ErstelltAm)
            .Select(p => new InternePruefungListeDto(
                p.Id,
                p.LeasingantragId,
                p.Leasingantrag.AntragNummer,
                p.PrueferMB.Anzeigename,
                p.Abgeschlossen,
                p.AbgeschlossenAm,
                p.Pflichten.Count,
                p.Pflichten.Count(f => f.Erfuellt)))
            .ToListAsync();
    }

    public async Task PflichtErfuellenAsync(int pflichtId, string? bemerkungen)
    {
        var pflicht = await _db.PruefungsPflichten.FindAsync(pflichtId)
            ?? throw new KeyNotFoundException($"PruefungsPflicht {pflichtId} nicht gefunden.");

        pflicht.Erfuellt = true;
        pflicht.ErfuelltAm = DateTime.UtcNow;
        pflicht.Bemerkungen = bemerkungen;
        pflicht.GeaendertAm = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task PflichtRueckgaengigAsync(int pflichtId)
    {
        var pflicht = await _db.PruefungsPflichten.FindAsync(pflichtId)
            ?? throw new KeyNotFoundException($"PruefungsPflicht {pflichtId} nicht gefunden.");

        pflicht.Erfuellt = false;
        pflicht.ErfuelltAm = null;
        pflicht.Bemerkungen = null;
        pflicht.GeaendertAm = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task AbschliessenAsync(int id, int benutzerId, string? ergebnis)
    {
        var pruefung = await _db.InternePruefungen
            .Include(p => p.Leasingantrag)
            .FirstOrDefaultAsync(p => p.Id == id)
            ?? throw new KeyNotFoundException($"InternePruefung {id} nicht gefunden.");

        pruefung.Abgeschlossen = true;
        pruefung.AbgeschlossenAm = DateTime.UtcNow;
        pruefung.Ergebnis = ergebnis;
        pruefung.GeaendertAm = DateTime.UtcNow;
        pruefung.GeaendertVonId = benutzerId;

        // Transition antrag back to BeiMitarbeiter
        pruefung.Leasingantrag.Status = AntragStatus.BeiMitarbeiter;
        pruefung.Leasingantrag.GeaendertAm = DateTime.UtcNow;
        pruefung.Leasingantrag.GeaendertVonId = benutzerId;

        await _db.SaveChangesAsync();
        await _ereignis.EreignisAufzeichnen(pruefung.LeasingantragId, EreignisTyp.AntragStatusGeaendert,
            benutzerId, $"Interne Kontrolle abgeschlossen. Ergebnis: {ergebnis ?? "–"}");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static InternePruefungDto ToDto(InternePruefung p) => new(
        p.Id,
        p.LeasingantragId,
        p.Leasingantrag.AntragNummer,
        p.PrueferMB.Anzeigename,
        p.PrueferMBId,
        p.Abgeschlossen,
        p.AbgeschlossenAm,
        p.Ergebnis,
        p.Pflichten.OrderBy(f => f.Sortierung).Select(f => new PruefungsPflichtDto(
            f.Id, f.InternePruefungId, f.Bezeichnung, f.Beschreibung,
            f.Erfuellt, f.ErfuelltAm, f.Bemerkungen, f.Sortierung)),
        p.Anhaenge.Select(a => new AnhangDto(a.Id, a.Dateiname, a.Typ,
            a.DateigroesseBytes, a.HochgeladenVon.Anzeigename, a.ErstelltAm))
    );

    private static List<PruefungsPflicht> StandardPflichten(int pruefungId) =>
    [
        new() {
            InternePruefungId = pruefungId, Sortierung = 10,
            Bezeichnung = "KYC-Prüfung",
            Beschreibung = "Know-Your-Customer-Prüfung durchgeführt und dokumentiert.",
            Erfuellt = false, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow
        },
        new() {
            InternePruefungId = pruefungId, Sortierung = 20,
            Bezeichnung = "Bonitätsprüfung",
            Beschreibung = "Bonitätsprüfung des Antragstellers abgeschlossen.",
            Erfuellt = false, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow
        },
        new() {
            InternePruefungId = pruefungId, Sortierung = 30,
            Bezeichnung = "Geldwäsche-Check",
            Beschreibung = "Geldwäsche-Prüfung gemäß GwG durchgeführt.",
            Erfuellt = false, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow
        },
        new() {
            InternePruefungId = pruefungId, Sortierung = 40,
            Bezeichnung = "Sanktionslisten-Abgleich",
            Beschreibung = "Abgleich mit EU- und internationalen Sanktionslisten erfolgt.",
            Erfuellt = false, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow
        },
        new() {
            InternePruefungId = pruefungId, Sortierung = 50,
            Bezeichnung = "Unterlagen vollständig",
            Beschreibung = "Alle erforderlichen Unterlagen liegen vor und wurden geprüft.",
            Erfuellt = false, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow
        },
        new() {
            InternePruefungId = pruefungId, Sortierung = 60,
            Bezeichnung = "Obligo-Limit eingehalten",
            Beschreibung = "Das Obligo-Limit der Leasinggesellschaft wird durch den Antrag nicht überschritten.",
            Erfuellt = false, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow
        },
    ];
}
