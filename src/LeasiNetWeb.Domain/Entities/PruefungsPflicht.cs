namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Individual compliance obligation within an internal audit check.
/// Maps to bm_lnw_ic_obligations.
/// </summary>
public class PruefungsPflicht : BaseEntity
{
    public int InternePruefungId { get; set; }
    public InternePruefung InternePruefung { get; set; } = null!;

    public string Bezeichnung { get; set; } = string.Empty;
    public string? Beschreibung { get; set; }
    public bool Erfuellt { get; set; }
    public DateTime? ErfuelltAm { get; set; }
    public string? Bemerkungen { get; set; }
    public int Sortierung { get; set; }
}
