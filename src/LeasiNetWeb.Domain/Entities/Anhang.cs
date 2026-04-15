using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Unified attachment model. Replaces separate bm_lnw_attachements and
/// bm_lnw_comments_attach tables from Intrexx — both had the same structure.
/// Linked via nullable FKs to the owning entity.
/// </summary>
public class Anhang : BaseEntity
{
    public AnhangTyp Typ { get; set; }
    public string Dateiname { get; set; } = string.Empty;
    public string Dateipfad { get; set; } = string.Empty;
    public string? ContentType { get; set; }
    public long DateigroesseBytes { get; set; }
    public int HochgeladenVonId { get; set; }
    public Benutzer HochgeladenVon { get; set; } = null!;

    // Owning entity (only one FK will be set per record)
    public int? LeasingantragId { get; set; }
    public Leasingantrag? Leasingantrag { get; set; }

    public int? KommentarId { get; set; }
    public Kommentar? Kommentar { get; set; }

    public int? InternePruefungId { get; set; }
    public InternePruefung? InternePruefung { get; set; }

    public int? VertragId { get; set; }
    public Vertrag? Vertrag { get; set; }
}
