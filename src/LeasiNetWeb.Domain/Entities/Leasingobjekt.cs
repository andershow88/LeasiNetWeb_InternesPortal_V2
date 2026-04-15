namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Asset/equipment associated with a leasing application. Maps to bm_lnw_object.
/// </summary>
public class Leasingobjekt : BaseEntity
{
    public int LeasingantragId { get; set; }
    public Leasingantrag Leasingantrag { get; set; } = null!;

    public int? GeraetetypId { get; set; }
    public Geraetetyp? Geraetetyp { get; set; }

    public string Bezeichnung { get; set; } = string.Empty;
    public bool IstNeu { get; set; } = true;
    public DateTime? Kaufdatum { get; set; }
    public decimal Listenpreis { get; set; }
    public decimal? Rabatt { get; set; }
    public decimal FinanzierungsBasis { get; set; }
    public decimal? NAK { get; set; }             // Netto-Anschaffungskosten

    public string? Hersteller { get; set; }
    public string? Lieferant { get; set; }
    public string? Seriennummer { get; set; }
    public int? Laufleistung { get; set; }        // km/hours for used equipment
    public string? Bemerkungen { get; set; }

    public bool IstKopie { get; set; }
    public int? OriginalObjektId { get; set; }
}
