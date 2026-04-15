using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Entities;
using LeasiNetWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LeasiNetWeb.Application.Services;

public class AdminService : IAdminService
{
    private readonly IApplicationDbContext _db;
    private readonly IAuthService _auth;

    public AdminService(IApplicationDbContext db, IAuthService auth)
    {
        _db = db;
        _auth = auth;
    }

    // ── Benutzer ──────────────────────────────────────────────────────────────

    public async Task<IEnumerable<BenutzerListeDto>> GetBenutzerAsync() =>
        await _db.Benutzer
            .Include(b => b.Leasinggesellschaft)
            .OrderBy(b => b.Nachname).ThenBy(b => b.Vorname)
            .Select(b => new BenutzerListeDto(
                b.Id, b.Benutzername,
                (b.Vorname + " " + b.Nachname).Trim(),
                b.EMail, b.Rolle, b.IstAktiv,
                b.Leasinggesellschaft != null ? b.Leasinggesellschaft.Name : null,
                b.ErstelltAm))
            .ToListAsync();

    public async Task<BenutzerDetailDto?> GetBenutzerByIdAsync(int id)
    {
        var b = await _db.Benutzer.FindAsync(id);
        if (b is null) return null;
        return new BenutzerDetailDto(b.Id, b.Benutzername, b.Vorname, b.Nachname,
            b.EMail, b.Rolle, b.IstAktiv, b.LeasinggesellschaftId);
    }

    public async Task<BenutzerDetailDto> BenutzerErstellenAsync(BenutzerErstellenDto dto)
    {
        if (await _db.Benutzer.AnyAsync(b => b.Benutzername == dto.Benutzername))
            throw new InvalidOperationException($"Benutzername '{dto.Benutzername}' ist bereits vergeben.");

        var benutzer = new Benutzer
        {
            Benutzername = dto.Benutzername,
            Vorname = dto.Vorname,
            Nachname = dto.Nachname,
            EMail = dto.EMail,
            Rolle = dto.Rolle,
            IstAktiv = dto.IstAktiv,
            LeasinggesellschaftId = dto.LeasinggesellschaftId,
            PasswortHash = _auth.HashPasswort(dto.Passwort),
            ErstelltAm = DateTime.UtcNow,
            GeaendertAm = DateTime.UtcNow
        };

        _db.Benutzer.Add(benutzer);
        await _db.SaveChangesAsync();

        return new BenutzerDetailDto(benutzer.Id, benutzer.Benutzername, benutzer.Vorname,
            benutzer.Nachname, benutzer.EMail, benutzer.Rolle, benutzer.IstAktiv,
            benutzer.LeasinggesellschaftId);
    }

    public async Task BenutzerBearbeitenAsync(BenutzerBearbeitenDto dto)
    {
        var benutzer = await _db.Benutzer.FindAsync(dto.Id)
            ?? throw new KeyNotFoundException("Benutzer nicht gefunden.");

        if (await _db.Benutzer.AnyAsync(b => b.Benutzername == dto.Benutzername && b.Id != dto.Id))
            throw new InvalidOperationException($"Benutzername '{dto.Benutzername}' ist bereits vergeben.");

        benutzer.Benutzername = dto.Benutzername;
        benutzer.Vorname = dto.Vorname;
        benutzer.Nachname = dto.Nachname;
        benutzer.EMail = dto.EMail;
        benutzer.Rolle = dto.Rolle;
        benutzer.IstAktiv = dto.IstAktiv;
        benutzer.LeasinggesellschaftId = dto.LeasinggesellschaftId;
        benutzer.GeaendertAm = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(dto.NeuesPasswort))
            benutzer.PasswortHash = _auth.HashPasswort(dto.NeuesPasswort);

        await _db.SaveChangesAsync();
    }

    public async Task BenutzerDeaktivierenAsync(int id)
    {
        var benutzer = await _db.Benutzer.FindAsync(id)
            ?? throw new KeyNotFoundException("Benutzer nicht gefunden.");
        benutzer.IstAktiv = false;
        benutzer.GeaendertAm = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    public async Task BenutzerAktivierenAsync(int id)
    {
        var benutzer = await _db.Benutzer.FindAsync(id)
            ?? throw new KeyNotFoundException("Benutzer nicht gefunden.");
        benutzer.IstAktiv = true;
        benutzer.GeaendertAm = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    // ── Leasinggesellschaften ─────────────────────────────────────────────────

    public async Task<IEnumerable<LeasinggesellschaftListeDto>> GetLeasinggesellschaftenAsync() =>
        await _db.Leasinggesellschaften
            .OrderBy(l => l.Name)
            .Select(l => new LeasinggesellschaftListeDto(
                l.Id, l.Name, l.Kurzbezeichnung, l.EMail, l.Telefon,
                l.ObligoLimit, l.IstAktiv,
                l.Benutzer.Count))
            .ToListAsync();

    public async Task<LeasinggesellschaftDetailDto?> GetLeasinggesellschaftByIdAsync(int id)
    {
        var l = await _db.Leasinggesellschaften.FindAsync(id);
        if (l is null) return null;
        return ToDetailDto(l);
    }

    public async Task<LeasinggesellschaftDetailDto> LeasinggesellschaftErstellenAsync(LeasinggesellschaftDetailDto dto)
    {
        var lg = new Leasinggesellschaft
        {
            Name = dto.Name,
            Kurzbezeichnung = dto.Kurzbezeichnung,
            Strasse = dto.Strasse,
            PLZ = dto.PLZ,
            Ort = dto.Ort,
            Land = dto.Land ?? "DE",
            Telefon = dto.Telefon,
            EMail = dto.EMail,
            Ansprechpartner = dto.Ansprechpartner,
            ObligoLimit = dto.ObligoLimit,
            IstAktiv = dto.IstAktiv,
            ErstelltAm = DateTime.UtcNow,
            GeaendertAm = DateTime.UtcNow
        };
        _db.Leasinggesellschaften.Add(lg);
        await _db.SaveChangesAsync();
        return ToDetailDto(lg);
    }

    public async Task LeasinggesellschaftBearbeitenAsync(LeasinggesellschaftDetailDto dto)
    {
        var lg = await _db.Leasinggesellschaften.FindAsync(dto.Id)
            ?? throw new KeyNotFoundException("Leasinggesellschaft nicht gefunden.");

        lg.Name = dto.Name;
        lg.Kurzbezeichnung = dto.Kurzbezeichnung;
        lg.Strasse = dto.Strasse;
        lg.PLZ = dto.PLZ;
        lg.Ort = dto.Ort;
        lg.Land = dto.Land ?? "DE";
        lg.Telefon = dto.Telefon;
        lg.EMail = dto.EMail;
        lg.Ansprechpartner = dto.Ansprechpartner;
        lg.ObligoLimit = dto.ObligoLimit;
        lg.IstAktiv = dto.IstAktiv;
        lg.GeaendertAm = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    private static LeasinggesellschaftDetailDto ToDetailDto(Leasinggesellschaft l) =>
        new(l.Id, l.Name, l.Kurzbezeichnung, l.Strasse, l.PLZ, l.Ort, l.Land,
            l.Telefon, l.EMail, l.Ansprechpartner, l.ObligoLimit, l.IstAktiv);

    // ── Ablehnungsgründe ──────────────────────────────────────────────────────

    public async Task<IEnumerable<AblehnungsgrundDto>> GetAblehnungsgruendeAsync() =>
        await _db.Ablehnungsgruende
            .OrderBy(a => a.Code)
            .Select(a => new AblehnungsgrundDto(a.Id, a.Code, a.Bezeichnung, a.IstAktiv))
            .ToListAsync();

    public async Task AblehnungsgrundSpeichernAsync(AblehnungsgrundDto dto)
    {
        if (dto.Id == 0)
        {
            _db.Ablehnungsgruende.Add(new Ablehnungsgrund
            {
                Code = dto.Code.ToUpper(),
                Bezeichnung = dto.Bezeichnung,
                IstAktiv = dto.IstAktiv,
                ErstelltAm = DateTime.UtcNow,
                GeaendertAm = DateTime.UtcNow
            });
        }
        else
        {
            var ag = await _db.Ablehnungsgruende.FindAsync(dto.Id)
                ?? throw new KeyNotFoundException("Ablehnungsgrund nicht gefunden.");
            ag.Code = dto.Code.ToUpper();
            ag.Bezeichnung = dto.Bezeichnung;
            ag.IstAktiv = dto.IstAktiv;
            ag.GeaendertAm = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
    }

    public async Task AblehnungsgrundLoeschenAsync(int id)
    {
        var ag = await _db.Ablehnungsgruende.FindAsync(id)
            ?? throw new KeyNotFoundException("Ablehnungsgrund nicht gefunden.");

        bool inVerwendung = await _db.Leasingantraege.AnyAsync(a => a.AblehnungsgrundId == id);
        if (inVerwendung)
            throw new InvalidOperationException("Ablehnungsgrund wird von Anträgen verwendet und kann nicht gelöscht werden.");

        _db.Ablehnungsgruende.Remove(ag);
        await _db.SaveChangesAsync();
    }

    // ── Gerätetypen ───────────────────────────────────────────────────────────

    public async Task<IEnumerable<GeraetetypDto>> GetGeraetetypenAsync() =>
        await _db.Geraetetypen
            .Include(g => g.Elterntyp)
            .OrderBy(g => g.Bezeichnung)
            .Select(g => new GeraetetypDto(
                g.Id, g.Bezeichnung, g.Beschreibung, g.IstAktiv,
                g.ElterntypId,
                g.Elterntyp != null ? g.Elterntyp.Bezeichnung : null))
            .ToListAsync();

    public async Task GeraetetypSpeichernAsync(GeraetetypDto dto)
    {
        if (dto.Id == 0)
        {
            _db.Geraetetypen.Add(new Geraetetyp
            {
                Bezeichnung = dto.Bezeichnung,
                Beschreibung = dto.Beschreibung,
                IstAktiv = dto.IstAktiv,
                ElterntypId = dto.ElterntypId,
                ErstelltAm = DateTime.UtcNow,
                GeaendertAm = DateTime.UtcNow
            });
        }
        else
        {
            var g = await _db.Geraetetypen.FindAsync(dto.Id)
                ?? throw new KeyNotFoundException("Gerätetyp nicht gefunden.");
            g.Bezeichnung = dto.Bezeichnung;
            g.Beschreibung = dto.Beschreibung;
            g.IstAktiv = dto.IstAktiv;
            g.ElterntypId = dto.ElterntypId;
            g.GeaendertAm = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync();
    }

    public async Task GeraetetypLoeschenAsync(int id)
    {
        var g = await _db.Geraetetypen.FindAsync(id)
            ?? throw new KeyNotFoundException("Gerätetyp nicht gefunden.");

        bool hatUntertypen = await _db.Geraetetypen.AnyAsync(x => x.ElterntypId == id);
        if (hatUntertypen)
            throw new InvalidOperationException("Gerätetyp hat Untertypen und kann nicht gelöscht werden.");

        bool inVerwendung = await _db.Leasingobjekte.AnyAsync(o => o.GeraetetypId == id);
        if (inVerwendung)
            throw new InvalidOperationException("Gerätetyp wird von Leasingobjekten verwendet und kann nicht gelöscht werden.");

        _db.Geraetetypen.Remove(g);
        await _db.SaveChangesAsync();
    }
}
