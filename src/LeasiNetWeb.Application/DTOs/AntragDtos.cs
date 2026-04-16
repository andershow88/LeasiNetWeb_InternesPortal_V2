using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Application.DTOs;

public record AntragListeDto(
    int Id,
    string AntragNummer,
    AntragTyp AntragTyp,
    AntragStatus Status,
    string EingereichtVon,
    string? Leasinggesellschaft,
    string? SachbearbeiterMB,
    decimal Obligo,
    DateTime ErstelltAm,
    DateTime GeaendertAm,
    bool ZweiteVoteErforderlich,
    bool KiErstellt
);

public record AntragDetailDto(
    int Id,
    string AntragNummer,
    AntragTyp AntragTyp,
    AntragStatus Status,
    decimal Obligo,
    string? Abrechnungsart,
    string EingereichtVon,
    string? Leasinggesellschaft,
    int? LeasinggesellschaftId,
    string? SachbearbeiterMB,
    int? SachbearbeiterMBId,
    string? SachbearbeiterLG,
    int? SachbearbeiterLGId,
    string? Ablehnungsgrund,
    string? AblehnungsKommentar,
    DateTime? AbgelehntAm,
    bool ZweiteVoteErforderlich,
    bool Archiviert,
    DateTime ErstelltAm,
    DateTime GeaendertAm,
    bool KiErstellt,
    IEnumerable<LeasingobjektDto> Objekte,
    IEnumerable<KommentarDto> Kommentare,
    IEnumerable<AnhangDto> Anhaenge,
    IEnumerable<EreignisDto> Ereignisse
);

public record AntragErstellenDto(
    AntragTyp AntragTyp,
    int? LeasinggesellschaftId,
    decimal Obligo,
    string? Abrechnungsart,
    bool KiErstellt = false
);

public record AntragAktualisierenDto(
    AntragTyp? AntragTyp,
    int? LeasinggesellschaftId,
    int? SachbearbeiterMBId,
    int? SachbearbeiterLGId,
    decimal? Obligo,
    string? Abrechnungsart
);
