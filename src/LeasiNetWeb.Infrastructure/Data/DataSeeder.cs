using LeasiNetWeb.Domain.Entities;
using LeasiNetWeb.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace LeasiNetWeb.Infrastructure.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(ApplicationDbContext db)
    {
        await db.Database.EnsureCreatedAsync();

        if (!await db.Benutzer.AnyAsync())
            await SeedStammdatenAsync(db);

        // Ensure all 5 demo LGs exist (safe to run on existing deployments)
        await SeedFehlendeLeasinggesellschaftenAsync(db);

        if (!await db.Leasingantraege.AnyAsync())
            await SeedDemoAntraegeAsync(db);
    }

    // ── Fehlende Leasinggesellschaften nachträglich hinzufügen ────────────────
    private static async Task SeedFehlendeLeasinggesellschaftenAsync(ApplicationDbContext db)
    {
        var vorhandeneKuerzel = await db.Leasinggesellschaften
            .Select(l => l.Kurzbezeichnung)
            .ToListAsync();

        var neu = new List<Leasinggesellschaft>();
        if (!vorhandeneKuerzel.Contains("DL-AG"))
            neu.Add(new() { Name = "Deutsche Leasing AG", Kurzbezeichnung = "DL-AG", Land = "DE", EMail = "kontakt@deutsche-leasing.de", ObligoLimit = 8_000_000, IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts });
        if (!vorhandeneKuerzel.Contains("VR-LG"))
            neu.Add(new() { Name = "VR Leasing GmbH",    Kurzbezeichnung = "VR-LG", Land = "DE", EMail = "service@vrleasing.de",        ObligoLimit = 3_500_000, IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts });
        if (!vorhandeneKuerzel.Contains("GR-AG"))
            neu.Add(new() { Name = "Grenke Leasing AG",  Kurzbezeichnung = "GR-AG", Land = "DE", EMail = "info@grenke.de",              ObligoLimit = 4_000_000, IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts });

        if (neu.Any())
        {
            db.Leasinggesellschaften.AddRange(neu);
            await db.SaveChangesAsync();
        }
    }

    // ── Stammdaten & Benutzer (einmalig) ─────────────────────────────────────
    private static async Task SeedStammdatenAsync(ApplicationDbContext db)
    {
        // ── Ablehnungsgründe ──────────────────────────────────────────────────
        db.Ablehnungsgruende.AddRange(
            new() { Code = "BONITÄT",   Bezeichnung = "Unzureichende Bonität",       IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts },
            new() { Code = "UNTERLAGEN",Bezeichnung = "Unvollständige Unterlagen",   IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts },
            new() { Code = "OBLIGO",    Bezeichnung = "Obligo-Limit überschritten",  IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts },
            new() { Code = "SONSTIGES", Bezeichnung = "Sonstiger Grund",             IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts }
        );

        // ── Vertragstypen ─────────────────────────────────────────────────────
        db.Vertragstypen.AddRange(
            new() { Code = "KFZ",       Bezeichnung = "KFZ-Leasing",       IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts },
            new() { Code = "MASCHINEN", Bezeichnung = "Maschinenleasing",   IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts },
            new() { Code = "IT",        Bezeichnung = "IT-Leasing",         IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts },
            new() { Code = "IMMOBILIEN",Bezeichnung = "Immobilienleasing",  IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts }
        );

        // ── Gerätetypen ────────────────────────────────────────────────────────
        db.Geraetetypen.AddRange(
            new() { Bezeichnung = "Fahrzeuge",          IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts },
            new() { Bezeichnung = "Maschinen & Anlagen",IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts },
            new() { Bezeichnung = "IT-Ausstattung",     IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts },
            new() { Bezeichnung = "Medizintechnik",     IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts }
        );

        // ── Leasinggesellschaften ─────────────────────────────────────────────
        var lg1 = new Leasinggesellschaft { Name = "Musterbank AG",         Kurzbezeichnung = "MB-AG",    Land = "DE", EMail = "kontakt@musterbank.de",    ObligoLimit = 5_000_000, IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts };
        var lg2 = new Leasinggesellschaft { Name = "Demo Leasing GmbH",     Kurzbezeichnung = "DL-GmbH",  Land = "DE", EMail = "info@demoleasing.de",       ObligoLimit = 2_500_000, IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts };
        var lg3 = new Leasinggesellschaft { Name = "Deutsche Leasing AG",   Kurzbezeichnung = "DL-AG",    Land = "DE", EMail = "kontakt@deutsche-leasing.de",ObligoLimit = 8_000_000, IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts };
        var lg4 = new Leasinggesellschaft { Name = "VR Leasing GmbH",       Kurzbezeichnung = "VR-LG",    Land = "DE", EMail = "service@vrleasing.de",       ObligoLimit = 3_500_000, IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts };
        var lg5 = new Leasinggesellschaft { Name = "Grenke Leasing AG",     Kurzbezeichnung = "GR-AG",    Land = "DE", EMail = "info@grenke.de",             ObligoLimit = 4_000_000, IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts };
        db.Leasinggesellschaften.AddRange(lg1, lg2, lg3, lg4, lg5);

        // ── Benutzer ──────────────────────────────────────────────────────────
        static string Hash(string pw) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(pw))).ToLower();

        var admin         = new Benutzer { Benutzername = "admin",              PasswortHash = Hash("Admin1234!"), Vorname = "System",  Nachname = "Administrator", EMail = "admin@leasinetweb.de",            Rolle = BenutzerRolle.Administrator,   IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts };
        var sachbearbeiter= new Benutzer { Benutzername = "sachbearbeiter.mb",  PasswortHash = Hash("Demo1234!"),  Vorname = "Max",     Nachname = "Mustermann",    EMail = "max.mustermann@leasinetweb.de",   Rolle = BenutzerRolle.SachbearbeiterMB,IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts };
        var genehmiger    = new Benutzer { Benutzername = "genehmiger.mb",      PasswortHash = Hash("Demo1234!"),  Vorname = "Anna",    Nachname = "Musterfrau",    EMail = "anna.musterfrau@leasinetweb.de",  Rolle = BenutzerRolle.Genehmiger,      IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts };
        var lgUser        = new Benutzer { Benutzername = "sachbearbeiter.lg",  PasswortHash = Hash("Demo1234!"),  Vorname = "Klaus",   Nachname = "Beispiel",      EMail = "k.beispiel@musterbank.de",        Rolle = BenutzerRolle.SachbearbeiterLG,IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts, Leasinggesellschaft = lg1 };
        var pruefer       = new Benutzer { Benutzername = "pruefer.mb",         PasswortHash = Hash("Demo1234!"),  Vorname = "Sandra",  Nachname = "Prüferin",      EMail = "s.prueferin@leasinetweb.de",      Rolle = BenutzerRolle.InternerPruefer, IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts };
        db.Benutzer.AddRange(admin, sachbearbeiter, genehmiger, lgUser, pruefer);

        // ── Hilfe-Texte ────────────────────────────────────────────────────────
        db.HilfeTexte.AddRange(
            new HilfeText { Schluessel = "dashboard", Titel = "Dashboard",       Inhalt = "Das Dashboard zeigt Ihre wichtigsten Kennzahlen auf einen Blick.", IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts },
            new HilfeText { Schluessel = "antraege",  Titel = "Leasinganträge",  Inhalt = "Hier finden Sie alle Leasinganträge. Nutzen Sie Filter und Suche zur schnellen Navigation.", IstAktiv = true, ErstelltAm = Ts, GeaendertAm = Ts }
        );

        await db.SaveChangesAsync();
    }

    // ── Demo-Anträge (100 Stück) ──────────────────────────────────────────────
    private static async Task SeedDemoAntraegeAsync(ApplicationDbContext db)
    {
        // Load references
        var benutzer      = await db.Benutzer.ToListAsync();
        var gesellschaften= await db.Leasinggesellschaften.Where(l => l.IstAktiv).ToListAsync();
        var ablehnungsgruende = await db.Ablehnungsgruende.ToListAsync();

        if (!benutzer.Any() || !gesellschaften.Any()) return;

        var einreicher   = benutzer.First(b => b.Benutzername == "sachbearbeiter.mb");
        var sbMB         = benutzer.First(b => b.Benutzername == "sachbearbeiter.mb");
        var genehm       = benutzer.First(b => b.Benutzername == "genehmiger.mb");
        var ablehnGrund  = ablehnungsgruende.FirstOrDefault();

        // Deterministic obligo values (€ 8 000 – € 480 000)
        decimal[] obligos = [
             8_000,  12_500,  15_000,  18_750,  22_000,
            28_500,  35_000,  42_000,  50_000,  60_000,
            75_000,  90_000, 105_000, 120_000, 145_000,
           160_000, 185_000, 210_000, 240_000, 275_000,
           300_000, 330_000, 360_000, 400_000, 480_000
        ];

        // Statuses distributed across 100 entries
        // ~25 Genehmigt, ~20 BeiMitarbeiter, ~15 InPruefung, ~10 Eingereicht,
        // ~10 Abgelehnt, ~8 BeiLeasinggesellschaft, ~6 Archiviert, ~3 Entwurf, ~3 InterneKontrolle
        var statusPool = new AntragStatus[]
        {
            AntragStatus.Genehmigt, AntragStatus.Genehmigt, AntragStatus.Genehmigt,
            AntragStatus.Genehmigt, AntragStatus.Genehmigt, AntragStatus.Genehmigt,
            AntragStatus.Genehmigt, AntragStatus.Genehmigt, AntragStatus.Genehmigt,
            AntragStatus.Genehmigt, AntragStatus.Genehmigt, AntragStatus.Genehmigt,
            AntragStatus.Genehmigt, AntragStatus.Genehmigt, AntragStatus.Genehmigt,
            AntragStatus.Genehmigt, AntragStatus.Genehmigt, AntragStatus.Genehmigt,
            AntragStatus.Genehmigt, AntragStatus.Genehmigt, AntragStatus.Genehmigt,
            AntragStatus.Genehmigt, AntragStatus.Genehmigt, AntragStatus.Genehmigt,
            AntragStatus.Genehmigt,
            AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter,
            AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter,
            AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter,
            AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter,
            AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter,
            AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter,
            AntragStatus.BeiMitarbeiter, AntragStatus.BeiMitarbeiter,
            AntragStatus.InPruefung, AntragStatus.InPruefung, AntragStatus.InPruefung,
            AntragStatus.InPruefung, AntragStatus.InPruefung, AntragStatus.InPruefung,
            AntragStatus.InPruefung, AntragStatus.InPruefung, AntragStatus.InPruefung,
            AntragStatus.InPruefung, AntragStatus.InPruefung, AntragStatus.InPruefung,
            AntragStatus.InPruefung, AntragStatus.InPruefung, AntragStatus.InPruefung,
            AntragStatus.Eingereicht, AntragStatus.Eingereicht, AntragStatus.Eingereicht,
            AntragStatus.Eingereicht, AntragStatus.Eingereicht, AntragStatus.Eingereicht,
            AntragStatus.Eingereicht, AntragStatus.Eingereicht, AntragStatus.Eingereicht,
            AntragStatus.Eingereicht,
            AntragStatus.Abgelehnt, AntragStatus.Abgelehnt, AntragStatus.Abgelehnt,
            AntragStatus.Abgelehnt, AntragStatus.Abgelehnt, AntragStatus.Abgelehnt,
            AntragStatus.Abgelehnt, AntragStatus.Abgelehnt, AntragStatus.Abgelehnt,
            AntragStatus.Abgelehnt,
            AntragStatus.BeiLeasinggesellschaft, AntragStatus.BeiLeasinggesellschaft,
            AntragStatus.BeiLeasinggesellschaft, AntragStatus.BeiLeasinggesellschaft,
            AntragStatus.BeiLeasinggesellschaft, AntragStatus.BeiLeasinggesellschaft,
            AntragStatus.BeiLeasinggesellschaft, AntragStatus.BeiLeasinggesellschaft,
            AntragStatus.Archiviert, AntragStatus.Archiviert, AntragStatus.Archiviert,
            AntragStatus.Archiviert, AntragStatus.Archiviert, AntragStatus.Archiviert,
            AntragStatus.InterneKontrolleErforderlich, AntragStatus.InterneKontrolleErforderlich,
            AntragStatus.InterneKontrolleErforderlich,
            AntragStatus.Entwurf, AntragStatus.Entwurf, AntragStatus.Entwurf,
            AntragStatus.ZweiteVoteErforderlich, AntragStatus.ZweiteVoteErforderlich,
        };

        var typPool = new[] {
            AntragTyp.Neugeschaeft, AntragTyp.Neugeschaeft, AntragTyp.Neugeschaeft,
            AntragTyp.Neugeschaeft, AntragTyp.Neugeschaeft, AntragTyp.Prolongation,
            AntragTyp.Prolongation, AntragTyp.Abloesung, AntragTyp.Abloesung,
            AntragTyp.Rahmenvertrag
        };

        var antragNamen = new[]
        {
            "Fuhrparkleasing PKW",     "Maschinenleasing Presse",   "IT-Infrastruktur Upgrade",
            "Medizingerät CT-Scanner",  "Gabelstapler Flotte",       "Server & Storage",
            "KFZ Transporter",          "CNC-Fräsmaschine",          "Laptop-Fleet 200 Stück",
            "MRT-Gerät Klinik",         "LKW Fernverkehr",           "Schweißanlage",
            "Netzwerk-Equipment",       "Röntgengerät",              "Reifenmontiermaschine",
            "Cloud-Cluster Nodes",      "Spritzgussmaschine",        "Desktop-Workstations",
            "Ultraschallgerät",         "Kleintransporter 8er-Pack",
        };

        // Spread creation dates across the last 18 months
        var baseDate = new DateTime(DateTime.UtcNow.Year - 1, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        int counter = 1;
        for (int i = 0; i < 100; i++)
        {
            var status   = statusPool[i];
            var typ      = typPool[i % typPool.Length];
            var lg       = gesellschaften[i % gesellschaften.Count];
            var obligo   = obligos[i % obligos.Length];
            var name     = antragNamen[i % antragNamen.Length];
            var erstelltAm = baseDate.AddDays(i * 5).AddHours(i % 24);   // spread over ~18 months
            var geaendertAm = erstelltAm.AddDays((i % 14) + 1);

            var antrag = new Leasingantrag
            {
                AntragNummer       = $"LNW-{erstelltAm.Year}-{counter:D5}",
                AntragTyp          = typ,
                Status             = status,
                Obligo             = obligo,
                Abrechnungsart     = (i % 3) switch { 0 => "Monatlich", 1 => "Quartalsweise", _ => "Jährlich" },
                LeasinggesellschaftId = lg.Id,
                EingereichtVonId   = einreicher.Id,
                SachbearbeiterMBId = status != AntragStatus.Entwurf ? sbMB.Id : null,
                GenehmigerMBId     = status == AntragStatus.Genehmigt ? genehm.Id : null,
                Archiviert         = status == AntragStatus.Archiviert,
                ArchiviertAm       = status == AntragStatus.Archiviert ? geaendertAm : null,
                AblehnungsgrundId  = status == AntragStatus.Abgelehnt ? ablehnGrund?.Id : null,
                AblehnungsKommentar= status == AntragStatus.Abgelehnt ? "Automatisch generierter Demo-Kommentar." : null,
                AbgelehntAm        = status == AntragStatus.Abgelehnt ? geaendertAm : null,
                ZweiteVoteErforderlich = status == AntragStatus.ZweiteVoteErforderlich,
                ErstelltAm         = erstelltAm,
                GeaendertAm        = geaendertAm,
                ErstelltVonId      = einreicher.Id,
                GeaendertVonId     = einreicher.Id
            };

            db.Leasingantraege.Add(antrag);

            // Add one Leasingobjekt per Antrag
            db.Leasingobjekte.Add(new Leasingobjekt
            {
                Leasingantrag    = antrag,
                Bezeichnung      = name,
                IstNeu           = (i % 4) != 0,
                Listenpreis      = obligo * 1.1m,
                Rabatt           = obligo * 0.05m,
                FinanzierungsBasis = obligo,
                NAK              = obligo,
                Hersteller       = (i % 5) switch { 0 => "Siemens", 1 => "BMW", 2 => "Dell", 3 => "Trumpf", _ => "Linde" },
                ErstelltAm       = erstelltAm,
                GeaendertAm      = erstelltAm
            });

            counter++;
        }

        await db.SaveChangesAsync();

        // ── Verträge für genehmigte Anträge ────────────────────────────────────
        var genehmigteAntraege = await db.Leasingantraege
            .Where(a => a.Status == AntragStatus.Genehmigt)
            .ToListAsync();

        var vertragstypen = await db.Vertragstypen.ToListAsync();
        int vtgCounter = 1;

        foreach (var (antrag, idx) in genehmigteAntraege.Select((a, i) => (a, i)))
        {
            var laufzeit   = (idx % 3 + 2) * 12;   // 24, 36, or 48 months
            var beginn     = antrag.GeaendertAm.AddDays(14);
            var vtStatus   = idx < 5  ? VertragStatus.Beendet :
                             idx < 10 ? VertragStatus.Gekuendigt :
                                        VertragStatus.Aktiv;
            var vt         = vertragstypen[idx % vertragstypen.Count];

            db.Vertraege.Add(new Vertrag
            {
                VertragNummer      = $"VTG-{antrag.ErstelltAm.Year}-{vtgCounter:D5}",
                Status             = vtStatus,
                LeasingantragId    = antrag.Id,
                VertragtypId       = vt.Id,
                Vertragsbeginn     = beginn,
                LaufzeitMonate     = laufzeit,
                Vertragsende       = beginn.AddMonths(laufzeit),
                Finanzierungsbetrag= antrag.Obligo,
                MonatlicheRate     = Math.Round(antrag.Obligo / laufzeit, 2),
                Zinssatz           = 2.5m + (idx % 5) * 0.25m,
                ErstelltAm         = antrag.GeaendertAm,
                GeaendertAm        = antrag.GeaendertAm,
                ErstelltVonId      = einreicher.Id,
                GeaendertVonId     = einreicher.Id
            });
            vtgCounter++;
        }

        await db.SaveChangesAsync();
    }

    private static readonly DateTime Ts = DateTime.UtcNow;
}
