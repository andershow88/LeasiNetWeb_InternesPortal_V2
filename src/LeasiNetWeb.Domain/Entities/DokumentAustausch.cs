namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// External document exchange record. Maps to bm_lnw_documentexchange.
/// </summary>
public class DokumentAustausch : BaseEntity
{
    public int? VertragId { get; set; }
    public Vertrag? Vertrag { get; set; }

    public int? LeasingantragId { get; set; }
    public Leasingantrag? Leasingantrag { get; set; }

    public string Dateiname { get; set; } = string.Empty;
    public string Dateipfad { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long DateigroesseBytes { get; set; }
    public bool Gelesen { get; set; }
    public DateTime? GelesenAm { get; set; }
    public string? Richtung { get; set; }     // "eingehend" / "ausgehend"
}
