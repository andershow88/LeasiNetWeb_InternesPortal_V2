namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Leasing company partner onboarding registration. Maps to bm_lnw_lgregistration.
/// </summary>
public class LgRegistrierung : BaseEntity
{
    public int LeasinggesellschaftId { get; set; }
    public Leasinggesellschaft Leasinggesellschaft { get; set; } = null!;

    public string Kontaktperson { get; set; } = string.Empty;
    public string EMail { get; set; } = string.Empty;
    public string? Telefon { get; set; }
    public bool Abgeschlossen { get; set; }
    public DateTime? AbgeschlossenAm { get; set; }
    public string? Bemerkungen { get; set; }
}
