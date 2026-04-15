using LeasiNetWeb.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LeasiNetWeb.Web.Controllers;

public class DashboardController : BaseController
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard) => _dashboard = dashboard;

    public async Task<IActionResult> Index()
    {
        var daten = await _dashboard.GetDashboardDaten(AktuellerBenutzerId);
        return View(daten);
    }
}
