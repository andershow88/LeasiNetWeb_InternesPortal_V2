using System.Text.Json;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Entities;
using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Application.Services;

public class EreignisService : IEreignisService
{
    private readonly IApplicationDbContext _db;

    public EreignisService(IApplicationDbContext db) => _db = db;

    public async Task EreignisAufzeichnen(int antragId, EreignisTyp typ, int ausgeloestVonId,
        string? beschreibung = null, object? nutzlast = null)
    {
        var ereignis = new Ereignis
        {
            LeasingantragId = antragId,
            Typ = typ,
            AusgeloestVonId = ausgeloestVonId,
            Beschreibung = beschreibung,
            Nutzlast = nutzlast is not null ? JsonSerializer.Serialize(nutzlast) : null,
            ErstelltAm = DateTime.UtcNow,
            GeaendertAm = DateTime.UtcNow,
            ErstelltVonId = ausgeloestVonId,
            GeaendertVonId = ausgeloestVonId
        };

        _db.Ereignisse.Add(ereignis);
        await _db.SaveChangesAsync();

        // Notifications are dispatched asynchronously via Hangfire (wired in Infrastructure)
        await BenachrichtigungenVersenden(ereignis.Id);
    }

    public async Task BenachrichtigungenVersenden(int ereignisId)
    {
        // Notification dispatch logic is implemented in Infrastructure layer
        // where email/push providers are available. This service records the event;
        // the Hangfire background job handles delivery.
        await Task.CompletedTask;
    }
}
