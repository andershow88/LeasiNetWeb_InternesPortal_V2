using LeasiNetWeb.Domain.Enums;

namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Core entity. Maps to bm_lnw_leasingapplications.
/// Status is now a proper state machine enum instead of boolean flags.
/// </summary>
public class Leasingantrag : BaseEntity
{
    public string AntragNummer { get; set; } = string.Empty;
    public AntragTyp AntragTyp { get; set; }
    public AntragStatus Status { get; set; } = AntragStatus.Entwurf;

    // Financials
    public decimal Obligo { get; set; }
    public string? Abrechnungsart { get; set; }

    // Rejection
    public int? AblehnungsgrundId { get; set; }
    public Ablehnungsgrund? Ablehnungsgrund { get; set; }
    public DateTime? AbgelehntAm { get; set; }
    public string? AblehnungsKommentar { get; set; }

    // Relationships
    public int? LeasinggesellschaftId { get; set; }
    public Leasinggesellschaft? Leasinggesellschaft { get; set; }

    public int EingereichtVonId { get; set; }
    public Benutzer EingereichtVon { get; set; } = null!;

    public int? SachbearbeiterMBId { get; set; }
    public Benutzer? SachbearbeiterMB { get; set; }

    public int? SachbearbeiterLGId { get; set; }
    public Benutzer? SachbearbeiterLG { get; set; }

    public int? GenehmigerMBId { get; set; }
    public Benutzer? GenehmigerMB { get; set; }

    public bool ZweiteVoteErforderlich { get; set; }
    public int? ZweiteVoteGenehmigerMBId { get; set; }
    public Benutzer? ZweiteVoteGenehmigerMB { get; set; }

    public bool Archiviert { get; set; }
    public DateTime? ArchiviertAm { get; set; }

    public bool KiErstellt { get; set; }

    // Navigation
    public ICollection<Leasingobjekt> Objekte { get; set; } = new List<Leasingobjekt>();
    public ICollection<InternePruefung> InternePruefungen { get; set; } = new List<InternePruefung>();
    public ICollection<Kommentar> Kommentare { get; set; } = new List<Kommentar>();
    public ICollection<Anhang> Anhaenge { get; set; } = new List<Anhang>();
    public ICollection<Ereignis> Ereignisse { get; set; } = new List<Ereignis>();
    public ICollection<Obligo> ObligoEintraege { get; set; } = new List<Obligo>();
    public Vertrag? Vertrag { get; set; }
}
