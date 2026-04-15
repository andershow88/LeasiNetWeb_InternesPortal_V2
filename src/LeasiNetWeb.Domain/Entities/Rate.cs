namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Pricing rate entry. Maps to bm_lnw_rates.
/// </summary>
public class Rate : BaseEntity
{
    public int RatentabelleId { get; set; }
    public Ratentabelle Ratentabelle { get; set; } = null!;

    public decimal LaufzeitMonate { get; set; }
    public decimal? Restwertprozent { get; set; }
    public decimal Faktor { get; set; }
    public bool IstAktiv { get; set; } = true;
}
