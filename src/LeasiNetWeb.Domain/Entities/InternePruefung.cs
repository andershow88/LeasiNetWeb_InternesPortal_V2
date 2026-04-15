namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Internal compliance check for a leasing application.
/// Consolidates bm_lnw_internalchecks, bm_lnw_ic_applications, bm_lnw_ic_obligations.
/// </summary>
public class InternePruefung : BaseEntity
{
    public int LeasingantragId { get; set; }
    public Leasingantrag Leasingantrag { get; set; } = null!;

    public int PrueferMBId { get; set; }
    public Benutzer PrueferMB { get; set; } = null!;

    public bool Abgeschlossen { get; set; }
    public DateTime? AbgeschlossenAm { get; set; }
    public string? Ergebnis { get; set; }

    public ICollection<PruefungsPflicht> Pflichten { get; set; } = new List<PruefungsPflicht>();
    public ICollection<Anhang> Anhaenge { get; set; } = new List<Anhang>();
}
