using LeasiNetWeb.Application.DTOs;

namespace LeasiNetWeb.Application.Interfaces;

public interface IInternePruefungService
{
    /// <summary>
    /// Startet eine neue interne Prüfung mit mehreren sequentiellen Prüfschritten.
    /// Generiert eine eindeutige Prüfnummer und setzt den Antragsstatus auf InterneKontrolleErforderlich.
    /// </summary>
    Task<int> StartenAsync(int antragId, int hauptPrueferMBId, List<PruefungsSchrittInput> schritte);

    Task<InternePruefungDto?> GetByAntragIdAsync(int antragId);
    Task<InternePruefungDto?> GetByIdAsync(int id);

    Task<IEnumerable<InternePruefungListeDto>> GetMeinePruefungenAsync(int prueferMBId);
    Task<IEnumerable<InternePruefungListeDto>> GetAllePruefungenAsync();

    /// <summary>Lädt Antragsdaten + verfügbare Prüfer für den Wizard.</summary>
    Task<PruefungWizardDatenDto?> GetWizardDatenAsync(int antragId);

    Task PflichtErfuellenAsync(int pflichtId, string? bemerkungen, int benutzerId);
    Task PflichtRueckgaengigAsync(int pflichtId);

    /// <summary>Schließt einen einzelnen Workflow-Schritt ab und aktiviert den nächsten.</summary>
    Task SchrittAbschliessenAsync(int schrittId, int benutzerId, string? ergebnis);

    /// <summary>Schließt die gesamte Prüfung ab und setzt den Antragsstatus zurück.</summary>
    Task AbschliessenAsync(int id, int benutzerId, string? ergebnis);
}
