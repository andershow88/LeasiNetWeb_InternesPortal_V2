using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Application.Services;

public class NachrichtService : INachrichtService
{
    private readonly IApplicationDbContext _db;

    public NachrichtService(IApplicationDbContext db) => _db = db;

    public async Task<IEnumerable<NachrichtDto>> GetPosteingang(int benutzerId)
    {
        return await _db.Nachrichten
            .Include(n => n.Absender)
            .Include(n => n.Leasingantrag)
            .Where(n => n.EmpfaengerId == benutzerId)
            .OrderByDescending(n => n.ErstelltAm)
            .Select(n => new NachrichtDto(n.Id, n.Absender.Anzeigename, n.Betreff,
                n.Text, n.Gelesen, n.GelesenAm, n.ErstelltAm,
                n.LeasingantragId, n.Leasingantrag != null ? n.Leasingantrag.AntragNummer : null))
            .ToListAsync();
    }

    public async Task<int> UngeleseneAnzahl(int benutzerId)
        => await _db.Nachrichten.CountAsync(n => n.EmpfaengerId == benutzerId && !n.Gelesen);

    public async Task<NachrichtDto?> GetNachricht(int id, int benutzerId)
    {
        var n = await _db.Nachrichten
            .Include(n => n.Absender)
            .Include(n => n.Leasingantrag)
            .FirstOrDefaultAsync(n => n.Id == id && n.EmpfaengerId == benutzerId);

        if (n is null) return null;

        return new NachrichtDto(n.Id, n.Absender.Anzeigename, n.Betreff, n.Text,
            n.Gelesen, n.GelesenAm, n.ErstelltAm,
            n.LeasingantragId, n.Leasingantrag?.AntragNummer);
    }

    public async Task<int> Senden(NachrichtSendenDto dto, int absenderId)
    {
        var nachricht = new Nachricht
        {
            AbsenderId = absenderId,
            EmpfaengerId = dto.EmpfaengerId,
            LeasingantragId = dto.LeasingantragId,
            Betreff = dto.Betreff,
            Text = dto.Text,
            Gelesen = false,
            ErstelltAm = DateTime.UtcNow,
            GeaendertAm = DateTime.UtcNow,
            ErstelltVonId = absenderId,
            GeaendertVonId = absenderId
        };

        _db.Nachrichten.Add(nachricht);
        await _db.SaveChangesAsync();
        return nachricht.Id;
    }

    public async Task AlsGelesenMarkieren(int nachrichtId, int benutzerId)
    {
        var nachricht = await _db.Nachrichten
            .FirstOrDefaultAsync(n => n.Id == nachrichtId && n.EmpfaengerId == benutzerId);

        if (nachricht is null || nachricht.Gelesen) return;

        nachricht.Gelesen = true;
        nachricht.GelesenAm = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }
}
