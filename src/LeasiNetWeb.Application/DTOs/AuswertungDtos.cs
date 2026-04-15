using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Application.DTOs;

public record AuswertungDto(
    int Jahr,
    // Anträge
    int AntraegeGesamt,
    int AntraegeGenehmigt,
    int AntraegeAbgelehnt,
    int AntraegeOffen,
    decimal GesamtObligoAktiv,
    IEnumerable<StatusZaehlerDto> AntraegeNachStatus,
    IEnumerable<MonatsZaehlerDto> AntraegeProMonat,
    IEnumerable<AntragTypZaehlerDto> AntraegeNachTyp,
    IEnumerable<LgObligoDto> ObligoProLg,
    // Verträge
    int VertraegeGesamt,
    int VertraegeAktiv,
    decimal GesamtFinanzierungsvolumen,
    decimal? DurchschnittlicheMonatlicheRate,
    IEnumerable<VertragStatusZaehlerDto> VertraegeNachStatus,
    // Benutzer
    IEnumerable<BenutzerAktivitaetDto> BenutzerAktivitaet
);

public record MonatsZaehlerDto(
    int Jahr,
    int Monat,
    string MonatLabel,
    int AnzahlAntraege,
    int AnzahlGenehmigt,
    int AnzahlAbgelehnt,
    decimal ObligoSumme
);

public record LgObligoDto(
    string Name,
    decimal ObligoSumme,
    decimal ObligoLimit,
    int AnzahlAntraege,
    int AnzahlAktiveVertraege
);

public record AntragTypZaehlerDto(string Typ, int Anzahl, decimal ObligoSumme);

public record VertragStatusZaehlerDto(VertragStatus Status, int Anzahl, decimal VolumenSumme);

public record BenutzerAktivitaetDto(
    string Anzeigename,
    string Rolle,
    int EingereichtAntraege,
    int BearbeiteteAntraege
);
