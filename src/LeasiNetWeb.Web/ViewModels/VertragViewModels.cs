using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LeasiNetWeb.Web.ViewModels;

public class VertragBearbeitenViewModel
{
    public int Id { get; set; }
    public string VertragNummer { get; set; } = string.Empty;
    public int LeasingantragId { get; set; }
    public string AntragNummer { get; set; } = string.Empty;

    [Display(Name = "Vertragstyp")]
    public int? VertragtypId { get; set; }

    [Display(Name = "Vertragsbeginn")]
    [DataType(DataType.Date)]
    public DateTime? Vertragsbeginn { get; set; }

    [Display(Name = "Vertragsende")]
    [DataType(DataType.Date)]
    public DateTime? Vertragsende { get; set; }

    [Display(Name = "Laufzeit (Monate)")]
    [Range(1, 360)]
    public int? LaufzeitMonate { get; set; }

    [Display(Name = "Finanzierungsbetrag (€)")]
    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Finanzierungsbetrag muss größer als 0 sein.")]
    public decimal Finanzierungsbetrag { get; set; }

    [Display(Name = "Restwert (€)")]
    public decimal? Restwert { get; set; }

    [Display(Name = "Monatliche Rate (€)")]
    public decimal? MonatlicheRate { get; set; }

    [Display(Name = "Zinssatz (%)")]
    [Range(0, 100)]
    public decimal? Zinssatz { get; set; }

    public IEnumerable<SelectListItem> Vertragstypen { get; set; } = [];
}
