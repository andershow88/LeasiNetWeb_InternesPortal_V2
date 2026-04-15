namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Equipment/asset type classification. Maps to bm_lnw_assets + bm_lnw_assettypes.
/// Consolidated into a single type with optional parent category.
/// </summary>
public class Geraetetyp : BaseEntity
{
    public string Bezeichnung { get; set; } = string.Empty;
    public string? Beschreibung { get; set; }
    public bool IstAktiv { get; set; } = true;

    // Parent category (top-level types have null)
    public int? ElterntypId { get; set; }
    public Geraetetyp? Elterntyp { get; set; }
    public ICollection<Geraetetyp> Untertypen { get; set; } = [];

    public ICollection<Leasingobjekt> Objekte { get; set; } = [];
}
