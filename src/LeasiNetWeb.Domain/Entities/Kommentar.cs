namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Comment/note on a leasing application. Maps to bm_lnw_comments.
/// Attachments are now unified under Anhang (no separate bm_lnw_comments_attach table).
/// </summary>
public class Kommentar : BaseEntity
{
    public int LeasingantragId { get; set; }
    public Leasingantrag Leasingantrag { get; set; } = null!;

    public int AutorId { get; set; }
    public Benutzer Autor { get; set; } = null!;

    public string Text { get; set; } = string.Empty;
    public bool IstIntern { get; set; }           // Internal-only comments not visible to LG users

    public ICollection<Anhang> Anhaenge { get; set; } = [];
}
