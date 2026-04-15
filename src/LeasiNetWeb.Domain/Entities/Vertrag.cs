using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Active leasing contract. Maps to bm_lnw_cm_contracts.
/// </summary>
public class Vertrag : BaseEntity
{
    public string VertragNummer { get; set; } = string.Empty;
    public VertragStatus Status { get; set; } = VertragStatus.InVorbereitung;

    public int LeasingantragId { get; set; }
    public Leasingantrag Leasingantrag { get; set; } = null!;

    public int? VertragtypId { get; set; }
    public Vertragstyp? Vertragstyp { get; set; }

    public DateTime? Vertragsbeginn { get; set; }
    public DateTime? Vertragsende { get; set; }
    public int? LaufzeitMonate { get; set; }

    public decimal Finanzierungsbetrag { get; set; }
    public decimal? Restwert { get; set; }
    public decimal? MonatlicheRate { get; set; }
    public decimal? Zinssatz { get; set; }

    public ICollection<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    public ICollection<DokumentAustausch> DokumentAustausche { get; set; } = new List<DokumentAustausch>();
}
