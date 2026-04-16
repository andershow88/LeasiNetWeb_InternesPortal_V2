using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Entities;
using LeasiNetWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Application.Services;

public class LeasingantragService : ILeasingantragService
{
    private readonly IApplicationDbContext _db;
    private readonly IEreignisService _ereignis;

    public LeasingantragService(IApplicationDbContext db, IEreignisService ereignis)
    {
        _db = db;
        _ereignis = ereignis;
    }

    public async Task<IEnumerable<AntragListeDto>> GetAlleAntraege(int? benutzerId = null, AntragStatus? status = null)
    {
        var query = _db.Leasingantraege
            .Include(a => a.EingereichtVon)
            .Include(a => a.Leasinggesellschaft)
            .Include(a => a.SachbearbeiterMB)
            .Where(a => !a.Archiviert)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        return await query
            .OrderByDescending(a => a.GeaendertAm)
            .Select(a => ToListeDto(a))
            .ToListAsync();
    }

    public async Task<AntragDetailDto?> GetAntragDetail(int id)
    {
        var antrag = await _db.Leasingantraege
            .Include(a => a.EingereichtVon)
            .Include(a => a.Leasinggesellschaft)
            .Include(a => a.SachbearbeiterMB)
            .Include(a => a.SachbearbeiterLG)
            .Include(a => a.Ablehnungsgrund)
            .Include(a => a.Objekte).ThenInclude(o => o.Geraetetyp)
            .Include(a => a.Kommentare).ThenInclude(k => k.Autor)
            .Include(a => a.Kommentare).ThenInclude(k => k.Anhaenge)
            .Include(a => a.Anhaenge).ThenInclude(an => an.HochgeladenVon)
            .Include(a => a.Ereignisse).ThenInclude(e => e.AusgeloestVon)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (antrag is null) return null;

        return new AntragDetailDto(
            antrag.Id,
            antrag.AntragNummer,
            antrag.AntragTyp,
            antrag.Status,
            antrag.Obligo,
            antrag.Abrechnungsart,
            antrag.EingereichtVon.Anzeigename,
            antrag.Leasinggesellschaft?.Name,
            antrag.LeasinggesellschaftId,
            antrag.SachbearbeiterMB?.Anzeigename,
            antrag.SachbearbeiterMBId,
            antrag.SachbearbeiterLG?.Anzeigename,
            antrag.SachbearbeiterLGId,
            antrag.Ablehnungsgrund?.Bezeichnung,
            antrag.AblehnungsKommentar,
            antrag.AbgelehntAm,
            antrag.ZweiteVoteErforderlich,
            antrag.Archiviert,
            antrag.ErstelltAm,
            antrag.GeaendertAm,
            antrag.KiErstellt,
            antrag.Objekte.Select(o => new LeasingobjektDto(o.Id, o.Bezeichnung, o.IstNeu,
                o.Listenpreis, o.Rabatt, o.FinanzierungsBasis, o.NAK, o.Hersteller, o.Lieferant,
                o.Geraetetyp?.Bezeichnung)),
            antrag.Kommentare.OrderByDescending(k => k.ErstelltAm).Select(k => new KommentarDto(
                k.Id, k.Autor.Anzeigename, k.Text, k.IstIntern, k.ErstelltAm,
                k.Anhaenge.Select(a => new AnhangDto(a.Id, a.Dateiname, a.Typ,
                    a.DateigroesseBytes, a.HochgeladenVon.Anzeigename, a.ErstelltAm)))),
            antrag.Anhaenge.Where(a => a.KommentarId == null)
                .Select(a => new AnhangDto(a.Id, a.Dateiname, a.Typ,
                    a.DateigroesseBytes, a.HochgeladenVon.Anzeigename, a.ErstelltAm)),
            antrag.Ereignisse.OrderByDescending(e => e.ErstelltAm)
                .Select(e => new EreignisDto(e.Id, e.Typ, e.AusgeloestVon.Anzeigename,
                    e.Beschreibung, e.ErstelltAm))
        );
    }

    public async Task<int> ErstelleAntrag(AntragErstellenDto dto, int benutzerId)
    {
        var antrag = new Leasingantrag
        {
            AntragNummer = await GeneriereAntragNummer(),
            AntragTyp = dto.AntragTyp,
            Status = AntragStatus.Entwurf,
            LeasinggesellschaftId = dto.LeasinggesellschaftId,
            Obligo = dto.Obligo,
            Abrechnungsart = dto.Abrechnungsart,
            KiErstellt = dto.KiErstellt,
            EingereichtVonId = benutzerId,
            ErstelltAm = DateTime.UtcNow,
            GeaendertAm = DateTime.UtcNow,
            ErstelltVonId = benutzerId,
            GeaendertVonId = benutzerId
        };

        _db.Leasingantraege.Add(antrag);
        await _db.SaveChangesAsync();

        await _ereignis.EreignisAufzeichnen(antrag.Id, EreignisTyp.AntragEingereicht, benutzerId);

        return antrag.Id;
    }

    public async Task AktualisiereAntrag(int id, AntragAktualisierenDto dto, int benutzerId)
    {
        var antrag = await _db.Leasingantraege.FindAsync(id)
            ?? throw new KeyNotFoundException($"Antrag {id} nicht gefunden.");

        if (dto.LeasinggesellschaftId.HasValue) antrag.LeasinggesellschaftId = dto.LeasinggesellschaftId;
        if (dto.SachbearbeiterMBId.HasValue) antrag.SachbearbeiterMBId = dto.SachbearbeiterMBId;
        if (dto.SachbearbeiterLGId.HasValue) antrag.SachbearbeiterLGId = dto.SachbearbeiterLGId;
        if (dto.Obligo.HasValue) antrag.Obligo = dto.Obligo.Value;
        if (dto.Abrechnungsart is not null) antrag.Abrechnungsart = dto.Abrechnungsart;

        antrag.GeaendertAm = DateTime.UtcNow;
        antrag.GeaendertVonId = benutzerId;

        await _db.SaveChangesAsync();
    }

    public async Task<int> ErstelleKiAntrag(AntragErstellenDto dto, int benutzerId)
    {
        var antrag = new Leasingantrag
        {
            AntragNummer = await GeneriereAntragNummer(),
            AntragTyp = dto.AntragTyp,
            Status = AntragStatus.KiEingereicht,
            LeasinggesellschaftId = dto.LeasinggesellschaftId,
            Obligo = dto.Obligo,
            Abrechnungsart = dto.Abrechnungsart,
            KiErstellt = true,
            EingereichtVonId = benutzerId,
            ErstelltAm = DateTime.UtcNow,
            GeaendertAm = DateTime.UtcNow,
            ErstelltVonId = benutzerId,
            GeaendertVonId = benutzerId
        };

        _db.Leasingantraege.Add(antrag);
        await _db.SaveChangesAsync();

        await _ereignis.EreignisAufzeichnen(antrag.Id, EreignisTyp.AntragEingereicht, benutzerId, "Via KI-Analyse eingereicht");

        return antrag.Id;
    }

    public async Task<bool> StatusWechsel(int antragId, AntragStatus neuerStatus, int benutzerId, string? kommentar = null)
    {
        var antrag = await _db.Leasingantraege.FindAsync(antragId);
        if (antrag is null) return false;

        if (!IstStatusUebergangErlaubt(antrag.Status, neuerStatus))
            return false;

        antrag.Status = neuerStatus;
        antrag.GeaendertAm = DateTime.UtcNow;
        antrag.GeaendertVonId = benutzerId;
        await _db.SaveChangesAsync();

        await _ereignis.EreignisAufzeichnen(antragId, EreignisTyp.AntragStatusGeaendert, benutzerId,
            $"Status geändert zu: {neuerStatus}");

        return true;
    }

    public async Task<bool> Genehmigen(int antragId, int genehmigerMBId)
    {
        var antrag = await _db.Leasingantraege.FindAsync(antragId);
        if (antrag is null) return false;

        antrag.Status = AntragStatus.Genehmigt;
        antrag.GenehmigerMBId = genehmigerMBId;
        antrag.GeaendertAm = DateTime.UtcNow;
        antrag.GeaendertVonId = genehmigerMBId;
        await _db.SaveChangesAsync();

        await _ereignis.EreignisAufzeichnen(antragId, EreignisTyp.AntragGenehmigt, genehmigerMBId);
        return true;
    }

    public async Task<bool> Ablehnen(int antragId, int genehmigerMBId, int ablehnungsgrundId, string? kommentar)
    {
        var antrag = await _db.Leasingantraege.FindAsync(antragId);
        if (antrag is null) return false;

        antrag.Status = AntragStatus.Abgelehnt;
        antrag.AblehnungsgrundId = ablehnungsgrundId;
        antrag.AblehnungsKommentar = kommentar;
        antrag.AbgelehntAm = DateTime.UtcNow;
        antrag.GeaendertAm = DateTime.UtcNow;
        antrag.GeaendertVonId = genehmigerMBId;
        await _db.SaveChangesAsync();

        await _ereignis.EreignisAufzeichnen(antragId, EreignisTyp.AntragAbgelehnt, genehmigerMBId, kommentar);
        return true;
    }

    public async Task<bool> ZweiteVoteAnfordern(int antragId, int genehmigerMBId)
    {
        var antrag = await _db.Leasingantraege.FindAsync(antragId);
        if (antrag is null) return false;

        antrag.ZweiteVoteErforderlich = true;
        antrag.Status = AntragStatus.ZweiteVoteErforderlich;
        antrag.GeaendertAm = DateTime.UtcNow;
        antrag.GeaendertVonId = genehmigerMBId;
        await _db.SaveChangesAsync();

        await _ereignis.EreignisAufzeichnen(antragId, EreignisTyp.ZweiteVoteAngefordert, genehmigerMBId);
        return true;
    }

    public async Task Archivieren(int antragId)
    {
        var antrag = await _db.Leasingantraege.FindAsync(antragId)
            ?? throw new KeyNotFoundException($"Antrag {antragId} nicht gefunden.");

        antrag.Archiviert = true;
        antrag.ArchiviertAm = DateTime.UtcNow;
        antrag.Status = AntragStatus.Archiviert;
        antrag.GeaendertAm = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public Task<IEnumerable<AntragListeDto>> GetMeineZuPruefendenAntraege(int benutzerId)
        => GetAlleAntraege(benutzerId, AntragStatus.InPruefung);

    public Task<IEnumerable<AntragListeDto>> GetMeineZuGenehmigendenAntraege(int benutzerId)
        => GetAlleAntraege(benutzerId, AntragStatus.BeiMitarbeiter);

    public Task<IEnumerable<AntragListeDto>> GetMeineZweiteVotenAntraege(int benutzerId)
        => GetAlleAntraege(benutzerId, AntragStatus.ZweiteVoteErforderlich);

    // ────────────────────────────────────────────────────────────
    // Private helpers
    // ────────────────────────────────────────────────────────────

    private static AntragListeDto ToListeDto(Leasingantrag a) => new(
        a.Id, a.AntragNummer, a.AntragTyp, a.Status,
        a.EingereichtVon.Anzeigename,
        a.Leasinggesellschaft?.Name,
        a.SachbearbeiterMB?.Anzeigename,
        a.Obligo, a.ErstelltAm, a.GeaendertAm, a.ZweiteVoteErforderlich,
        a.KiErstellt);

    private async Task<string> GeneriereAntragNummer()
    {
        var count = await _db.Leasingantraege.CountAsync();
        return $"LNW-{DateTime.UtcNow:yyyy}-{count + 1:D5}";
    }

    /// <summary>
    /// Defines allowed state machine transitions.
    /// Replaces the scattered Intrexx workflow/button-based transitions.
    /// </summary>
    private static bool IstStatusUebergangErlaubt(AntragStatus von, AntragStatus nach) => (von, nach) switch
    {
        (AntragStatus.Entwurf, AntragStatus.Eingereicht) => true,
        (AntragStatus.Eingereicht, AntragStatus.InPruefung) => true,
        (AntragStatus.Eingereicht, AntragStatus.BeiMitarbeiter) => true,
        (AntragStatus.InPruefung, AntragStatus.BeiMitarbeiter) => true,
        (AntragStatus.BeiMitarbeiter, AntragStatus.BeiLeasinggesellschaft) => true,
        (AntragStatus.BeiMitarbeiter, AntragStatus.ZweiteVoteErforderlich) => true,
        (AntragStatus.BeiMitarbeiter, AntragStatus.InterneKontrolleErforderlich) => true,
        (AntragStatus.BeiMitarbeiter, AntragStatus.Genehmigt) => true,
        (AntragStatus.BeiMitarbeiter, AntragStatus.Abgelehnt) => true,
        (AntragStatus.BeiLeasinggesellschaft, AntragStatus.BeiMitarbeiter) => true,
        (AntragStatus.ZweiteVoteErforderlich, AntragStatus.Genehmigt) => true,
        (AntragStatus.ZweiteVoteErforderlich, AntragStatus.Abgelehnt) => true,
        (AntragStatus.InterneKontrolleErforderlich, AntragStatus.BeiMitarbeiter) => true,
        (AntragStatus.KiEingereicht, AntragStatus.InPruefung) => true,
        (AntragStatus.KiEingereicht, AntragStatus.Abgelehnt) => true,
        (AntragStatus.Genehmigt, AntragStatus.Archiviert) => true,
        (AntragStatus.Abgelehnt, AntragStatus.Archiviert) => true,
        _ => false
    };
}
