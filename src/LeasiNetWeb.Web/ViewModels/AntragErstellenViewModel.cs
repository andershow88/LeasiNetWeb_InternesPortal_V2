using System.ComponentModel.DataAnnotations;
using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Web.ViewModels;

public class AntragErstellenViewModel
{
    [Required]
    [Display(Name = "Antragstyp")]
    public AntragTyp AntragTyp { get; set; }

    [Display(Name = "Leasinggesellschaft")]
    public int? LeasinggesellschaftId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Obligo muss größer als 0 sein.")]
    [Display(Name = "Obligo (EUR)")]
    public decimal Obligo { get; set; }

    [Display(Name = "Abrechnungsart")]
    [StringLength(100)]
    public string? Abrechnungsart { get; set; }

    // KI-Erstellung
    public bool KiErstellt { get; set; }
    public string? PdfTempToken { get; set; }
    public string? PdfDateiname { get; set; }
}
