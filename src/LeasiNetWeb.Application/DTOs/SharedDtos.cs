using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Application.DTOs;

public record LeasingobjektDto(
    int Id,
    string Bezeichnung,
    bool IstNeu,
    decimal Listenpreis,
    decimal? Rabatt,
    decimal FinanzierungsBasis,
    decimal? NAK,
    string? Hersteller,
    string? Lieferant,
    string? Geraetetyp
);

public record KommentarDto(
    int Id,
    string Autor,
    string Text,
    bool IstIntern,
    DateTime ErstelltAm,
    IEnumerable<AnhangDto> Anhaenge
);

public record KommentarErstellenDto(
    int LeasingantragId,
    string Text,
    bool IstIntern
);

public record AnhangDto(
    int Id,
    string Dateiname,
    AnhangTyp Typ,
    long DateigroesseBytes,
    string HochgeladenVon,
    DateTime ErstelltAm
);

public record EreignisDto(
    int Id,
    EreignisTyp Typ,
    string AusgeloestVon,
    string? Beschreibung,
    DateTime ErstelltAm
);

public record NachrichtDto(
    int Id,
    string Absender,
    string Betreff,
    string Text,
    bool Gelesen,
    DateTime? GelesenAm,
    DateTime ErstelltAm,
    int? LeasingantragId,
    string? AntragNummer
);

public record NachrichtSendenDto(
    int EmpfaengerId,
    int? LeasingantragId,
    string Betreff,
    string Text
);
