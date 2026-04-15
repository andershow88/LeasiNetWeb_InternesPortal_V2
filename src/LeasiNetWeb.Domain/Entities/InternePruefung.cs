namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Internal compliance check for a leasing application.
/// Consolidates bm_lnw_internalchecks, bm_lnw_ic_applications, bm_lnw_ic_obligations.
/// </summary>
public class InternePruefung : BaseEntity
{
    public int LeasingantragId { get; set; }
    public Leasingantrag Leasingantrag { get; set; } = null!;

    /// <summary>Auto-generierte Prüfnummer: {LG-Kürzel}/{Jahr}/{Seq:000}, z.B. "MB-AG/2026/001"</summary>
    public string? PruefungNummer { get; set; }

    /// <summary>Hauptverantwortlicher Prüfer (erster Schritt oder Fallback).</summary>
    public int PrueferMBId { get; set; }
    public Benutzer PrueferMB { get; set; } = null!;

    public bool Abgeschlossen { get; set; }
    public DateTime? AbgeschlossenAm { get; set; }
    public string? Ergebnis { get; set; }

    public ICollection<PruefungsPflicht> Pflichten { get; set; } = new List<PruefungsPflicht>();
    public ICollection<PruefungsSchritt> Schritte { get; set; } = new List<PruefungsSchritt>();
    public ICollection<Anhang> Anhaenge { get; set; } = new List<Anhang>();
}
