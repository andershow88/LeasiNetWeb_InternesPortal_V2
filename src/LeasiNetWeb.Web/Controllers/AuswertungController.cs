using System.Text;
using LeasiNetWeb.Application.Interfaces;
using LeasiNetWeb.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace LeasiNetWeb.Web.Controllers;

public class AuswertungController : BaseController
{
    private readonly IAuswertungService _auswertung;

    public AuswertungController(IAuswertungService auswertung) => _auswertung = auswertung;

    public async Task<IActionResult> Index(int? jahr = null)
    {
        var j = jahr ?? DateTime.UtcNow.Year;
        var dto = await _auswertung.GetAuswertungAsync(j);
        ViewBag.VerfuegbareJahre = Enumerable.Range(DateTime.UtcNow.Year - 4, 5).Reverse().ToList();
        return View(dto);
    }

    // ── CSV Exports ────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> AntraegeCsv(int? jahr = null)
    {
        var daten = await _auswertung.GetAntraegeFuerExportAsync(jahr);

        var sb = new StringBuilder();
        sb.AppendLine("Id;AntragNummer;Typ;Status;EingereichtVon;Leasinggesellschaft;SachbearbeiterMB;Obligo;ErstelltAm;GeaendertAm");

        foreach (var a in daten)
        {
            sb.AppendLine(string.Join(";",
                a.Id,
                a.AntragNummer,
                a.AntragTyp,
                a.Status,
                CsvEscape(a.EingereichtVon),
                CsvEscape(a.Leasinggesellschaft ?? ""),
                CsvEscape(a.SachbearbeiterMB ?? ""),
                a.Obligo.ToString("F2"),
                a.ErstelltAm.ToString("yyyy-MM-dd"),
                a.GeaendertAm.ToString("yyyy-MM-dd")
            ));
        }

        var dateiname = jahr.HasValue
            ? $"antraege_{jahr}.csv"
            : "antraege_alle.csv";

        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray(),
            "text/csv", dateiname);
    }

    [HttpGet]
    public async Task<IActionResult> VertraegeCsv()
    {
        var daten = await _auswertung.GetVertraegeFuerExportAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Id;VertragNummer;Status;AntragNummer;Leasinggesellschaft;Vertragstyp;Finanzierungsbetrag;Vertragsbeginn;Vertragsende;LaufzeitMonate");

        foreach (var v in daten)
        {
            sb.AppendLine(string.Join(";",
                v.Id,
                v.VertragNummer,
                v.Status,
                v.AntragNummer,
                CsvEscape(v.Leasinggesellschaft ?? ""),
                CsvEscape(v.Vertragstyp ?? ""),
                v.Finanzierungsbetrag.ToString("F2"),
                v.Vertragsbeginn?.ToString("yyyy-MM-dd") ?? "",
                v.Vertragsende?.ToString("yyyy-MM-dd") ?? "",
                v.LaufzeitMonate?.ToString() ?? ""
            ));
        }

        return File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(sb.ToString())).ToArray(),
            "text/csv", "vertraege.csv");
    }

    private static string CsvEscape(string value)
        => value.Contains(';') || value.Contains('"') || value.Contains('\n')
            ? $"\"{value.Replace("\"", "\"\"")}\""
            : value;
}
