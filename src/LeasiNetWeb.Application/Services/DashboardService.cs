using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _db;
    private readonly INachrichtService _nachrichten;

    public DashboardService(IApplicationDbContext db, INachrichtService nachrichten)
    {
        _db = db;
        _nachrichten = nachrichten;
    }

    public async Task<DashboardDto> GetDashboardDaten(int benutzerId)
    {
        var antraege = await _db.Leasingantraege
            .Include(a => a.EingereichtVon)
            .Include(a => a.Leasinggesellschaft)
            .Include(a => a.SachbearbeiterMB)
            .Where(a => !a.Archiviert)
            .ToListAsync();

        var meineAntraege = antraege
            .Where(a => a.EingereichtVonId == benutzerId
                     || a.SachbearbeiterMBId == benutzerId
                     || a.SachbearbeiterLGId == benutzerId)
            .OrderByDescending(a => a.GeaendertAm)
            .Take(10)
            .Select(a => new AntragListeDto(a.Id, a.AntragNummer, a.AntragTyp, a.Status,
                a.EingereichtVon.Anzeigename, a.Leasinggesellschaft?.Name,
                a.SachbearbeiterMB?.Anzeigename, a.Obligo, a.ErstelltAm, a.GeaendertAm,
                a.ZweiteVoteErforderlich, a.KiErstellt))
            .ToList();

        var statusZaehler = antraege
            .GroupBy(a => a.Status)
            .Select(g => new StatusZaehlerDto(g.Key, g.Count()))
            .ToList();

        var aktiveVertraege = await _db.Vertraege
            .CountAsync(v => v.Status == VertragStatus.Aktiv);

        var gesamtObligo = await _db.Leasingantraege
            .Where(a => a.Status == AntragStatus.Genehmigt && !a.Archiviert)
            .SumAsync(a => a.Obligo);

        return new DashboardDto(
            OffeneAntraege: antraege.Count(a => a.Status == AntragStatus.Eingereicht),
            AntraegeInBearbeitung: antraege.Count(a =>
                a.Status is AntragStatus.InPruefung or AntragStatus.BeiMitarbeiter or AntragStatus.BeiLeasinggesellschaft),
            ZuPruefendeAntraege: antraege.Count(a => a.SachbearbeiterMBId == benutzerId && a.Status == AntragStatus.InPruefung),
            ZuGenehmigendeAntraege: antraege.Count(a => a.GenehmigerMBId == benutzerId && a.Status == AntragStatus.BeiMitarbeiter),
            PendingZweiteVoten: antraege.Count(a => a.ZweiteVoteErforderlich && a.ZweiteVoteGenehmigerMBId == benutzerId),
            AktiveVertraege: aktiveVertraege,
            UngeleseneNachrichten: await _nachrichten.UngeleseneAnzahl(benutzerId),
            GesamtObligoAktiv: gesamtObligo,
            MeineAktuellenAntraege: meineAntraege,
            AntraegePorStatus: statusZaehler
        );
    }
}
