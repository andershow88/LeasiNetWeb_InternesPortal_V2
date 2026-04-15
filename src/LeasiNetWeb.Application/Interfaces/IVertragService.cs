using LeasiNetWeb.Application.DTOs;
using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Application.Interfaces;

public interface IVertragService
{
    Task<int> ErstellenAsync(int antragId, int benutzerId);

    Task<IEnumerable<VertragListeDto>> GetAlleVertraege(VertragStatus? status = null);
    Task<VertragDetailDto?> GetVertragDetail(int id);
    Task<VertragDetailDto?> GetVertragByAntragId(int antragId);

    Task AktualisiereVertrag(int id, VertragAktualisierenDto dto, int benutzerId);
    Task StatusWechsel(int id, VertragStatus neuerStatus, int benutzerId);
}
