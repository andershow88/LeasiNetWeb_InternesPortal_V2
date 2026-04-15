using LeasiNetWeb.Application.DTOs;

namespace LeasiNetWeb.Application.Interfaces;

public interface IInternePruefungService
{
    /// <summary>Creates a new InternePruefung for the antrag and transitions it to InterneKontrolleErforderlich.</summary>
    Task<int> StartenAsync(int antragId, int prueferMBId);

    Task<InternePruefungDto?> GetByAntragIdAsync(int antragId);
    Task<InternePruefungDto?> GetByIdAsync(int id);

    Task<IEnumerable<InternePruefungListeDto>> GetMeinePruefungenAsync(int prueferMBId);
    Task<IEnumerable<InternePruefungListeDto>> GetAllePruefungenAsync();

    Task PflichtErfuellenAsync(int pflichtId, string? bemerkungen);
    Task PflichtRueckgaengigAsync(int pflichtId);

    /// <summary>Completes the check and transitions the antrag back to BeiMitarbeiter.</summary>
    Task AbschliessenAsync(int id, int benutzerId, string? ergebnis);
}
