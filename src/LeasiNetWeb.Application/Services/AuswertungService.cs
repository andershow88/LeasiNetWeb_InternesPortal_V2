using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Application.Services;

public class AuswertungService : IAuswertungService
{
    private readonly IApplicationDbContext _db;

    public AuswertungService(IApplicationDbContext db) => _db = db;

    public async Task<AuswertungDto> GetAuswertungAsync(int jahr)
    {
        // ── Anträge ────────────────────────────────────────────────────────────
        var antraege = await _db.Leasingantraege
            .Include(a => a.EingereichtVon)
            .Include(a => a.Leasinggesellschaft)
            .Include(a => a.SachbearbeiterMB)
            .ToListAsync();

        var antraegeImJahr = antraege
            .Where(a => a.ErstelltAm.Year == jahr)
            .ToList();

        var antraegeNachStatus = antraege
            .Where(a => !a.Archiviert)
            .GroupBy(a => a.Status)
            .Select(g => new StatusZaehlerDto(g.Key, g.Count()))
            .OrderBy(s => s.Status)
            .ToList();

        // Monthly breakdown for the selected year
        var antraegeProMonat = Enumerable.Range(1, 12).Select(m =>
        {
            var monatAntraege = antraegeImJahr.Where(a => a.ErstelltAm.Month == m).ToList();
            return new MonatsZaehlerDto(
                Jahr: jahr,
                Monat: m,
                MonatLabel: new DateTime(jahr, m, 1).ToString("MMM"),
                AnzahlAntraege: monatAntraege.Count,
                AnzahlGenehmigt: monatAntraege.Count(a => a.Status == AntragStatus.Genehmigt || a.Archiviert),
                AnzahlAbgelehnt: monatAntraege.Count(a => a.Status == AntragStatus.Abgelehnt),
                ObligoSumme: monatAntraege.Sum(a => a.Obligo)
            );
        }).ToList();

        var antraegeNachTyp = antraegeImJahr
            .GroupBy(a => a.AntragTyp.ToString())
            .Select(g => new AntragTypZaehlerDto(g.Key, g.Count(), g.Sum(a => a.Obligo)))
            .OrderByDescending(t => t.Anzahl)
            .ToList();

        // ── Obligo pro LG ──────────────────────────────────────────────────────
        var gesellschaften = await _db.Leasinggesellschaften
            .Include(lg => lg.Leasingantraege)
            .ToListAsync();

        var vertraegeProLg = await _db.Vertraege
            .Include(v => v.Leasingantrag)
            .Where(v => v.Status == VertragStatus.Aktiv)
            .ToListAsync();

        var obligoProLg = gesellschaften.Select(lg =>
        {
            var lgAntraege = lg.Leasingantraege
                .Where(a => a.Status == AntragStatus.Genehmigt || a.Status == AntragStatus.BeiMitarbeiter)
                .ToList();
            var aktiveVertraege = vertraegeProLg
                .Count(v => v.Leasingantrag.LeasinggesellschaftId == lg.Id);
            return new LgObligoDto(
                lg.Name,
                lgAntraege.Sum(a => a.Obligo),
                lg.ObligoLimit,
                lgAntraege.Count,
                aktiveVertraege
            );
        })
        .Where(lg => lg.AnzahlAntraege > 0 || lg.AnzahlAktiveVertraege > 0)
        .OrderByDescending(lg => lg.ObligoSumme)
        .ToList();

        // ── Verträge ───────────────────────────────────────────────────────────
        var vertraege = await _db.Vertraege.ToListAsync();

        var vertraegeNachStatus = vertraege
            .GroupBy(v => v.Status)
            .Select(g => new VertragStatusZaehlerDto(g.Key, g.Count(), g.Sum(v => v.Finanzierungsbetrag)))
            .OrderBy(s => s.Status)
            .ToList();

        var aktiveVertraegeList = vertraege.Where(v => v.Status == VertragStatus.Aktiv).ToList();
        var durchschnittRate = aktiveVertraegeList.Any(v => v.MonatlicheRate.HasValue)
            ? aktiveVertraegeList.Where(v => v.MonatlicheRate.HasValue).Average(v => v.MonatlicheRate!.Value)
            : (decimal?)null;

        // ── Benutzeraktivität ──────────────────────────────────────────────────
        var benutzer = await _db.Benutzer
            .Where(b => b.IstAktiv)
            .ToListAsync();

        var benutzerAktivitaet = benutzer
            .Select(b => new BenutzerAktivitaetDto(
                b.Anzeigename,
                b.Rolle.ToString(),
                antraege.Count(a => a.EingereichtVonId == b.Id),
                antraege.Count(a => a.SachbearbeiterMBId == b.Id)
            ))
            .Where(b => b.EingereichtAntraege > 0 || b.BearbeiteteAntraege > 0)
            .OrderByDescending(b => b.EingereichtAntraege + b.BearbeiteteAntraege)
            .ToList();

        // ── Zusammenfassung ────────────────────────────────────────────────────
        var gesamtObligo = antraege
            .Where(a => a.Status == AntragStatus.Genehmigt && !a.Archiviert)
            .Sum(a => a.Obligo);

        return new AuswertungDto(
            Jahr: jahr,
            AntraegeGesamt: antraege.Count,
            AntraegeGenehmigt: antraege.Count(a => a.Status == AntragStatus.Genehmigt || a.Archiviert),
            AntraegeAbgelehnt: antraege.Count(a => a.Status == AntragStatus.Abgelehnt),
            AntraegeOffen: antraege.Count(a => !a.Archiviert &&
                a.Status is AntragStatus.Eingereicht or AntragStatus.InPruefung
                    or AntragStatus.BeiMitarbeiter or AntragStatus.BeiLeasinggesellschaft
                    or AntragStatus.ZweiteVoteErforderlich or AntragStatus.InterneKontrolleErforderlich),
            GesamtObligoAktiv: gesamtObligo,
            AntraegeNachStatus: antraegeNachStatus,
            AntraegeProMonat: antraegeProMonat,
            AntraegeNachTyp: antraegeNachTyp,
            ObligoProLg: obligoProLg,
            VertraegeGesamt: vertraege.Count,
            VertraegeAktiv: aktiveVertraegeList.Count,
            GesamtFinanzierungsvolumen: vertraege.Sum(v => v.Finanzierungsbetrag),
            DurchschnittlicheMonatlicheRate: durchschnittRate,
            VertraegeNachStatus: vertraegeNachStatus,
            BenutzerAktivitaet: benutzerAktivitaet
        );
    }

    public async Task<IEnumerable<AntragListeDto>> GetAntraegeFuerExportAsync(int? jahr = null)
    {
        var query = _db.Leasingantraege
            .Include(a => a.EingereichtVon)
            .Include(a => a.Leasinggesellschaft)
            .Include(a => a.SachbearbeiterMB)
            .AsQueryable();

        if (jahr.HasValue)
            query = query.Where(a => a.ErstelltAm.Year == jahr.Value);

        return await query
            .OrderByDescending(a => a.ErstelltAm)
            .Select(a => new AntragListeDto(
                a.Id, a.AntragNummer, a.AntragTyp, a.Status,
                a.EingereichtVon.Anzeigename,
                a.Leasinggesellschaft != null ? a.Leasinggesellschaft.Name : null,
                a.SachbearbeiterMB != null ? a.SachbearbeiterMB.Anzeigename : null,
                a.Obligo, a.ErstelltAm, a.GeaendertAm, a.ZweiteVoteErforderlich, a.KiErstellt))
            .ToListAsync();
    }

    public async Task<IEnumerable<VertragListeDto>> GetVertraegeFuerExportAsync()
    {
        return await _db.Vertraege
            .Include(v => v.Leasingantrag).ThenInclude(a => a.Leasinggesellschaft)
            .Include(v => v.Vertragstyp)
            .OrderByDescending(v => v.ErstelltAm)
            .Select(v => new VertragListeDto(
                v.Id, v.VertragNummer, v.Status, v.LeasingantragId,
                v.Leasingantrag.AntragNummer,
                v.Leasingantrag.Leasinggesellschaft != null ? v.Leasingantrag.Leasinggesellschaft.Name : null,
                v.Vertragstyp != null ? v.Vertragstyp.Bezeichnung : null,
                v.Finanzierungsbetrag, v.Vertragsbeginn, v.Vertragsende, v.LaufzeitMonate))
            .ToListAsync();
    }
}
