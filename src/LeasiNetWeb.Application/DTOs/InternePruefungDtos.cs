namespace LeasiNetWeb.Application.DTOs;

public record InternePruefungListeDto(
    int Id,
    int LeasingantragId,
    string AntragNummer,
    string PrueferMB,
    string? PruefungNummer,
    bool Abgeschlossen,
    DateTime? AbgeschlossenAm,
    int AnzahlPflichten,
    int AnzahlErfuellt,
    int AnzahlSchritte,
    int AnzahlSchritteAbgeschlossen
);

public record InternePruefungDto(
    int Id,
    int LeasingantragId,
    string AntragNummer,
    string? Leasinggesellschaft,
    decimal Obligo,
    string PrueferMB,
    int PrueferMBId,
    string? PruefungNummer,
    bool Abgeschlossen,
    DateTime? AbgeschlossenAm,
    string? Ergebnis,
    IEnumerable<PruefungsPflichtDto> Pflichten,
    IEnumerable<PruefungsSchrittDto> Schritte,
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

public record PruefungsSchrittDto(
    int Id,
    int InternePruefungId,
    int Sortierung,
    string Bezeichnung,
    string PrueferName,
    int PrueferMBId,
    bool Abgeschlossen,
    DateTime? AbgeschlossenAm,
    string? Ergebnis
);

/// <summary>Eingabe-ViewModel für den Wizard: Prüfung starten.</summary>
public record PruefungStartenInput(
    int AntragId,
    List<PruefungsSchrittInput> Schritte
);

public record PruefungsSchrittInput(
    int PrueferMBId,
    string Bezeichnung
);

/// <summary>Daten die der Wizard initial braucht (per JSON aus Controller).</summary>
public record PruefungWizardDatenDto(
    int AntragId,
    string AntragNummer,
    string? Leasinggesellschaft,
    decimal Obligo,
    List<PrueferOptionDto> VerfuegbarePruefer
);

public record PrueferOptionDto(
    int Id,
    string Name,
    string Rolle
);
