using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Application.Interfaces;

public interface ILeasingantragService
{
    Task<IEnumerable<AntragListeDto>> GetAlleAntraege(int? benutzerId = null, AntragStatus? status = null);
    Task<AntragDetailDto?> GetAntragDetail(int id);
    Task<int> ErstelleAntrag(AntragErstellenDto dto, int benutzerId);
    Task<int> ErstelleKiAntrag(AntragErstellenDto dto, int benutzerId);
    Task AktualisiereAntrag(int id, AntragAktualisierenDto dto, int benutzerId);
    Task<bool> StatusWechsel(int antragId, AntragStatus neuerStatus, int benutzerId, string? kommentar = null);
    Task<bool> Genehmigen(int antragId, int genehmigerMBId);
    Task<bool> Ablehnen(int antragId, int genehmigerMBId, int ablehnungsgrundId, string? kommentar);
    Task<bool> ZweiteVoteAnfordern(int antragId, int genehmigerMBId);
    Task Archivieren(int antragId);
    Task<IEnumerable<AntragListeDto>> GetMeineZuPruefendenAntraege(int benutzerId);
    Task<IEnumerable<AntragListeDto>> GetMeineZuGenehmigendenAntraege(int benutzerId);
    Task<IEnumerable<AntragListeDto>> GetMeineZweiteVotenAntraege(int benutzerId);
}
