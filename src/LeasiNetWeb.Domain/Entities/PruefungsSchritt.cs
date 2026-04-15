namespace LeasiNetWeb.Domain.Entities;

/// <summary>
/// Einzelner sequentieller Prüfschritt innerhalb einer internen Kontrolle.
/// Ermöglicht mehrstufige Prüfungen mit verschiedenen Prüfern.
/// Ersetzt die bm_lnw_workflowsteps-Logik aus dem alten Intrexx-System.
/// </summary>
public class PruefungsSchritt : BaseEntity
{
    public int InternePruefungId { get; set; }
    public InternePruefung InternePruefung { get; set; } = null!;

    /// <summary>Reihenfolge: 1 = erster Prüfer, 2 = zweiter usw.</summary>
    public int Sortierung { get; set; }

    /// <summary>z.B. "1. Prüfer", "Supervisor", "Compliance Officer"</summary>
    public string Bezeichnung { get; set; } = string.Empty;

    public int PrueferMBId { get; set; }
    public Benutzer PrueferMB { get; set; } = null!;

    public bool Abgeschlossen { get; set; }
    public DateTime? AbgeschlossenAm { get; set; }
    public string? Ergebnis { get; set; }
}
