namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Internal message between users (inbox). Maps to bm_lnw_messages.
/// </summary>
public class Nachricht : BaseEntity
{
    public int AbsenderId { get; set; }
    public Benutzer Absender { get; set; } = null!;

    public int EmpfaengerId { get; set; }
    public Benutzer Empfaenger { get; set; } = null!;

    public int? LeasingantragId { get; set; }
    public Leasingantrag? Leasingantrag { get; set; }

    public string Betreff { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public bool Gelesen { get; set; }
    public DateTime? GelesenAm { get; set; }
}
