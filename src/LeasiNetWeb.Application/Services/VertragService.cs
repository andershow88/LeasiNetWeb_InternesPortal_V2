using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Entities;
using LeasiNetWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Application.Services;

public class VertragService : IVertragService
{
    private readonly IApplicationDbContext _db;
    private readonly IEreignisService _ereignis;

    public VertragService(IApplicationDbContext db, IEreignisService ereignis)
    {
        _db = db;
        _ereignis = ereignis;
    }

    public async Task<int> ErstellenAsync(int antragId, int benutzerId)
    {
        var antrag = await _db.Leasingantraege
            .Include(a => a.Leasinggesellschaft)
            .FirstOrDefaultAsync(a => a.Id == antragId)
            ?? throw new KeyNotFoundException($"Antrag {antragId} nicht gefunden.");

        if (antrag.Status != AntragStatus.Genehmigt)
            throw new InvalidOperationException("Vertrag kann nur für genehmigte Anträge erstellt werden.");

        // Prevent duplicate
        var existing = await _db.Vertraege.FirstOrDefaultAsync(v => v.LeasingantragId == antragId);
        if (existing is not null) return existing.Id;

        var vertrag = new Vertrag
        {
            VertragNummer = await GeneriereVertragNummer(),
            Status = VertragStatus.InVorbereitung,
            LeasingantragId = antragId,
            Finanzierungsbetrag = antrag.Obligo,
            ErstelltAm = DateTime.UtcNow,
            GeaendertAm = DateTime.UtcNow,
            ErstelltVonId = benutzerId,
            GeaendertVonId = benutzerId
        };

        _db.Vertraege.Add(vertrag);
        await _db.SaveChangesAsync();

        await _ereignis.EreignisAufzeichnen(antragId, EreignisTyp.AntragStatusGeaendert, benutzerId,
            $"Vertrag {vertrag.VertragNummer} angelegt");

        return vertrag.Id;
    }

    public async Task<IEnumerable<VertragListeDto>> GetAlleVertraege(VertragStatus? status = null)
    {
        var query = _db.Vertraege
            .Include(v => v.Leasingantrag).ThenInclude(a => a.Leasinggesellschaft)
            .Include(v => v.Vertragstyp)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(v => v.Status == status.Value);

        return await query
            .OrderByDescending(v => v.ErstelltAm)
            .Select(v => new VertragListeDto(
                v.Id,
                v.VertragNummer,
                v.Status,
                v.LeasingantragId,
                v.Leasingantrag.AntragNummer,
                v.Leasingantrag.Leasinggesellschaft != null ? v.Leasingantrag.Leasinggesellschaft.Name : null,
                v.Vertragstyp != null ? v.Vertragstyp.Bezeichnung : null,
                v.Finanzierungsbetrag,
                v.Vertragsbeginn,
                v.Vertragsende,
                v.LaufzeitMonate))
            .ToListAsync();
    }

    public async Task<VertragDetailDto?> GetVertragDetail(int id)
    {
        var v = await _db.Vertraege
            .Include(v => v.Leasingantrag).ThenInclude(a => a.EingereichtVon)
            .Include(v => v.Leasingantrag).ThenInclude(a => a.Leasinggesellschaft)
            .Include(v => v.Leasingantrag).ThenInclude(a => a.Objekte).ThenInclude(o => o.Geraetetyp)
            .Include(v => v.Vertragstyp)
            .Include(v => v.Anhaenge).ThenInclude(a => a.HochgeladenVon)
            .FirstOrDefaultAsync(v => v.Id == id);

        return v is null ? null : ToDetailDto(v);
    }

    public async Task<VertragDetailDto?> GetVertragByAntragId(int antragId)
    {
        var v = await _db.Vertraege
            .Include(v => v.Leasingantrag).ThenInclude(a => a.EingereichtVon)
            .Include(v => v.Leasingantrag).ThenInclude(a => a.Leasinggesellschaft)
            .Include(v => v.Leasingantrag).ThenInclude(a => a.Objekte).ThenInclude(o => o.Geraetetyp)
            .Include(v => v.Vertragstyp)
            .Include(v => v.Anhaenge).ThenInclude(a => a.HochgeladenVon)
            .FirstOrDefaultAsync(v => v.LeasingantragId == antragId);

        return v is null ? null : ToDetailDto(v);
    }

    public async Task AktualisiereVertrag(int id, VertragAktualisierenDto dto, int benutzerId)
    {
        var vertrag = await _db.Vertraege.FindAsync(id)
            ?? throw new KeyNotFoundException($"Vertrag {id} nicht gefunden.");

        vertrag.VertragtypId = dto.VertragtypId;
        vertrag.Vertragsbeginn = dto.Vertragsbeginn;
        vertrag.LaufzeitMonate = dto.LaufzeitMonate;
        vertrag.Vertragsende = dto.Vertragsende
            ?? (dto.Vertragsbeginn.HasValue && dto.LaufzeitMonate.HasValue
                ? dto.Vertragsbeginn.Value.AddMonths(dto.LaufzeitMonate.Value)
                : null);
        vertrag.Finanzierungsbetrag = dto.Finanzierungsbetrag;
        vertrag.Restwert = dto.Restwert;
        vertrag.MonatlicheRate = dto.MonatlicheRate;
        vertrag.Zinssatz = dto.Zinssatz;
        vertrag.GeaendertAm = DateTime.UtcNow;
        vertrag.GeaendertVonId = benutzerId;

        await _db.SaveChangesAsync();
    }

    public async Task StatusWechsel(int id, VertragStatus neuerStatus, int benutzerId)
    {
        var vertrag = await _db.Vertraege
            .Include(v => v.Leasingantrag)
            .FirstOrDefaultAsync(v => v.Id == id)
            ?? throw new KeyNotFoundException($"Vertrag {id} nicht gefunden.");

        if (!IstStatusUebergangErlaubt(vertrag.Status, neuerStatus))
            throw new InvalidOperationException(
                $"Statuswechsel von {vertrag.Status} nach {neuerStatus} ist nicht erlaubt.");

        vertrag.Status = neuerStatus;
        vertrag.GeaendertAm = DateTime.UtcNow;
        vertrag.GeaendertVonId = benutzerId;
        await _db.SaveChangesAsync();

        await _ereignis.EreignisAufzeichnen(vertrag.LeasingantragId,
            EreignisTyp.AntragStatusGeaendert, benutzerId,
            $"Vertragsstatus geändert zu: {neuerStatus}");
    }

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static VertragDetailDto ToDetailDto(Vertrag v) => new(
        v.Id,
        v.VertragNummer,
        v.Status,
        v.LeasingantragId,
        v.Leasingantrag.AntragNummer,
        v.Leasingantrag.Leasinggesellschaft?.Name,
        v.Leasingantrag.EingereichtVon.Anzeigename,
        v.VertragtypId,
        v.Vertragstyp?.Bezeichnung,
        v.Vertragsbeginn,
        v.Vertragsende,
        v.LaufzeitMonate,
        v.Finanzierungsbetrag,
        v.Restwert,
        v.MonatlicheRate,
        v.Zinssatz,
        v.ErstelltAm,
        v.GeaendertAm,
        v.Anhaenge.Select(a => new AnhangDto(a.Id, a.Dateiname, a.Typ,
            a.DateigroesseBytes, a.HochgeladenVon.Anzeigename, a.ErstelltAm)),
        v.Leasingantrag.Objekte.Select(o => new LeasingobjektDto(
            o.Id, o.Bezeichnung, o.IstNeu, o.Listenpreis, o.Rabatt,
            o.FinanzierungsBasis, o.NAK, o.Hersteller, o.Lieferant,
            o.Geraetetyp?.Bezeichnung))
    );

    private async Task<string> GeneriereVertragNummer()
    {
        var count = await _db.Vertraege.CountAsync();
        return $"VTG-{DateTime.UtcNow:yyyy}-{count + 1:D5}";
    }

    private static bool IstStatusUebergangErlaubt(VertragStatus von, VertragStatus nach) => (von, nach) switch
    {
        (VertragStatus.InVorbereitung, VertragStatus.Aktiv) => true,
        (VertragStatus.Aktiv, VertragStatus.Beendet) => true,
        (VertragStatus.Aktiv, VertragStatus.Gekuendigt) => true,
        (VertragStatus.Beendet, VertragStatus.Archiviert) => true,
        (VertragStatus.Gekuendigt, VertragStatus.Archiviert) => true,
        _ => false
    };
}
