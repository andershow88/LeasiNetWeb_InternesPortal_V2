namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Tracks sync jobs to/from external systems (KNE, KDM, etc).
/// Maps to bm_lnw_syncrequest.
/// </summary>
public class SynchronisierungsAnfrage : BaseEntity
{
    public string Quelle { get; set; } = string.Empty;    // e.g. "KNE", "KDM"
    public string Aktion { get; set; } = string.Empty;
    public string? Nutzlast { get; set; }                  // JSON payload
    public bool Verarbeitet { get; set; }
    public DateTime? VerarbeitetAm { get; set; }
    public string? Fehler { get; set; }
    public int Versuche { get; set; }
}
