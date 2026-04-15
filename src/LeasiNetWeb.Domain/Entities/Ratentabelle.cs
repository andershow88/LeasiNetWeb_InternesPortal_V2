namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Rate tier / rate table. Maps to bm_lnw_raterotas.
/// </summary>
public class Ratentabelle : BaseEntity
{
    public string Bezeichnung { get; set; } = string.Empty;
    public string? Beschreibung { get; set; }
    public bool IstAktiv { get; set; } = true;
    public DateTime GueltigAb { get; set; }
    public DateTime? GueltigBis { get; set; }

    public ICollection<Rate> Raten { get; set; } = new List<Rate>();
}
