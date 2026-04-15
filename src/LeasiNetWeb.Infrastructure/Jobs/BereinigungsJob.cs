using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LeasiNetWeb.Infrastructure.Jobs;

/// <summary>
/// Replaces the 8 Intrexx timer-based cleanup workflows.
/// Scheduled via Hangfire recurring jobs — no manual cron XML needed.
/// </summary>
public class BereinigungsJob
{
    private readonly IApplicationDbContext _db;
    private readonly ILogger<BereinigungsJob> _logger;

    public BereinigungsJob(IApplicationDbContext db, ILogger<BereinigungsJob> logger)
    {
        _db = db;
        _logger = logger;
    }

    /// <summary>
    /// Soft-archives approved/rejected applications older than the retention period.
    /// Replaces Intrexx "Anträge Löschen" (hard delete) — safer: no data loss.
    /// Runs monthly.
    /// </summary>
    public async Task AntraegeArchivieren(int aufbewahrungsMonaste = 24)
    {
        var grenze = DateTime.UtcNow.AddMonths(-aufbewahrungsMonaste);
        var antraege = await _db.Leasingantraege
            .Where(a => !a.Archiviert
                && (a.Status == AntragStatus.Genehmigt || a.Status == AntragStatus.Abgelehnt || a.Status == AntragStatus.Storniert)
                && a.GeaendertAm < grenze)
            .ToListAsync();

        foreach (var antrag in antraege)
        {
            antrag.Archiviert = true;
            antrag.ArchiviertAm = DateTime.UtcNow;
            antrag.Status = AntragStatus.Archiviert;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("{Count} Anträge archiviert (Aufbewahrung: {Monate} Monate).",
            antraege.Count, aufbewahrungsMonaste);
    }

    /// <summary>
    /// Removes processed sync requests older than 30 days.
    /// Replaces Intrexx "Synchronisierungsanfragen leeren". Runs daily.
    /// </summary>
    public async Task SynchronisierungsAnfragenBereinigen()
    {
        var grenze = DateTime.UtcNow.AddDays(-30);
        var alte = await _db.SynchronisierungsAnfragen
            .Where(s => s.Verarbeitet && s.VerarbeitetAm < grenze)
            .ToListAsync();

        _db.SynchronisierungsAnfragen.RemoveRange(alte);
        await _db.SaveChangesAsync();
        _logger.LogInformation("{Count} verarbeitete Synchronisierungsanfragen gelöscht.", alte.Count);
    }

    /// <summary>
    /// Cleans up orphaned upload files that have no matching Anhang record.
    /// Runs daily. No Intrexx equivalent — new safeguard.
    /// </summary>
    public async Task VerwaisteDateienBereinigen(string uploadPfad)
    {
        if (!Directory.Exists(uploadPfad)) return;

        var dbPfadeListe = await _db.Anhaenge
            .Select(a => a.Dateipfad)
            .ToListAsync();
        var dbPfade = dbPfadeListe.ToHashSet();

        var dateipfade = Directory.GetFiles(uploadPfad, "*", SearchOption.AllDirectories)
            .Select(p => Path.GetRelativePath(uploadPfad, p))
            .Where(p => !dbPfade.Contains(p))
            .ToList();

        foreach (var pfad in dateipfade)
        {
            var vollPfad = Path.Combine(uploadPfad, pfad);
            if (File.Exists(vollPfad)) File.Delete(vollPfad);
        }

        _logger.LogInformation("{Count} verwaiste Dateien gelöscht.", dateipfade.Count);
    }
}
