using LeasiNetWeb.Application.DTOs;

namespace LeasiNetWeb.Application.Interfaces;

public interface IAuswertungService
{
    Task<AuswertungDto> GetAuswertungAsync(int jahr);
    Task<IEnumerable<AntragListeDto>> GetAntraegeFuerExportAsync(int? jahr = null);
    Task<IEnumerable<VertragListeDto>> GetVertraegeFuerExportAsync();
}
