using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Entities;
using LeasiNetWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace LeasiNetWeb.Infrastructure.Data;

public class AnhangService : IAnhangService
{
    private readonly IApplicationDbContext _db;
    private readonly string _uploadPfad;

    private static readonly HashSet<string> ErlaubteContentTypes = new HashSet<string>
    {
        "application/pdf",
        "image/jpeg", "image/png",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.ms-excel",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    private const long MaxDateigroesse = 20 * 1024 * 1024; // 20 MB

    public AnhangService(IApplicationDbContext db, IConfiguration config)
    {
        _db = db;
        _uploadPfad = config["Uploads:Pfad"] ?? Path.Combine("wwwroot", "uploads");
    }

    public async Task<AnhangDto> HochladenAsync(Stream dateiStream, string dateiname, string contentType,
        long dateigroesse, AnhangTyp typ, int hochgeladenVonId,
        int? antragId = null, int? kommentarId = null, int? pruefungId = null, int? vertragId = null)
    {
        if (dateigroesse > MaxDateigroesse)
            throw new InvalidOperationException("Datei überschreitet die maximale Größe von 20 MB.");

        if (!ErlaubteContentTypes.Contains(contentType))
            throw new InvalidOperationException($"Dateityp '{contentType}' ist nicht erlaubt.");

        var unterordner = DateTime.UtcNow.ToString("yyyy/MM");
        var zielOrdner = Path.Combine(_uploadPfad, unterordner);
        Directory.CreateDirectory(zielOrdner);

        var gespeicherterDateiname = $"{Guid.NewGuid()}{Path.GetExtension(dateiname)}";
        var vollstaendigerPfad = Path.Combine(zielOrdner, gespeicherterDateiname);

        await using (var fs = new FileStream(vollstaendigerPfad, FileMode.Create))
            await dateiStream.CopyToAsync(fs);

        var benutzer = await _db.Benutzer.FindAsync(hochgeladenVonId)
            ?? throw new KeyNotFoundException("Benutzer nicht gefunden.");

        var anhang = new Anhang
        {
            Typ = typ,
            Dateiname = dateiname,
            Dateipfad = Path.Combine(unterordner, gespeicherterDateiname),
            ContentType = contentType,
            DateigroesseBytes = dateigroesse,
            HochgeladenVonId = hochgeladenVonId,
            LeasingantragId = antragId,
            KommentarId = kommentarId,
            InternePruefungId = pruefungId,
            VertragId = vertragId,
            ErstelltAm = DateTime.UtcNow,
            GeaendertAm = DateTime.UtcNow,
            ErstelltVonId = hochgeladenVonId,
            GeaendertVonId = hochgeladenVonId
        };

        _db.Anhaenge.Add(anhang);
        await _db.SaveChangesAsync();

        return new AnhangDto(anhang.Id, anhang.Dateiname, anhang.Typ,
            anhang.DateigroesseBytes, benutzer.Anzeigename, anhang.ErstelltAm);
    }

    public async Task<(byte[] Inhalt, string ContentType, string Dateiname)> HerunterladenAsync(int anhangId)
    {
        var anhang = await _db.Anhaenge.FindAsync(anhangId)
            ?? throw new KeyNotFoundException("Anhang nicht gefunden.");

        var vollstaendigerPfad = Path.Combine(_uploadPfad, anhang.Dateipfad);
        if (!File.Exists(vollstaendigerPfad))
            throw new FileNotFoundException("Datei nicht gefunden.", vollstaendigerPfad);

        return (await File.ReadAllBytesAsync(vollstaendigerPfad),
                anhang.ContentType ?? "application/octet-stream",
                anhang.Dateiname);
    }

    public async Task LoeschenAsync(int anhangId, int benutzerId)
    {
        var anhang = await _db.Anhaenge.FindAsync(anhangId)
            ?? throw new KeyNotFoundException("Anhang nicht gefunden.");

        var vollstaendigerPfad = Path.Combine(_uploadPfad, anhang.Dateipfad);
        if (File.Exists(vollstaendigerPfad))
            File.Delete(vollstaendigerPfad);

        _db.Anhaenge.Remove(anhang);
        await _db.SaveChangesAsync();
    }

    public async Task<IEnumerable<AnhangDto>> GetAnhaengeAsync(int? antragId = null, int? kommentarId = null, int? pruefungId = null)
    {
        var query = _db.Anhaenge
            .Include(a => a.HochgeladenVon)
            .AsQueryable();

        if (antragId.HasValue) query = query.Where(a => a.LeasingantragId == antragId);
        if (kommentarId.HasValue) query = query.Where(a => a.KommentarId == kommentarId);
        if (pruefungId.HasValue) query = query.Where(a => a.InternePruefungId == pruefungId);

        return await query
            .OrderByDescending(a => a.ErstelltAm)
            .Select(a => new AnhangDto(a.Id, a.Dateiname, a.Typ, a.DateigroesseBytes,
                a.HochgeladenVon.Anzeigename, a.ErstelltAm))
            .ToListAsync();
    }
}
