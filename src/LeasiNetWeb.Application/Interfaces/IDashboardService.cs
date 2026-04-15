using LeasiNetWeb.Application.DTOs;

namespace LeasiNetWeb.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardDaten(int benutzerId);
}
