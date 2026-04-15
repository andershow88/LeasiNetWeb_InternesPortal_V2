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
        // EnsureCreated works for SQLite; for PostgreSQL on Railway we also run it
        // (no migrations needed for MVP — schema is created fresh on first deploy)
        await db.Database.EnsureCreatedAsync();

        if (await db.Benutzer.AnyAsync()) return;

        // ── Ablehnungsgründe ──────────────────────────────────────────────────
        var ablehnungsgruende = new List<Ablehnungsgrund>
        {
            new() { Code = "BONITÄT", Bezeichnung = "Unzureichende Bonität", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow },
            new() { Code = "UNTERLAGEN", Bezeichnung = "Unvollständige Unterlagen", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow },
            new() { Code = "OBLIGO", Bezeichnung = "Obligo-Limit überschritten", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow },
            new() { Code = "SONSTIGES", Bezeichnung = "Sonstiger Grund", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow }
        };
        db.Ablehnungsgruende.AddRange(ablehnungsgruende);

        // ── Vertragstypen ─────────────────────────────────────────────────────
        var vertragstypen = new List<Vertragstyp>
        {
            new() { Code = "KFZ", Bezeichnung = "KFZ-Leasing", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow },
            new() { Code = "MASCHINEN", Bezeichnung = "Maschinenleasing", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow },
            new() { Code = "IT", Bezeichnung = "IT-Leasing", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow },
            new() { Code = "IMMOBILIEN", Bezeichnung = "Immobilienleasing", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow }
        };
        db.Vertragstypen.AddRange(vertragstypen);

        // ── Gerätetypen ────────────────────────────────────────────────────────
        var geraetetypen = new List<Geraetetyp>
        {
            new() { Bezeichnung = "Fahrzeuge", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow },
            new() { Bezeichnung = "Maschinen & Anlagen", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow },
            new() { Bezeichnung = "IT-Ausstattung", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow },
            new() { Bezeichnung = "Medizintechnik", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow }
        };
        db.Geraetetypen.AddRange(geraetetypen);

        // ── Leasinggesellschaften ─────────────────────────────────────────────
        var lg1 = new Leasinggesellschaft
        {
            Name = "Musterbank AG", Kurzbezeichnung = "MB-AG", Land = "DE",
            EMail = "kontakt@musterbank.de", ObligoLimit = 5_000_000,
            IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow
        };
        var lg2 = new Leasinggesellschaft
        {
            Name = "Demo Leasing GmbH", Kurzbezeichnung = "DL-GmbH", Land = "DE",
            EMail = "info@demoleasing.de", ObligoLimit = 2_500_000,
            IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow
        };
        db.Leasinggesellschaften.AddRange(lg1, lg2);

        // ── Benutzer ──────────────────────────────────────────────────────────
        static string Hash(string pw) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(pw))).ToLower();

        var admin = new Benutzer
        {
            Benutzername = "admin", PasswortHash = Hash("Admin1234!"),
            Vorname = "System", Nachname = "Administrator",
            EMail = "admin@leasinetweb.de", Rolle = BenutzerRolle.Administrator,
            IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow
        };
        var sachbearbeiter = new Benutzer
        {
            Benutzername = "sachbearbeiter.mb", PasswortHash = Hash("Demo1234!"),
            Vorname = "Max", Nachname = "Mustermann",
            EMail = "max.mustermann@leasinetweb.de", Rolle = BenutzerRolle.SachbearbeiterMB,
            IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow
        };
        var genehmiger = new Benutzer
        {
            Benutzername = "genehmiger.mb", PasswortHash = Hash("Demo1234!"),
            Vorname = "Anna", Nachname = "Musterfrau",
            EMail = "anna.musterfrau@leasinetweb.de", Rolle = BenutzerRolle.Genehmiger,
            IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow
        };
        var lgUser = new Benutzer
        {
            Benutzername = "sachbearbeiter.lg", PasswortHash = Hash("Demo1234!"),
            Vorname = "Klaus", Nachname = "Beispiel",
            EMail = "k.beispiel@musterbank.de", Rolle = BenutzerRolle.SachbearbeiterLG,
            IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow,
            Leasinggesellschaft = lg1
        };
        db.Benutzer.AddRange(admin, sachbearbeiter, genehmiger, lgUser);

        // ── Hilfe-Texte ────────────────────────────────────────────────────────
        db.HilfeTexte.AddRange(
            new HilfeText { Schluessel = "dashboard", Titel = "Dashboard", Inhalt = "Das Dashboard zeigt Ihre wichtigsten Kennzahlen auf einen Blick.", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow },
            new HilfeText { Schluessel = "antraege", Titel = "Leasinganträge", Inhalt = "Hier finden Sie alle Leasinganträge. Nutzen Sie Filter und Suche zur schnellen Navigation.", IstAktiv = true, ErstelltAm = DateTime.UtcNow, GeaendertAm = DateTime.UtcNow }
        );

        await db.SaveChangesAsync();
    }
}
