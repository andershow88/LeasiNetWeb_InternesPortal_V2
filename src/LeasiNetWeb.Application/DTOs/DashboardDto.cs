using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Application.DTOs;

public record DashboardDto(
    int OffeneAntraege,
    int AntraegeInBearbeitung,
    int ZuPruefendeAntraege,
    int ZuGenehmigendeAntraege,
    int PendingZweiteVoten,
    int AktiveVertraege,
    int UngeleseneNachrichten,
    decimal GesamtObligoAktiv,
    IEnumerable<AntragListeDto> MeineAktuellenAntraege,
    IEnumerable<StatusZaehlerDto> AntraegePorStatus
);

public record StatusZaehlerDto(AntragStatus Status, int Anzahl);
