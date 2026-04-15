namespace LeasiNetWeb.Domain.Enums;

/// <summary>
/// Lifecycle-Status eines Leasingantrags.
/// Replaces the manual B_EDITED_AND_CHECKDONE flag from Intrexx.
/// </summary>
public enum AntragStatus
{
    Entwurf = 0,
    Eingereicht = 1,
    InPruefung = 2,
    BeiMitarbeiter = 3,
    BeiLeasinggesellschaft = 4,
    ZweiteVoteErforderlich = 5,
    InterneKontrolleErforderlich = 6,
    Genehmigt = 7,
    Abgelehnt = 8,
    Archiviert = 9,
    Storniert = 10
}
