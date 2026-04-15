using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Application.DTOs;

public record VertragListeDto(
    int Id,
    string VertragNummer,
    VertragStatus Status,
    int LeasingantragId,
    string AntragNummer,
    string? Leasinggesellschaft,
    string? Vertragstyp,
    decimal Finanzierungsbetrag,
    DateTime? Vertragsbeginn,
    DateTime? Vertragsende,
    int? LaufzeitMonate
);

public record VertragDetailDto(
    int Id,
    string VertragNummer,
    VertragStatus Status,
    int LeasingantragId,
    string AntragNummer,
    string? Leasinggesellschaft,
    string EingereichtVon,
    int? VertragtypId,
    string? Vertragstyp,
    DateTime? Vertragsbeginn,
    DateTime? Vertragsende,
    int? LaufzeitMonate,
    decimal Finanzierungsbetrag,
    decimal? Restwert,
    decimal? MonatlicheRate,
    decimal? Zinssatz,
    DateTime ErstelltAm,
    DateTime GeaendertAm,
    IEnumerable<AnhangDto> Anhaenge,
    IEnumerable<LeasingobjektDto> Objekte
);

public record VertragAktualisierenDto(
    int? VertragtypId,
    DateTime? Vertragsbeginn,
    DateTime? Vertragsende,
    int? LaufzeitMonate,
    decimal Finanzierungsbetrag,
    decimal? Restwert,
    decimal? MonatlicheRate,
    decimal? Zinssatz
);
