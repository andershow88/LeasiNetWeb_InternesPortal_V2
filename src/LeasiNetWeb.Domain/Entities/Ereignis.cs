using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Audit event log for application lifecycle events.
/// Consolidates bm_lnw_events + bm_lnw_events_emails + bm_lnw_events_pushmessages
/// into a single entity with notification tracking flags.
/// </summary>
public class Ereignis : BaseEntity
{
    public EreignisTyp Typ { get; set; }
    public int LeasingantragId { get; set; }
    public Leasingantrag Leasingantrag { get; set; } = null!;

    public int AusgeloestVonId { get; set; }
    public Benutzer AusgeloestVon { get; set; } = null!;

    public string? Beschreibung { get; set; }
    public string? Nutzlast { get; set; }            // JSON for structured event data

    // Notification tracking (replaces separate _emails and _pushmessages tables)
    public bool EmailVersendet { get; set; }
    public DateTime? EmailVersendetAm { get; set; }
    public bool PushVersendet { get; set; }
    public DateTime? PushVersendetAm { get; set; }
}
