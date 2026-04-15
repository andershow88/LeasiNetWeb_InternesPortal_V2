namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Manageable help content per page/section. Maps to bm_lnw_help.
/// </summary>
public class HilfeText : BaseEntity
{
    public string Schluessel { get; set; } = string.Empty;   // page/section identifier
    public string Titel { get; set; } = string.Empty;
    public string Inhalt { get; set; } = string.Empty;       // HTML allowed
    public bool IstAktiv { get; set; } = true;
}
