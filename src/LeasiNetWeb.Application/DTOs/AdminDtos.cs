using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Application.DTOs;

// ── Benutzer ──────────────────────────────────────────────────────────────────

public record BenutzerListeDto(
    int Id,
    string Benutzername,
    string Anzeigename,
    string EMail,
    BenutzerRolle Rolle,
    bool IstAktiv,
    string? Leasinggesellschaft,
    DateTime ErstelltAm
);

public record BenutzerDetailDto(
    int Id,
    string Benutzername,
    string Vorname,
    string Nachname,
    string EMail,
    BenutzerRolle Rolle,
    bool IstAktiv,
    int? LeasinggesellschaftId
);

public record BenutzerErstellenDto(
    string Benutzername,
    string Vorname,
    string Nachname,
    string EMail,
    BenutzerRolle Rolle,
    bool IstAktiv,
    int? LeasinggesellschaftId,
    string Passwort
);

public record BenutzerBearbeitenDto(
    int Id,
    string Benutzername,
    string Vorname,
    string Nachname,
    string EMail,
    BenutzerRolle Rolle,
    bool IstAktiv,
    int? LeasinggesellschaftId,
    string? NeuesPasswort   // null = kein Passwortwechsel
);

// ── Leasinggesellschaft ───────────────────────────────────────────────────────

public record LeasinggesellschaftListeDto(
    int Id,
    string Name,
    string? Kurzbezeichnung,
    string? EMail,
    string? Telefon,
    decimal ObligoLimit,
    bool IstAktiv,
    int AnzahlBenutzer
);

public record LeasinggesellschaftDetailDto(
    int Id,
    string Name,
    string? Kurzbezeichnung,
    string? Strasse,
    string? PLZ,
    string? Ort,
    string? Land,
    string? Telefon,
    string? EMail,
    string? Ansprechpartner,
    decimal ObligoLimit,
    bool IstAktiv
);

// ── Ablehnungsgrund ───────────────────────────────────────────────────────────

public record AblehnungsgrundDto(
    int Id,
    string Code,
    string Bezeichnung,
    bool IstAktiv
);

// ── Gerätetyp ─────────────────────────────────────────────────────────────────

public record GeraetetypDto(
    int Id,
    string Bezeichnung,
    string? Beschreibung,
    bool IstAktiv,
    int? ElterntypId,
    string? Elterntyp
);
