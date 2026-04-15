using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Entities;
using LeasiNetWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Application.Services;

public class KommentarService : IKommentarService
{
    private readonly IApplicationDbContext _db;
    private readonly IEreignisService _ereignis;

    public KommentarService(IApplicationDbContext db, IEreignisService ereignis)
    {
        _db = db;
        _ereignis = ereignis;
    }

    public async Task<IEnumerable<KommentarDto>> GetKommentare(int antragId, bool nurOeffentliche = false)
    {
        var query = _db.Kommentare
            .Include(k => k.Autor)
            .Include(k => k.Anhaenge).ThenInclude(a => a.HochgeladenVon)
            .Where(k => k.LeasingantragId == antragId);

        if (nurOeffentliche)
            query = query.Where(k => !k.IstIntern);

        return await query
            .OrderByDescending(k => k.ErstelltAm)
            .Select(k => new KommentarDto(
                k.Id,
                k.Autor.Anzeigename,
                k.Text,
                k.IstIntern,
                k.ErstelltAm,
                k.Anhaenge.Select(a => new AnhangDto(a.Id, a.Dateiname, a.Typ,
                    a.DateigroesseBytes, a.HochgeladenVon.Anzeigename, a.ErstelltAm))
            ))
            .ToListAsync();
    }

    public async Task<int> HinzufuegenAsync(KommentarErstellenDto dto, int autorId)
    {
        var kommentar = new Kommentar
        {
            LeasingantragId = dto.LeasingantragId,
            AutorId = autorId,
            Text = dto.Text,
            IstIntern = dto.IstIntern,
            ErstelltAm = DateTime.UtcNow,
            GeaendertAm = DateTime.UtcNow,
            ErstelltVonId = autorId,
            GeaendertVonId = autorId
        };

        _db.Kommentare.Add(kommentar);
        await _db.SaveChangesAsync();

        await _ereignis.EreignisAufzeichnen(dto.LeasingantragId,
            EreignisTyp.KommentarHinzugefuegt, autorId);

        return kommentar.Id;
    }
}
