namespace LeasiNetWeb.Application.DTOs;

public record InternePruefungListeDto(
    int Id,
    int LeasingantragId,
    string AntragNummer,
    string PrueferMB,
    bool Abgeschlossen,
    DateTime? AbgeschlossenAm,
    int AnzahlPflichten,
    int AnzahlErfuellt
);

public record InternePruefungDto(
    int Id,
    int LeasingantragId,
    string AntragNummer,
    string PrueferMB,
    int PrueferMBId,
    bool Abgeschlossen,
    DateTime? AbgeschlossenAm,
    string? Ergebnis,
    IEnumerable<PruefungsPflichtDto> Pflichten,
    IEnumerable<AnhangDto> Anhaenge
);

public record PruefungsPflichtDto(
    int Id,
    int InternePruefungId,
    string Bezeichnung,
    string? Beschreibung,
    bool Erfuellt,
    DateTime? ErfuelltAm,
    string? Bemerkungen,
    int Sortierung
);
