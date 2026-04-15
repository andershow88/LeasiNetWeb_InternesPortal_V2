namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Obligation/liability tracking per application and leasing company.
/// Maps to bm_lnw_la_lg_obligo.
/// </summary>
public class Obligo : BaseEntity
{
    public int LeasingantragId { get; set; }
    public Leasingantrag Leasingantrag { get; set; } = null!;

    public int LeasinggesellschaftId { get; set; }
    public Leasinggesellschaft Leasinggesellschaft { get; set; } = null!;

    public decimal Betrag { get; set; }
    public string? Status { get; set; }
    public DateTime? GueltigAb { get; set; }
    public DateTime? GueltigBis { get; set; }
}
